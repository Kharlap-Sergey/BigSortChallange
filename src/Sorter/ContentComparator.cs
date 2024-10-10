using System;
using System.CommandLine.Parsing;

namespace Sorter;

public class ContentComparator : IComparer<Content>
{
  public static ContentComparator Default = new ContentComparator();
  public int Compare(Content? x, Content? y)
  {
    if (x == null && y == null) return 0;
    if (x == null)
      return -1;
    if (y == null)
      return 1;

    var stringComparisonResult = x.String.CompareTo(y.String);

    return stringComparisonResult == 0
      ? x.Number.CompareTo(y.Number)
      : stringComparisonResult;
  }
}
