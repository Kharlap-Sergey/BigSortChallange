using System.CommandLine;
using System.Reflection;
using System.Text;
using FileGenerationUtil;

var currentDirectory = Directory.GetCurrentDirectory();
var defaultFilepath = @$"{currentDirectory}\default.txt";
string exePath = Assembly.GetExecutingAssembly().Location;
string exeDirectory = Path.GetDirectoryName(exePath);
string defaultInputForSourceGeneration = @$"{exeDirectory}\loremIpsum.txt";
string generatedSourceFilePath = @$"{exeDirectory}\source.txt";

var rootCommand = new RootCommand("Utility to generate file with random data in format 'Number.String'");
var initSourceCommand = new InitCommand(defaultInputForSourceGeneration, generatedSourceFilePath);
rootCommand.AddCommand(initSourceCommand);

var outputFileOption = new Option<string>(
  name: "--output",
  getDefaultValue: () => defaultFilepath,
  description: "The file to save generated content to"
);
var contentSizeOption = new Option<int>(
  name: "--size",
  getDefaultValue: () => 1024 * 1024 * 1024,
  description: "The generated content size in bytes"
);
var sourceFileOption = new Option<string>(
  name: "--source",
  getDefaultValue: () => generatedSourceFilePath,
  description: "The source file for string generation"
);

rootCommand.AddOption(outputFileOption);
rootCommand.AddOption(contentSizeOption);
rootCommand.AddOption(sourceFileOption);

rootCommand.SetHandler(async (string outputPath, int contentSize, string sourceFilePath) =>
{
  if (!File.Exists(sourceFilePath))
  {
    ConsoleHelper.WriteWithColor(() =>
    {
      Console.WriteLine("Source file doesn't exist, specify one with '--source' flag, or create default running 'init' command");
    }, ConsoleColor.Red);
    return;
  }
  try
  {
    await GenerateFile(outputPath, contentSize, sourceFilePath);
    ConsoleHelper.WriteWithColor(() =>
    {
      Console.WriteLine("Content was generated and written to:");
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
}, outputFileOption, contentSizeOption, sourceFileOption);

return await rootCommand.InvokeAsync(args);


async Task GenerateFile(string path, int contentSize, string sourceFilePath)
{
  if (File.Exists(path))
  {
    File.Delete(path);
  }

  var currentSize = 0;
  string textPart;
  string numberPart;
  var source = await File.ReadAllLinesAsync(sourceFilePath);
  IContentGenerator<string> textPartGenerator = new RandomStringGenerator(source);
  IContentGenerator<int> numberPartGenerator = new RandomNumberGenerator();

  using var fs = File.Create(path);
  while (currentSize <= contentSize)
  {
    textPart = textPartGenerator.GetNext();
    numberPart = numberPartGenerator.GetNext().ToString();
    var buffer = UTF8Encoding.UTF8.GetBytes($"{numberPart}.{textPart}\n");
    await fs.WriteAsync(buffer, 0, buffer.Length);
    currentSize += buffer.Length;
  }
}