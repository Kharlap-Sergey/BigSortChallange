using System;

namespace FileGenerationUtil;

public class RandomStringGenerator : IContentGenerator<string>
{
  protected string[] _sourceForGeneration;
  public RandomStringGenerator(IEnumerable<string> sourceForGeneration)
  {
    _sourceForGeneration = sourceForGeneration.ToArray();
  }

  private Random _rand = new();
  public string GetNext()
    {
        int index = _rand.Next(_sourceForGeneration.Length);

        return _sourceForGeneration[index];
    }
}
