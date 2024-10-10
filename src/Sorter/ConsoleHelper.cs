using System;

namespace Sorter;

public static class ConsoleHelper
{
  public static void WriteWithColor(Action action, ConsoleColor consoleColor){
    var temp = Console.ForegroundColor;
    Console.ForegroundColor =  consoleColor;
    action.Invoke();
    Console.ForegroundColor = temp;
  }

  public static void PrintElapsedTime(TimeSpan elapsed, string messagePrefix)
  {
    var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10);
    WriteWithColor(() =>
    {
      Console.WriteLine(messagePrefix + ": " + elapsedTime);
    }, ConsoleColor.Blue);
  }
}
