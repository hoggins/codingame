using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

public class SerialReader
{
  private MemoryStream _buffer;
  private BinaryFormatter _bf = new BinaryFormatter()
  {
    TypeFormat = FormatterTypeStyle.TypesWhenNeeded,
    Binder = new PreMergeToMergedDeserializationBinder()
  };

  public SerialReader(string data)
  {
    using var pipeMs = new MemoryStream(Convert.FromBase64String(data));
    _buffer = new MemoryStream();

    using (var gs = new GZipStream(pipeMs, CompressionMode.Decompress))
      gs.CopyTo(_buffer);
    _buffer.Seek(0, SeekOrigin.Begin);
  }

  public T Read<T>()
  {
    return (T) _bf.Deserialize(_buffer);
  }

  sealed class PreMergeToMergedDeserializationBinder : SerializationBinder
  {
    public override Type BindToType(string assemblyName, string typeName)
    {
      typeName = typeName.Replace("Answer", "APackman");
      assemblyName = assemblyName.Replace("Answer", "APackman");
      return Type.GetType($"{typeName}, {assemblyName}");
    }
  }
}