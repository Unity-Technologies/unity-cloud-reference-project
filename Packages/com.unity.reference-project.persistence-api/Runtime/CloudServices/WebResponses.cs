namespace Unity.ReferenceProject.Persistence
{
  public class WebResponse<T> : WebResponse
  {
      public T Data = default(T);
  }
  
  public class WebResponse
  {
      public bool Success = false;
      public int StatusCode = 0;
      public string Message = null;
      public string DataAsText = null;
      public string Uri = null;
      public string ContentType = null;
      public byte[] RawData = null;
  
      public void CopyFrom(WebResponse result)
      {
          Message = result.Message;
          Uri = result.Uri;
          StatusCode = result.StatusCode;
          ContentType = result.ContentType;
          DataAsText = result.DataAsText;
          Success = result.Success;
          RawData = result.RawData;
      }
  }
  
  public class ErrorResponse
  {
      public bool Success { get; set; }
      public string Error { get; set; }
  }  
}