using System;

namespace Sorter;

public class ContentReader
{
    public static async IAsyncEnumerable<Content> InitRead(string filePath, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(filePath);
        while (!reader.EndOfStream)
        {
          string line = await reader.ReadLineAsync();
          yield return Content.Parse(line);
        }
    }
}
