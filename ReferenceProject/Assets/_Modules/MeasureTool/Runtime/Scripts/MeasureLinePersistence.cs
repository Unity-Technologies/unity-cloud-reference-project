using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.ReferenceProject.Persistence;

namespace Unity.ReferenceProject.MeasureTool
{
    public class MeasureLinePersistence
    {
        const string k_MeasureDirectory = "measure-tool/lines";
        readonly Api m_Api;
        readonly string m_BaseUri;

        public MeasureLinePersistence(string baseUri)
        {
            m_BaseUri = baseUri;
            m_Api = new Api(baseUri, "", new ApiClient(new DotNetHttpClient()));
        }

        public async Task<WebResponse<List<MeasureLineData>>> GetLines(DatasetDescriptor descriptor)
        {
            var uri = GetUri(descriptor);
            return await GetLines(uri);
        }

        public async Task<WebResponse<List<MeasureLineData>>> GetLines(string uri)
        {
            return await m_Api.GetAsync<List<MeasureLineData>>(uri);
        }

        public async Task<WebResponse> SaveLine(DatasetDescriptor descriptor, MeasureLineData line)
        {
            var uri = GetUri(descriptor);
            var lineCollectionResponse = await GetLines(uri);

            var lineCollection = new List<MeasureLineData>();
            if (lineCollectionResponse.Data != null)
            {
                lineCollection = lineCollectionResponse.Data.ToList();
            }

            if (!lineCollection.Contains(line))
            {
                lineCollection.Add(line);
            }
            else
            {
                var index = lineCollection.IndexOf(line);
                lineCollection[index] = line;
            }

            return await m_Api.UserPostAsync<List<MeasureLineData>, MeasureLineData>(uri, lineCollection);
        }

        public async Task<WebResponse> DeleteLine(DatasetDescriptor descriptor, MeasureLineData line)
        {
            var uri = GetUri(descriptor);
            var lineCollectionResponse = await GetLines(uri);

            var lineCollection = new List<MeasureLineData>();

            if (lineCollectionResponse.Data != null)
            {
                lineCollection = lineCollectionResponse.Data.ToList();
            }

            if (lineCollection.Contains(line))
            {
                lineCollection.Remove(line);
            }

            return await m_Api.UserPostAsync<List<MeasureLineData>, MeasureLineData>(uri, lineCollection);
        }

        string GetUri(DatasetDescriptor descriptor)
        {
            return $"{m_BaseUri}/{descriptor.OrganizationId.ToString()}/{descriptor.ProjectId.ToString()}/{descriptor.AssetId.ToString()}/{k_MeasureDirectory}/lines.json";
        }
    }
}