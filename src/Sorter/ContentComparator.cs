using System;
using System.CommandLine.Parsing;

namespace Sorter;

public class ContentComparator : IComparer<Content>
{
    public int Compare(Content? x, Content? y)
    {
        if (x == y) return 0;

        var stringComparisonResult = x.String.CompareTo(y.String);

        return stringComparisonResult == 0
          ? x.Number.CompareTo(y.Number)
          : stringComparisonResult;
    }
}
