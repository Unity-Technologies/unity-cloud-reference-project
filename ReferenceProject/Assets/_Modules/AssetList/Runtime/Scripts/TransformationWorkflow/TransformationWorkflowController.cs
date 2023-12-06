using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using UnityEngine;

namespace Unity.ReferenceProject.AssetList
{
    [Serializable]
    class TransformationData
    {
        public string id { get; set; } = null;
        public string userId { get; set; } = null;
        public string assetId { get; set; } = null;
        public string assetVersion { get; set; } = null;
        public string inputDatasetId { get; set; } = null;
        public List<string> inputFiles { get; set; } = null;
        public string outputDatasetId { get; set; } = null;
        public string linkDatasetId { get; set; } = null;
        public string workflowType { get; set; } = null;
        public string jobId { get; set; } = null;
        public string status { get; set; } = null;
        public string errorMessage { get; set; } = null;
        public string progress { get; set; } = null;
        
        public bool IsRunning()
        {
            return status?.ToLower() == "running";
        }
    }
    
    public class TransformationWorkflowController
    {
        readonly IServiceHttpClient m_ServiceHttpClient;
        readonly IServiceHostResolver m_ServiceHostResolver;

        public TransformationWorkflowController(IServiceHttpClient serviceHttpClient, IServiceHostResolver serviceHostResolver)
        {
            m_ServiceHttpClient = serviceHttpClient;
            m_ServiceHostResolver = serviceHostResolver;
        }

        public async Task StartTransformation(IDataset dataset, string file)
        {
            var descriptor = dataset.Descriptor;
            var url = ConstructUrl($"projects/{descriptor.ProjectId}/assets/{descriptor.AssetId}/versions/1/datasets/{descriptor.DatasetId}/transformations/start/3d-data-streaming");

            try
            {
                using var httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Method = HttpMethod.Post;
                httpRequestMessage.RequestUri = new Uri(url);

                var body = "{\"inputFiles\":[\"" + file + "\"]}";

                httpRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");

                using var response = await m_ServiceHttpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
        
        public async Task<string> CurrentTransformations(ProjectId projectId)
        {
            var url = ConstructUrl($"projects/{projectId}/transformations");

            try
            {
               using var httpRequestMessage = new HttpRequestMessage();
               httpRequestMessage.Method = HttpMethod.Get;
               httpRequestMessage.RequestUri = new Uri(url);
   
               using var response = await m_ServiceHttpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead);
               response.EnsureSuccessStatusCode();
   
               return await response.Content.ReadAsStringAsync(); 
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        string ConstructUrl(string path)
        {
            return $"{m_ServiceHostResolver.GetResolvedAddress()}/assets/v1/{path}";
        }
    }
}
