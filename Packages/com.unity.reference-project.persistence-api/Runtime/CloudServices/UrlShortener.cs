using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Unity.ReferenceProject.Persistence
{
   /// <summary>
   /// Converts and manages short representation (GUID + ext) for file paths. 
   /// </summary>
   public class UrlShortener
   {
       string _directory = null;
       const string indexName = "index.json";
       Dictionary<string, string> _shortenPaths = new Dictionary<string, string>();
   
       public UrlShortener(string directory)
       {
           _directory = directory;
           if (!Directory.Exists(_directory))
           {
               Directory.CreateDirectory(_directory);
               _shortenPaths = new Dictionary<string, string>();                
           }
           else
           {
               string indexLocation = $"{_directory}/{indexName}";
               if (File.Exists(indexLocation))
               {
                   string json = File.ReadAllText(indexLocation);
                   _shortenPaths = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
               }
           }
       }
           
       public string ShortenPath(string path)
       {
           string ext = Path.GetExtension(path);
           string guid = Guid.NewGuid().ToString();
               
           if (_shortenPaths.TryGetValue(path, out string value))
               return value;
   
           string shortName = $"{_directory}/{guid}{ext}";
           _shortenPaths.Add(path, shortName);
           SaveToDisk();
           return shortName;
       }
   
       void SaveToDisk()
       {
           if (_shortenPaths == null)
               return;
               
           string json = JsonConvert.SerializeObject(_shortenPaths);
           File.WriteAllText($"{_directory}/{indexName}", json);
       }
   } 
}