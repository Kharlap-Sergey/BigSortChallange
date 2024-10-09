using System.Reflection.Metadata.Ecma335;

namespace FileGenerationUtil;

public interface IContentGenerator<T>{
  T GetNext();
}