using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using UnityEngine;

namespace Unity.ReferenceProject.DataStreaming
{
    public static class StreamableAssetHelper
    {
        static readonly string k_StreamableTag = "Streamable";
        
        public static async Task<IDataset> FindStreamableDataset(IAsset asset)
        {
            IDataset streamableDataset = null;

            try
            {
                streamableDataset = await FindStreamableDatasetInternal(asset, CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (streamableDataset is null)
            {
                Debug.LogError($"The {nameof(IAsset)} {asset.Name} does not contain a streamable {nameof(IDataset)}.");
            }

            return streamableDataset;
        }

        static async Task<IDataset> FindStreamableDatasetInternal(IAsset asset, CancellationToken cancellationToken = default)
        {
            var datasets = asset.ListDatasetsAsync(Range.All, cancellationToken);

            await foreach (var dataset in datasets.WithCancellation(cancellationToken))
            {
                if (dataset.SystemTags.Contains(k_StreamableTag))
                {
                    return dataset;
                }
            }

            // Temporary support for manually tagged assets
            if (asset.Tags.Contains(k_StreamableTag))
            {
                await foreach (var dataset in datasets.WithCancellation(cancellationToken))
                {
                    if (dataset != null)
                    {
                        return dataset;
                    }
                }
            }

            return null;
        }

        public static async Task<bool> IsStreamable(IAsset asset)
        {
            var dataset = await FindStreamableDatasetInternal(asset);
            return dataset != null;
        }
        
        public static async Task IsStreamable(IAsset asset, Action<bool> isStreamableCallback)
        {
            var result = await IsStreamable(asset);
            isStreamableCallback?.Invoke(result);
        }

        public static void ApplyFilter(AssetSearchFilter assetSearchFilter)
        { 
            assetSearchFilter.Datasets.SystemTags.Include(k_StreamableTag);   
        }
        
        public static void RemoveFilter(AssetSearchFilter assetSearchFilter)
        {
            assetSearchFilter.Datasets.SystemTags.Clear();
        }
    }
}
