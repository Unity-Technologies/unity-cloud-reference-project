using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Unity.Cloud.Common;

namespace Unity.ReferenceProject.Persistence
{
  public class ApiClient
  {
      readonly IHttpClient m_HttpClient;
  
      public ApiClient(IHttpClient httpClient)
      {
          m_HttpClient = httpClient;
      }
  
      public async Task<WebResponse> Query(RequestData data)
      {
          var request = WebRequest.Create(data.Uri);
          request.Method = data.Method.ToString();
          var httpWebRequest = request as HttpWebRequest;
          
          if (data.Headers != null)
          {
              foreach (var header in data.Headers)
              {
                  switch (header.Key)
                  {
                      case "Content-Type":
                          request.ContentType = header.Value;
                          break;
                      case "Content-Length":
                          request.ContentLength = data.RawData.Length;
                          break;
                      case "Accept":
                          if (httpWebRequest != null)
                              httpWebRequest.Accept = header.Value;
                          break;
                      case "Authorization":
                          if (httpWebRequest != null)
                              httpWebRequest.PreAuthenticate = true;
                          request.Headers.Add(header.Key, header.Value);
                          break;
                      default:
                          request.Headers.Add(header.Key, header.Value);
                          break;
                  }
              }
          }
  
          HttpContent content = null;
          if (data.Parameters != null && data.Parameters.Count > 0)
          {
              var paramSet = new List<KeyValuePair<string, string>>();
              foreach (var param in data.Parameters)
                  paramSet.Add(new KeyValuePair<string, string>(param.Key, (string)param.Value));
  
              content = new FormUrlEncodedContent(paramSet);
              content.Headers.ContentType = MediaTypeHeaderValue.Parse(data.ContentType);
          }
  
          if (data.Fields != null && data.Fields.Count > 0 && (data.Files == null || data.Files.Count == 0))
          {
              var paramSet = new List<KeyValuePair<string, string>>();
              foreach (var param in data.Fields)
                  paramSet.Add(new KeyValuePair<string, string>(param.Key, (string)param.Value));
  
              content = new FormUrlEncodedContent(paramSet);
              content.Headers.ContentType = MediaTypeHeaderValue.Parse(data.ContentType);
          }
  
          if (data.Files != null && data.Files.Count > 0)
          {
              var multipartFormDataContent = new MultipartFormDataContent();
              foreach (var param in data.Fields)
                  multipartFormDataContent.Add(new StringContent((string)param.Value), param.Key);
              foreach (var file in data.Files)
              {
                  multipartFormDataContent.Add(new ByteArrayContent(file.Value.Bytes), file.Key, file.Value.Filename);
              }
  
              content = multipartFormDataContent;
          }
  
          if (data.RawData != null)
          {
              content = new ByteArrayContent(data.RawData);
              content.Headers.ContentType = MediaTypeHeaderValue.Parse(data.ContentType);
          }
  
          var result = new WebResponse();
          try
          {
              if (content != null)
              {
                  await using (var stream = await request.GetRequestStreamAsync())
                  {
                      var bytes = await content.ReadAsByteArrayAsync();
                      await stream.WriteAsync(bytes, 0, bytes.Length);
                      stream.Close();
                  }
              }
  
              var response = await request.GetResponseAsync();
              var responseStream = response.GetResponseStream();
              var reader = new StreamReader(responseStream);  
              
              result.DataAsText = await reader.ReadToEndAsync();
              result.Uri = request.RequestUri.AbsoluteUri;
              
              var memStream = new MemoryStream();
              await responseStream.CopyToAsync(memStream);
              result.RawData = memStream.ToArray();
              memStream.Close();
              
              HttpWebResponse httpWebResponse = response as HttpWebResponse;
              if (httpWebResponse != null)
              {
                  result.StatusCode = (int)httpWebResponse.StatusCode;
                  result.Success = httpWebResponse.StatusCode == HttpStatusCode.OK;
                  result.Message = httpWebResponse.StatusDescription;
              }
  
              reader.Close();
          }
          catch (Exception e)
          {
              result.Success = false;
              result.Message = e.Message;
          }
          
          return result;
      }
  }  
}