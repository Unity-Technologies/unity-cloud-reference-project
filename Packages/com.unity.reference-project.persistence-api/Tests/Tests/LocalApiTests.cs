using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Unity.Cloud.Common;
using Unity.ReferenceProject.Persistence;
using UnityEngine;

namespace CloudServices.Tests
{
    public class LocalApiTests
    {
        Api api = null;
        string baseUrl;
        string testAuthToken;

        [SetUp]
        public void SetUp()
        {
            baseUrl = $"{Application.persistentDataPath}/local-api";
            api = new Api(baseUrl, testAuthToken, new ApiClient(new DotNetHttpClient()));
        }

        [TearDown]
        public void TearDown()
        {
            var path = $"{baseUrl}/test.json";
            if(File.Exists(path))
                File.Delete(path);
        }

        [Test, Order(2)]
        public async Task GetFile()
        {
            if (!Directory.Exists(baseUrl))
                Directory.CreateDirectory(baseUrl);
            File.WriteAllText($"{baseUrl}/test.json", "Test\r\n");

            WebResponse<string> response = new WebResponse<string>();
            response = await api.GetAsync<string>("/test.json");
            Assert.AreEqual("Test\r\n", response.DataAsText);
        }
        
        [Test, Order(4)]
        public async Task PostFile()
        {
            WebResponse<string> response = new WebResponse<string>();
            
            await api.UserPostAsync<string, string>("/test.json", "TestPost");
            response = await api.GetAsync<string>("/test.json");
            Assert.AreEqual("TestPost", response.Data);
        }
        
        [Test, Order(5)]
        public async Task PatchFile()
        {
            WebResponse<string> response = new WebResponse<string>();
            
            await api.UserPatchAsync<string, string>("/test.json", "TestPatched");
            response = await api.GetAsync<string>("/test.json");
            Assert.AreEqual("TestPatched", response.Data);
        }
        
        [Test, Order(3)]
        public async Task DeleteFile()
        {
            WebResponse<string> response = new WebResponse<string>();
            
            await api.UserDeleteAsync("/test.json");
            response = await api.GetAsync<string>("/test.json");
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task LoadNonExistentFileGracefully()
        {
            WebResponse<string> response = new WebResponse<string>();
            response = await api.GetAsync<string>("/nonexistent.json");
            Assert.IsNull(response.Data);
        }

        [Test, Order(1)]
        public void SaveDataToDevice()
        {
            api.SaveDataToDevice("/test",  "TestData");
            string readText = File.ReadAllText($"{baseUrl}/test.json");
            var jsonStr = JsonConvert.DeserializeObject<string>(readText);
            
            Assert.AreEqual("TestData", jsonStr);
        }

        [Test]
        public void SaveLongFileUsingSaveFile()
        {
            string textToSave = "Test Data\r\n";
            string guid = Guid.NewGuid().ToString();
            string longFile = $"/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}";

            byte[] data = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(textToSave);
                    writer.Close();
                }
                
                data = stream.ToArray();
                stream.Close();
            }

            string path = api.SaveFileToDevice(longFile, data, ".txt");
            byte[] loadedData = File.ReadAllBytes(path);

            CollectionAssert.AreEqual(data, loadedData);
        }

        [Test]
        public void SaveLongFileUsingSaveData()
        {
            string guid = Guid.NewGuid().ToString();
            string longFile = $"/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}/{guid}";

            api.SaveDataToDevice(longFile, new List<string>() { "1", "2", "3" });
            List<string> list = api.LoadDataFromDevice<List<string>>(longFile);

            Assert.IsTrue(list.Contains("1"));
            Assert.IsTrue(list.Contains("2"));
            Assert.IsTrue(list.Contains("3"));
        }
    }
}