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
  finally
  {
    stopwatch.Stop();
    ConsoleHelper.PrintElapsedTime(stopwatch.Elapsed, "Run time");
  }
}, outputFileOption, sourceFileOption);

return await rootCommand.InvokeAsync(args);

async Task SortFile(string inputPath, string outputPath, int linesPerChunk, string tempDirectory)
{
  Directory.CreateDirectory(tempDirectory);
  var chunksDirectory = tempDirectory + "\\chunks\\";
  var sortedChunksDirectory = tempDirectory + "\\sorted\\";
  Stopwatch stopwatch = new Stopwatch();

  //read and split by chunks
  stopwatch.Start();
  await SplitByChunks(inputPath, linesPerChunk, chunksDirectory);
  stopwatch.Stop();
  ConsoleHelper.PrintElapsedTime(stopwatch.Elapsed, "Elapsed for chunk creation");

  //sort chunks
  stopwatch.Restart();
  await SortChunks(chunksDirectory, sortedChunksDirectory);
  stopwatch.Stop();
  ConsoleHelper.PrintElapsedTime(stopwatch.Elapsed, "Elapsed for chunk sorting");

  //merge 
  stopwatch.Restart();
  List<IAsyncEnumerator<Content>> sources = new();
  var files = Directory.EnumerateFiles(sortedChunksDirectory);
  foreach (var file in files)
  {
    var reader = ContentReader.ReadFile(file);
    var source = reader.GetAsyncEnumerator();
    if (await source.MoveNextAsync())
      sources.Add(source);
  }
  if (File.Exists(outputPath))
    File.Delete(outputPath);
  var mergeSorter = new MergeSorter<Content>(sources, ContentComparator.Default);
  using var writer = new StreamWriter(outputPath);
  await foreach (var next in mergeSorter.MergeSort())
  {
    await writer.WriteLineAsync(next.ToString());
  }

  stopwatch.Stop();
  ConsoleHelper.PrintElapsedTime(stopwatch.Elapsed, "Elapsed for chunk merging");

  Directory.Delete(tempDirectory, true);
}

async Task SortChunks(string chunksDirectory, string outputDirectory)
{
  Directory.CreateDirectory(outputDirectory);
  var files = Directory.EnumerateFiles(chunksDirectory);
  await Parallel.ForEachAsync(
    files,
    async (file, cancellation) =>
    {
      var fileName = Path.GetFileName(file);
      var output = Path.Combine(outputDirectory, fileName);

      List<Content> contents = new();
      await foreach (var content in ContentReader.ReadFile(file))
      {
        contents.Add(content);
      }
      File.Delete(file);
      contents.Sort(ContentComparator.Default);

      using var writer = new StreamWriter(output);
      foreach (var content in contents)
      {
        await writer.WriteLineAsync(content.ToString());
      }
    }
  );
}

async Task<int> SplitByChunks(string inputPath, int linesPerChunk, string outputDirectory)
{
  Directory.CreateDirectory(outputDirectory);
  int fileIndex = 0;

  using (StreamReader reader = new StreamReader(inputPath))
  {
    while (!reader.EndOfStream)
    {
      string outputFile = Path.Combine(outputDirectory, $"chunk_{fileIndex}.txt");
      using (StreamWriter writer = new StreamWriter(outputFile))
      {
        for (int i = 0; i < linesPerChunk && !reader.EndOfStream; i++)
        {
          string? line = await reader.ReadLineAsync();
          if (line == null)
            continue;
          await writer.WriteLineAsync(line);
        }
      }
      fileIndex++;
    }
  }

  return fileIndex;
}