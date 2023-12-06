using System;

namespace Unity.ReferenceProject.Persistence
{
  [Serializable]
  public class FileField
  {
      public byte[] Bytes { get; set; }
      public string Filename { get; set; }
      public string ContentType { get; set; }
      public string Url { get; set; }
  
      public FileField()
      {
      }
  
      public FileField(byte[] bytes, string filename, string contentType = "image/png")
      {
          Bytes = bytes;
          Filename = filename;
          ContentType = contentType;
      }
  }  
}