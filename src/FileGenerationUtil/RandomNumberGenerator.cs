using System;

namespace FileGenerationUtil;

public class RandomNumberGenerator : IContentGenerator<int>
{
    private Random _rand = new();

    public int GetNext()
    {
        return _rand.Next();
    }
}
