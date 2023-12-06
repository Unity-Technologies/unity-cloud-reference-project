using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.ReferenceProject.Persistence
{
    public class RequestData
    {
        public string Uri = null;
    
        public byte[] RawData = null;
        public HttpMethod Method = HttpMethod.GET;
        public Dictionary<string, string> Headers = null;
        public Dictionary<string, object> Fields = null;
        public Dictionary<string, object> Parameters = null;
        public Dictionary<string, FileField> Files = null;
        public TimeSpan Timeout = TimeSpan.FromSeconds(60);
    
        public string ContentType
        {
            get => GetHeaderValue("Content-Type");
            set => AddHeader("Content-Type", value);
        }
    
        public string ContentRange
        {
            get => GetHeaderValue("Content-Range");
            set => AddHeader("Content-Range", value);
        }
    
        public string Accept
        {
            get => GetHeaderValue("Accept");
            set => AddHeader("Accept", value);
        }
    
        public string ContentLength => RawData != null ? RawData.Length.ToString() : "0";
    
        public bool IsContentInBody => Method == HttpMethod.POST || Method == HttpMethod.PATCH;
    
        public RequestData()
        {
        }
    
        public RequestData(Uri uri, HttpMethod method)
        {
            Uri = uri.AbsoluteUri;
            Method = method;
        }
    
        public RequestData(string uri, HttpMethod method)
        {
            Uri = uri;
            Method = method;
        }
    
        public string GetHeaderValue(string key)
        {
            return !Headers.ContainsKey(key) ? null : Headers[key];
        }
    
        public void AddHeader(string key, string value)
        {
            if (Headers == null)
                Headers = new Dictionary<string, string>();
    
            Headers[key] = value;
        }
    
        public void AddField(string key, object value)
        {
            if (Fields == null)
                Fields = new Dictionary<string, object>();
    
            Fields[key] = value;
        }
    
        public void AddParameter(string key, object value)
        {
            if (Parameters == null)
                Parameters = new Dictionary<string, object>();
    
            Parameters[key] = value;
        }
    
        public void AddFile(string key, FileField file)
        {
            if (Files == null)
                Files = new Dictionary<string, FileField>();
    
            Files[key] = file;
        }
    
        public new string ToString()
        {
            return
                $"[{Method.ToString()}] URI {Uri}: Headers <{Headers}>, Fields <{Fields}>, Data <{Encoding.UTF8.GetString(RawData)}>";
        }
    
        public enum HttpMethod
        {
            GET,
            POST,
            PATCH,
            PUT,
            DELETE
        }
    }
}