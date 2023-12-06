using System;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.ReferenceProject.AssetList
{
    public static class ThumbnailController
    {
        static readonly int k_TimeoutDelay = 10000;

        class ThumbnailDownloadEntry
        {
            public bool IsDownloading;
            public Texture2D Texture2D;
            public readonly List<Action<Texture2D>> Listeners = new ();
        }

        static Dictionary<string, ThumbnailDownloadEntry> s_ThumbnailCache = new ();

        public static async Task GetThumbnail(IAsset asset, int width, Action<Texture2D> thumbnailReadyCallback)
        {
            var file = await asset.GetFileAsync(asset.PreviewFile, CancellationToken.None);
            if (file == null)
            {
                thumbnailReadyCallback?.Invoke(null);
                return;
            }

            if (!s_ThumbnailCache.TryGetValue(file.Descriptor.Path, out var entry))
            {
                // Create new download request
                entry = new ThumbnailDownloadEntry { IsDownloading = true };

                lock (entry.Listeners)
                {
                    entry.Listeners.Add(thumbnailReadyCallback);
                }

                s_ThumbnailCache.Add(file.Descriptor.Path, entry);

                try
                {
                    var url = await GetUrl(file);
                    if (url == null)
                    {
                        entry.IsDownloading = false;
                        thumbnailReadyCallback?.Invoke(null);
                        return;
                    }

                    var resizedUrl = $"https://transformation.unity.com/api/images?url={Uri.EscapeDataString(url.ToString())}&width={width}";
                    var taskTexture = DownloadThumbnail(resizedUrl);
                    if (await Task.WhenAny(taskTexture) == taskTexture)
                    {
                        entry.Texture2D = taskTexture.Result;
                        entry.IsDownloading = false;
                    }
                    else
                    {
                        entry.IsDownloading = false;
                        thumbnailReadyCallback?.Invoke(null);
                        Debug.LogError($"Timed out downloading thumbnail for {file.Descriptor.Path}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error getting thumbnail for {asset.Name}: {e}");
                    OnThumbnailDownloaded(entry);
                    thumbnailReadyCallback?.Invoke(null);
                }
            }
            else if (entry.IsDownloading)
            {
                // Texture is being downloaded
                lock (entry.Listeners)
                {
                    entry.Listeners.Add(thumbnailReadyCallback);
                }

                return;
            }

            // Texture is ready
            OnThumbnailDownloaded(entry);
            thumbnailReadyCallback?.Invoke(entry.Texture2D);
        }

        static async Task<Uri> GetUrl(IFile file)
        {
            var taskUrl = file.GetDownloadUrlAsync(CancellationToken.None);
            if (await Task.WhenAny(taskUrl, Task.Delay(k_TimeoutDelay)) == taskUrl)
            {
                return taskUrl.Result;
            }

            Debug.LogError($"Timed out getting thumbnail url for {file.Descriptor.Path}");
            return null;
        }

        static void OnThumbnailDownloaded(ThumbnailDownloadEntry entry)
        {
            entry.IsDownloading = false;

            lock (entry.Listeners)
            {
                foreach (var listener in entry.Listeners)
                {
                    listener?.Invoke(entry.Texture2D);
                }
            }
        }

        static async Task<Texture2D> DownloadThumbnail(string url)
        {
            using var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            uwr.downloadHandler = new DownloadHandlerTexture();

            var operation = uwr.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            return DownloadHandlerTexture.GetContent(uwr);
        }
    }
}
