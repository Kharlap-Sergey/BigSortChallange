using System.CommandLine;
using System.Diagnostics;
using Sorter;

var currentDirectory = Directory.GetCurrentDirectory();
var defaultFilepath = @$"{currentDirectory}\sorted.txt";
// var tempPath = @$".\temp\{DateTime.UtcNow.ToString("fffffff")}\";
var tempPath = @$".\temp\3226675\";
var rootCommand = new RootCommand("The Program to sort big file");
int chunkSize = 1_000_000;
// int chunkSize = 10;

var outputFileOption = new Option<string>(
  name: "--output",
  getDefaultValue: () => defaultFilepath,
  description: "The file to save generated content to."
);
var sourceFileOption = new Option<string>(
  name: "--input",
  //getDefaultValue: () => "C:\\Users\\serge\\Projects\\BigSortChallange\\src\\Sorter\\test1.txt",
  description: "The Source file which should be sorted."
)
{
  IsRequired = true
};

rootCommand.AddOption(outputFileOption);
rootCommand.AddOption(sourceFileOption);

rootCommand.SetHandler(async (string outputPath, string sourceFilePath) =>
{
  if (!File.Exists(sourceFilePath))
  {
    ConsoleHelper.WriteWithColor(() =>
    {
      Console.WriteLine("Source file doesn't exist, specify one with '--input' flag");
    }, ConsoleColor.Red);
    return;
  }

  Stopwatch stopwatch = new Stopwatch();
  try
  {
    stopwatch.Start();
    await SortFile(sourceFilePath, outputPath, chunkSize, tempPath);
    ConsoleHelper.WriteWithColor(() =>
    {
      Console.WriteLine("Sorting result:");
      Console.WriteLine(outputPath);
    }, ConsoleColor.Green);
  }
  catch (Exception ex)
  {
    ConsoleHelper.WriteWithColor(() =>
    {
      Console.WriteLine("Something went wrong: {0}", ex.Message);
    }, ConsoleColor.Red);
  }
  finally{
    stopwatch.Stop();
    TimeSpan ts = stopwatch.Elapsed;
    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
    ConsoleHelper.WriteWithColor(() =>
    {
      Console.WriteLine("RunTime " + elapsedTime);
    }, ConsoleColor.Blue);
  }
}, outputFileOption, sourceFileOption);

return await rootCommand.InvokeAsync(args);


async Task SortFile(string inputPath, string outputPath, int linesPerChunk, string tempDirectory)
{
  //read and split by chunks
  Directory.CreateDirectory(tempDirectory);

  int fileIndex = 0;
  using (StreamReader reader = new StreamReader(inputPath))
  {
    while (!reader.EndOfStream)
    {
      string outputFile = Path.Combine(tempDirectory, $"input_chunk_{fileIndex}.txt");
      using (StreamWriter writer1 = new StreamWriter(outputFile))
      {
        for (int i = 0; i < linesPerChunk && !reader.EndOfStream; i++)
        {
          string? line = await reader.ReadLineAsync();
          if(line == null)
            continue;
          await writer1.WriteLineAsync(line);
        }
      }
      fileIndex++;
    }
  }

  //sort chunks
  var comp = new ContentComparator();
  var chunkIndexes = Enumerable.Range(0, fileIndex);
  await Parallel.ForEachAsync(
    chunkIndexes,
    async (index, cancellation) => {
      var path = Path.Combine(tempDirectory, $"input_chunk_{index}.txt");
      var output = Path.Combine(tempDirectory, $"sorted_chunk_{index}.txt");

      List<Content> contents = new();
      using var reader = new StreamReader(path);
      while (!reader.EndOfStream)
      {
        string? line = await reader.ReadLineAsync();
        if(line == null)
          continue;

        contents.Add(Content.Parse(line));
      }
      contents.Sort(comp);

      using var writer2 = new StreamWriter(output);
      foreach(var content in contents){
        await writer2.WriteLineAsync(content.ToString());
      }
    }
  );
  
  //merge 
  List<IAsyncEnumerator<Content>> sources = new();
  for (int i = 0; i < fileIndex; i++){
    var input = Path.Combine(tempDirectory, $"sorted_chunk_{i}.txt");
    var reader = ContentReader.InitRead(input);
    var source = reader.GetAsyncEnumerator();
    if(await source.MoveNextAsync())
      sources.Add(source);
  }
  
  if (File.Exists(outputPath))
  {
    File.Delete(outputPath);
  }

  var mergeSorter = new MergeSorter<Content>(sources, comp);
  using var writer = new StreamWriter(outputPath);
  await foreach(var next in mergeSorter.MergeSort()){
    await writer.WriteLineAsync(next.ToString());
  }

  Directory.Delete(tempDirectory, true);
}