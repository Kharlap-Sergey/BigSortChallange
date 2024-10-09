using System;

namespace Sorter;

public class MergeSorter<T>
{
  PriorityQueue<IAsyncEnumerator<T>, T> _priorityQueue;
  public MergeSorter(
    IReadOnlyCollection<IAsyncEnumerator<T>> sources,
    IComparer<T> comparer
  )
  {
    _priorityQueue = new(comparer);
    foreach(var source in sources){
      if(source.Current != null)
      _priorityQueue.Enqueue(source, source.Current);
    }
  }

  public async IAsyncEnumerable<T> MergeSort(){
    while(_priorityQueue.Count > 0){
      var source = _priorityQueue.Dequeue();
      var res = source.Current;
      if(await source.MoveNextAsync() && res != null){
        _priorityQueue.Enqueue(source, source.Current);
      }
      yield return res;
    }
  }
}
