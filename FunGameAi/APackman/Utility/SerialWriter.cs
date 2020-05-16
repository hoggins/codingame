using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

public class SerialWriter
{
  private MemoryStream _buffer;
  private BinaryFormatter _bf = new BinaryFormatter();

  public SerialWriter()
  {
    _buffer = new MemoryStream();
  }

  public void Write<T>(T obj)
  {
    _bf.Serialize(_buffer, obj);
  }

  public string Flush()
  {
    _buffer.Seek(0, SeekOrigin.Begin);

    using (var cs = new MemoryStream())
    {

      using (var gs = new GZipStream(cs, CompressionMode.Compress))
        _buffer.CopyTo(gs);

      _buffer.Seek(0, SeekOrigin.Begin);
      _buffer.SetLength(0);

      return Convert.ToBase64String(cs.ToArray());
    }
  }
}
