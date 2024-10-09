using System;
using System.CommandLine;
using System.Text;

namespace FileGenerationUtil;

public class InitCommand : Command
{
  public InitCommand(string defaultInputForSourceGeneration, string sourceFilePath)
    : base("init", "Init the source file to be used during the generation")
  {
    var inputTextPathOption = new Option<string>(
    name: "--input",
    getDefaultValue: () => defaultInputForSourceGeneration,
    description: "Text file path to be used for source generation."
);

    this.AddOption(inputTextPathOption);
    this.SetHandler(async (string input) =>
    {
      try
      {
        await InitSource(input, sourceFilePath);
        ConsoleHelper.WriteWithColor(() =>
        {
          Console.WriteLine("Source file:");
          Console.WriteLine(sourceFilePath);
        }, ConsoleColor.Green);
      }
      catch (Exception ex)
      {
        ConsoleHelper.WriteWithColor(() =>
        {
          Console.WriteLine("Something went wrong: {0}", ex.Message);
        }, ConsoleColor.Red);
      }

    }, inputTextPathOption);
  }

  private static async Task InitSource(string inputPath, string outputPath)
  {
    if (File.Exists(outputPath))
    {
      File.Delete(outputPath);
    }

    var rand = new Random();
    using var input = File.OpenRead(inputPath);
    using var output = File.Create(outputPath);

    while (true)
    {
      var m = rand.Next(5, 25);
      var buffer = new byte[m];
      var readCount = await input.ReadAsync(buffer, 0, m);
      if (readCount == 0)
      {
        break;
      }

      string result = Encoding.UTF8.GetString(buffer, 0, readCount);
      var escaped = result.Replace("\n", " ").Trim() + "\n";
      if (escaped.Length < 5)
        continue;
      buffer = Encoding.UTF8.GetBytes(escaped);

      await output.WriteAsync(buffer);
    }
  }
}
