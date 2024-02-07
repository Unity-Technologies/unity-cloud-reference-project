using System;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.ReferenceProject.AssetList
{
    public static class TextureController
    {
        static readonly int k_TimeoutDelay = 10000;

        class TextureDownloadEntry
        {
            public bool IsDownloading;
            public Texture2D Texture2D;
            public readonly List<Action<Texture2D>> Listeners = new();
        }

        static Dictionary<string, TextureDownloadEntry> s_TextureCache = new();
        static Dictionary<string, string> s_ProjectIconUrl = new();

        public static async Task GetThumbnail(IAsset asset, int width, Action<Texture2D> thumbnailReadyCallback)
        {
            var file = await asset.GetFileAsync(asset.PreviewFile, CancellationToken.None);
            if (file == null)
            {
                thumbnailReadyCallback?.Invoke(null);
                return;
            }

            var key = asset.Descriptor.AssetId.ToString();
            
            if (!s_TextureCache.TryGetValue(key, out var entry))
            {
                // Create new download request
                entry = new TextureDownloadEntry { IsDownloading = true };

                lock (entry.Listeners)
                {
                    entry.Listeners.Add(thumbnailReadyCallback);
                }

                s_TextureCache.Add(key, entry);

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
                    var taskTexture = DownloadTexture(resizedUrl);
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
                    OnTextureDownloaded(entry);
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
            OnTextureDownloaded(entry);
            thumbnailReadyCallback?.Invoke(entry.Texture2D);
        }

        public static void SetProjectIconUrl(string projectId, string iconUrl)
        {
            s_ProjectIconUrl[projectId] = iconUrl;
        }

        public static async Task GetProjectIcon(string projectId, Action<Texture2D> textureReadyCallback)
        {
            if(!s_ProjectIconUrl.TryGetValue(projectId, out var iconUrl) || string.IsNullOrEmpty(iconUrl))
            {
                textureReadyCallback?.Invoke(null);
                return;
            }

            if (!s_TextureCache.TryGetValue(iconUrl, out var entry))
            {
                // Create new download request
                entry = new TextureDownloadEntry { IsDownloading = true };

                lock (entry.Listeners)
                {
                    entry.Listeners.Add(textureReadyCallback);
                }

                s_TextureCache.Add(iconUrl, entry);

                try
                {
                    if (string.IsNullOrEmpty(iconUrl))
                    {
                        entry.IsDownloading = false;
                        textureReadyCallback?.Invoke(null);
                        return;
                    }

                    var taskTexture = DownloadTexture(iconUrl);
                    if (await Task.WhenAny(taskTexture) == taskTexture)
                    {
                        entry.Texture2D = taskTexture.Result;
                        entry.IsDownloading = false;
                    }
                    else
                    {
                        entry.IsDownloading = false;
                        textureReadyCallback?.Invoke(null);
                        Debug.LogError($"Timed out downloading project icon for {iconUrl}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error getting project icon for {iconUrl}: {e}");
                    OnTextureDownloaded(entry);
                    textureReadyCallback?.Invoke(null);
                }
            }
            else if (entry.IsDownloading)
            {
                // Texture is being downloaded
                lock (entry.Listeners)
                {
                    entry.Listeners.Add(textureReadyCallback);
                }

                return;
            }

            // Texture is ready
            OnTextureDownloaded(entry);
            textureReadyCallback?.Invoke(entry.Texture2D);
        }

        static readonly Color[] k_ProjectIconDefaultColors =
        {
            new Color32(233, 61, 130, 255), // Crimson
            new Color32(247, 107, 21, 255), // Orange
            new Color32(255, 166, 0, 255), // Amber
            new Color32(18, 165, 148, 255), // Teal
            new Color32(62, 99, 221, 255), // Indigo
            new Color32(110, 86, 207, 255), // Violet
        };

        public static Color GetProjectIconColor(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return k_ProjectIconDefaultColors[0];
            }

            var lastCharIndex = projectId.Length - 1;
            var lastCharCode = projectId[lastCharIndex];
            var colorIndex = lastCharCode % k_ProjectIconDefaultColors.Length;

            return k_ProjectIconDefaultColors[colorIndex];
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

        static void OnTextureDownloaded(TextureDownloadEntry entry)
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

        static async Task<Texture2D> DownloadTexture(string url)
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
