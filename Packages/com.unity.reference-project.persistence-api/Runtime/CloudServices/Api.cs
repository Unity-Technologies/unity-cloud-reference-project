using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.ReferenceProject.Persistence
{
  public class Api
  {
      public const int MaxPath = 256;
      private UrlShortener _urlShortener = null;
  
      /// <summary>
      /// The user's token information for the Connector
      /// </summary>
      readonly string authToken;
  
      /// <summary>
      /// Serializer settings to house custom JSON conversion instructions
      /// </summary>
      public JsonSerializerSettings SerializerSettings { get; private set; }
  
      /// <summary>
      /// The REST client used to make these API calls 
      /// </summary>
      public ApiClient client { get; set; }
  
      /// <summary>
      /// The Connector's base server URL used for API calls
      /// </summary>
      public string BaseUrl { get; set; }
      
      /// <summary>
      /// File extension to be used for save to device
      /// </summary>
      public string Extension { get; set; } = "json";
      
      /// <summary>
      /// Is this api targeting the web?
      /// </summary>
      public bool IsHttp => BaseUrl.Contains("http");
  
      /// <summary>
      /// Constructor, Creates a new api instance
      /// </summary>
      /// <param name="baseUrl">base server URL</param>
      /// <param name="auth">user auth tokens</param>
      /// <param name="client">rest client used for internet calls</param>
      /// <param name="settings">custom Serializer settings if any, Will overwrite any base settings</param>
      public Api(string baseUrl, string authToken, ApiClient client, JsonSerializerSettings jsonSettings = null)
      {
          this.authToken = authToken;
          this.client = client;
          
          SerializerSettings = jsonSettings ?? new JsonSerializerSettings
          {
              Converters = new JsonConverter[]
              {
                  new Vector3Converter(),
                  new Vector2Converter(),
                  new QuaternionConverter(),
                  new ColorValueConverter(),
              }
          };
          
          BaseUrl = baseUrl;
          EnsureDirectory(baseUrl);
          _urlShortener = new UrlShortener(Application.persistentDataPath + "/Links");
      }
  
      void EnsureDirectory(string path)
      {
          if (IsHttp)
              return;
          
          if (!Directory.Exists(path))
          {
              Directory.CreateDirectory(path);
          }
      }
  
      /// <summary>
      /// Creates a usable URI using the supplied HREF and the API's base URL
      /// </summary>
      /// <param name="href">API hyper reference minus the base URL</param>
      /// <returns></returns>
      private string CreateUri(string href)
      {
          if (string.IsNullOrEmpty(href))
              return null;
          
          //Check if the uri is an href
          string uri = href;
          if (!uri.Contains(BaseUrl))
              uri = $"{BaseUrl}{uri}";
  
          if (IsHttp) 
              return uri;
          
          var path = Path.GetDirectoryName(uri);
          EnsureDirectory(path);
  
          return uri;
      }
  
      /// <summary>
      /// A GET wrapper request for an HREF that does not require user authentication.
      /// </summary>
      public async Task<WebResponse<T>> GetAsync<T>(string href)
      {
          string uri = CreateUri(href);
          return await BaseRequestAsync<T>(uri);
      }
  
      /// <summary>
      /// A GET wrapper request for an HREF that requires user authentication.
      /// </summary>
      public async Task<WebResponse> UserGetAsync(string href)
      {
          string uri = CreateUri(href);
          return await UserRequestAsync(uri, RequestData.HttpMethod.GET);
      }
  
      /// <summary>
      /// A GET wrapper request for an HREF that requires user authentication. Parses response to supplied generic.
      /// </summary>
      public async Task<WebResponse<T>> UserGetAsync<T>(string href)
      {
          string uri = CreateUri(href);
          return await UserRequestAsync<T>(uri, RequestData.HttpMethod.GET);
      }
  
      /// <summary>
      /// A PATCH wrapper request for an HREF and file bytes that requires user authentication.
      /// </summary>
      public async Task<WebResponse> UserPatchAsync(string href, byte[] rawData)
      {
          string uri = CreateUri(href);
          return await UserUploadRequestAsync(uri, rawData, null, RequestData.HttpMethod.PATCH);
      }
  
      /// <summary>
      /// A PATCH wrapper request for an HREF and object that requires user authentication. Parses response to supplied generic.
      /// </summary>
      public async Task<WebResponse> UserPatchAsync<T>(string href, T dataToSerialize)
      {
          string uri = CreateUri(href);
          RequestData requestData = CreateUploadRequest(uri, dataToSerialize, null, RequestData.HttpMethod.PATCH);
          return await UserRequestAsync(requestData);
      }
  
      /// <summary>
      /// A POST wrapper request for an HREF and object that requires user authentication. Parses response to supplied generic.
      /// </summary>
      public async Task<WebResponse<T>> UserPatchAsync<TData, T>(string href, TData data,
          string contentType = "application/json")
      {
          string uri = CreateUri(href);
  
          Dictionary<string, string> headers = new Dictionary<string, string>()
          {
              { "Content-Type", contentType }
          };
  
          RequestData requestData = CreateUploadRequest(uri, data, headers, RequestData.HttpMethod.PATCH);
          return await UserUploadRequestAsync<T>(requestData);
      }
  
      public async Task<WebResponse<T>> UserPostAsync<TData, T>(string href, TData data,
          string contentType = "application/json")
      {
          string uri = CreateUri(href);
  
          Dictionary<string, string> headers = new Dictionary<string, string>()
          {
              { "Content-Type", contentType }
          };
  
          RequestData requestData = CreateUploadRequest(uri, data, headers, RequestData.HttpMethod.POST);
          return await UserUploadRequestAsync<T>(requestData);
      }
  
      /// <summary>
      /// A PUT wrapper request for an HREF and object that requires user authentication. Parses response to supplied generic.
      /// </summary>
      public async Task<WebResponse<T>> UserPutAsync<TData, T>(string href, TData data,
          string contentType = "application/json")
      {
          string uri = CreateUri(href);
  
          Dictionary<string, string> headers = new Dictionary<string, string>()
          {
              { "Content-Type", contentType }
          };
  
          RequestData requestData = CreateUploadRequest(uri, data, headers, RequestData.HttpMethod.PUT);
          return await UserUploadRequestAsync<T>(requestData);
      }
  
      /// <summary>
      /// A PUT wrapper request for an HREF and file parameters that requires user authentication. Parses response to supplied generic.
      /// </summary>
      public async Task<WebResponse<T>> UserPutAsync<TData, T>(string href, Dictionary<string, object> parameters,
          string contentType = "application/json")
      {
          string uri = CreateUri(href);
  
          Dictionary<string, string> headers = new Dictionary<string, string>()
          {
              { "Content-Type", contentType }
          };
  
          RequestData requestData = CreateUploadRequest(uri, headers, RequestData.HttpMethod.PUT);
  
          if (parameters != null)
              requestData.Parameters = parameters;
  
          return await UserUploadRequestAsync<T>(requestData);
      }
  
      /// <summary>
      /// A DELETE request for an HREF that requires user authentication.
      /// </summary>
      public async Task<WebResponse> UserDeleteAsync(string href)
      {
          if (!IsHttp)
          {
              var path = $"{BaseUrl}/{href}";
              Debug.Log("api path " + path);
              if(File.Exists(path))
                  File.Delete(path);
              return new WebResponse();
          }
          
          string uri = CreateUri(href);
          return await UserRequestAsync(uri, RequestData.HttpMethod.DELETE);
      }
  
      /// <summary>
      /// Base upload request for request data that requires user authentication and includes generic deserialization
      /// </summary>
      /// <param name="requestData"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      public async Task<WebResponse<T>> UserUploadRequestAsync<T>(RequestData requestData)
      {
          var result = await UserRequestAsync(requestData);
          return ParseResponse<T>(result);
      }
  
      /// <summary>
      /// Creates Request data from the supplied parameters for api requests.
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="headers"></param>
      /// <param name="method"></param>
      /// <param name="rawData"></param>
      /// <returns></returns>
      public RequestData CreateUploadRequest(string uri, Dictionary<string, string> headers,
          RequestData.HttpMethod method,
          byte[] rawData = null)
      {
          RequestData requestData = new RequestData
          {
              Uri = uri,
              RawData = rawData,
              Method = method,
              Headers = headers
          };
  
          if (requestData.Headers != null && requestData.Headers.ContainsKey("Content-Type"))
              requestData.ContentType = requestData.Headers["Content-Type"];
  
          return requestData;
      }
  
      /// <summary>
      /// Creates Request data from the supplied object and parameters for api requests.
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="data"></param>
      /// <param name="headers"></param>
      /// <param name="method"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      private RequestData CreateUploadRequest<T>(string uri, T data, Dictionary<string, string> headers,
          RequestData.HttpMethod method)
      {
          byte[] rawData = null;
          if (data.GetType() == typeof(byte[]))
              rawData = data as byte[];
          else
          {
              string serializeObject = JsonConvert.SerializeObject(data, SerializerSettings);
              rawData = Encoding.UTF8.GetBytes(serializeObject);
          }
  
          return CreateUploadRequest(uri, headers, method, rawData);
      }
  
      /// <summary>
      /// Parses webResponse's DataAsText into data objects from the supplied generic 
      /// </summary>
      /// <param name="baseResponse"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      private WebResponse<T> ParseResponse<T>(WebResponse baseResponse)
      {
          WebResponse<T> response = new WebResponse<T>();
          response.CopyFrom(baseResponse);
  
          try
          {
              response.Data = JsonConvert.DeserializeObject<T>(baseResponse.DataAsText, SerializerSettings);
          }
          catch (Exception e)
          {
              response.Success = false;
              string err =
                  $"[ApiBase<{typeof(T)}>] Exception thrown deserializing: {e} URI {response.Uri} Data String: {response.DataAsText}";
  
              response.Message = err;
          }
  
          return response;
      }
  
      /// <summary>
      ///  Base upload request for file bytes that requires user authentication
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="rawData"></param>
      /// <param name="onResponse"></param>
      /// <param name="method"></param>
      /// <param name="timeoutSeconds"></param>
      /// <returns></returns>
      public async Task<WebResponse> UserUploadRequestAsync(string uri, byte[] rawData, Action<string> onResponse = null,
          RequestData.HttpMethod method = RequestData.HttpMethod.POST, float timeoutSeconds = 60)
      {
          RequestData requestData = new RequestData()
          {
              Uri = uri,
              Method = method,
              RawData = rawData,
              Timeout = TimeSpan.FromSeconds(timeoutSeconds)
          };
  
          return await UserRequestAsync(requestData, onResponse);
      }
  
      /// <summary>
      /// Base upload request for FileField objects uploaded as form fields.
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="jsonData"></param>
      /// <param name="fileField"></param>
      /// <param name="method"></param>
      /// <returns></returns>
      public async Task<WebResponse<T>> UserUploadRequestAsync<T>(string uri, string jsonData, FileField fileField,
          RequestData.HttpMethod method = RequestData.HttpMethod.POST)
      {
          RequestData requestData = new RequestData()
          {
              Uri = uri,
              Method = method,
              RawData = Encoding.UTF8.GetBytes(jsonData),
              Timeout = TimeSpan.FromSeconds(60)
          };
  
          if (fileField != null)
              requestData.Files = new Dictionary<string, FileField>() { { fileField.Filename, fileField } };
  
          var response = await UserRequestAsync(requestData);
          return ParseResponse<T>(response);
      }
  
      /// <summary>
      /// Base upload request for FileField objects uploaded as form fields.
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="formFields"></param>
      /// <param name="fileFieldName"></param>
      /// <param name="fileField"></param>
      /// <param name="contentType"></param>
      /// <param name="method"></param>
      /// <returns></returns>
      public async Task<WebResponse<T>> UserUploadRequestAsync<T>(string uri, Dictionary<string, object> formFields,
          string fileFieldName, FileField fileField, string contentType,
          RequestData.HttpMethod method = RequestData.HttpMethod.POST)
      {
          RequestData requestData = new RequestData()
          {
              Uri = uri,
              Method = method,
              Fields = formFields,
              Files = new Dictionary<string, FileField>() { { fileFieldName, fileField } },
              Timeout = TimeSpan.FromSeconds(60),
              ContentType = contentType
          };
  
          return await UserUploadRequestAsync<T>(requestData);
      }
  
      /// <summary>
      /// User request wrapper for a full URL and HTTP method
      /// NOTE: This will not build a URL using the api's baseURL
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="method"></param>
      /// <returns></returns>
      private async Task<WebResponse> UserRequestAsync(string uri,
          RequestData.HttpMethod method = RequestData.HttpMethod.GET)
      {
          RequestData requestData = new RequestData()
          {
              Uri = uri,
              Method = method,
          };
  
          return await UserRequestAsync(requestData);
      }
  
      /// <summary>
      /// User request wrapper for a full URL and HTTP method. Parses response to supplied generic.
      /// NOTE: This will not build a URL using the api's baseURL
      /// </summary>
      /// <param name="uri"></param>
      /// <param name="method"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      private async Task<WebResponse<T>> UserRequestAsync<T>(string uri,
          RequestData.HttpMethod method = RequestData.HttpMethod.GET)
      {
          RequestData requestData = new RequestData()
          {
              Uri = uri,
              Method = method,
          };
  
          var result = await UserRequestAsync(requestData);
          return ParseResponse<T>(result);
      }
  
      /// <summary>
      /// Base user request that finalizes requestData and sends it to the BaseRequest
      /// </summary>
      /// <param name="requestData"></param>
      /// <param name="onResponse"></param>
      /// <returns></returns>
      public async Task<WebResponse> UserRequestAsync(RequestData requestData, Action<string> onResponse = null)
      {
          //Check if the uri is an href
          if (!requestData.Uri.Contains(BaseUrl))
              requestData.Uri = CreateUri(requestData.Uri);
  
          //Initialize the headers
          if (requestData.Headers == null)
              requestData.Headers = new Dictionary<string, string>();
  
          if (requestData.Fields == null)
              requestData.Fields = new Dictionary<string, object>();
  
          if (requestData.Parameters == null)
              requestData.Parameters = new Dictionary<string, object>();
  
          if (!requestData.Headers.ContainsKey("Content-Type"))
              requestData.AddHeader("Content-Type", "application/json");
  
          //Add the v3 headers
          if (!requestData.Headers.ContainsKey("Accept"))
              requestData.AddHeader("Accept", "*/*");
  
          if (!string.IsNullOrEmpty(authToken) && !requestData.Headers.ContainsKey("Authorization"))
              requestData.AddHeader("Authorization", $"Bearer {authToken}");
  
          return await BaseRequestAsync(requestData, onResponse);
      }
  
      /// <summary>
      /// Wrapper request that sends request data and executes an action after the response is received. 
      /// </summary>
      /// <param name="requestData"></param>
      /// <param name="onResponse"></param>
      /// <returns></returns>
      public async Task<WebResponse> BaseRequestAsync(RequestData requestData, Action<string> onResponse = null)
      {
          var response = await BaseRequestAsync(requestData);
          if (response.DataAsText != null)
              onResponse?.Invoke(response.DataAsText);
  
          return response;
      }
  
      /// <summary>
      /// Wrapper request that accepts a URL for requestData and sends.
      /// NOTE: This will not build a URL using the api's baseURL
      /// </summary>
      /// <param name="uri"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      public async Task<WebResponse<T>> BaseRequestAsync<T>(string uri)
      {
          RequestData requestData = new RequestData() { Uri = uri };
          return await BaseRequestAsync<T>(requestData);
      }
  
      /// <summary>
      /// Base request for all api requests sending out requestData. Parses response to supplied generic.
      /// </summary>
      /// <param name="requestData"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      public async Task<WebResponse<T>> BaseRequestAsync<T>(RequestData requestData)
      {
          var response = await BaseRequestAsync(requestData);
          return ParseResponse<T>(response);
      }
  
      /// <summary>
      /// Base request for all api requests sending out requestData.
      /// </summary>
      /// <param name="requestData"></param>
      /// <returns></returns>
      protected async Task<WebResponse> BaseRequestAsync(RequestData requestData)
      {
          WebResponse result = new WebResponse();
          result = await client.Query(requestData);
  
          if (result != null)
              return result;
  
  
          result.DataAsText = "Please check your internet connection.";
          result.Message = result.DataAsText;
          return result;
      }
  
      public void SaveDataToDevice<T>(string href, T data)
      {
          //Do not save to a web URL
          if (BaseUrl.Contains("https") || BaseUrl.Contains("http"))
              return;
  
          //save the index to the device
          string json = JsonConvert.SerializeObject(data, SerializerSettings);
          string path = $"{BaseUrl}{href}.{Extension}";
          path = TryShortenPath(path);
  
          //Create the directory and file
          Debug.Log(path);
          string dir = Path.GetDirectoryName(path);
          Directory.CreateDirectory(dir);
          File.WriteAllText(path, json);
      }
  
      public T LoadDataFromDevice<T>(string href, string fileExtension = null)
      {
          //Do not attempt to load from cloud
          if (IsHttp)
              return default;
  
          if (string.IsNullOrEmpty(fileExtension))
              fileExtension = Extension;
          
          string path = $"{BaseUrl}{href}.{fileExtension}";
          path = TryShortenPath(path);
  
          if (!Directory.Exists(Path.GetDirectoryName(path)) || !File.Exists(path))
              return default;
  
          string jsonStr = File.ReadAllText(path);
          return JsonConvert.DeserializeObject<T>(jsonStr, SerializerSettings);
      }
  
      public string SaveFileToDevice(string href, byte[] file, string extension = null)
      {
          //Do not save to a web URL
          if (IsHttp)
              return null;
  
          //save the index to the device
          string path = $"{BaseUrl}{href}";
          if (!string.IsNullOrEmpty(extension) && !path.EndsWith(extension))
              path += extension;
          path = TryShortenPath(path);
  
          //Create the directory and file
          string dir = Path.GetDirectoryName(path);
          Directory.CreateDirectory(dir);
          File.WriteAllBytes(path, file);
          return path;
      }
  
      private string TryShortenPath(string path)
      {
          //Shorten the path if it exceeds the length
          if (_urlShortener != null && path.Length >= MaxPath)
              path = _urlShortener.ShortenPath(path);
  
          return path;
      }
  }  
}