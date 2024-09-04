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
        class TextureDownloadEntry
        {
            public bool IsDownloading = true;
            public Texture2D Texture2D;
            public readonly List<Action<Texture2D>> Listeners = new();

            public TextureDownloadEntry(Action<Texture2D> listener)
            {
                Listeners.Add(listener);
            }
        }

        static Dictionary<string, TextureDownloadEntry> s_TextureCache = new();
        static Dictionary<string, string> s_ProjectIconUrl = new();

        public static async Task GetThumbnail(IAsset asset, int width, Action<Texture2D> thumbnailReadyCallback)
        {
            var key = asset.Descriptor.AssetId.ToString();

            if (!s_TextureCache.TryGetValue(key, out var entry))
            {
                // Create new download request
                entry = new TextureDownloadEntry(thumbnailReadyCallback);
                s_TextureCache.Add(key, entry);

                try
                {
                    var url = await asset.GetPreviewUrlAsync(CancellationToken.None);
                    if (url != null)
                    {
                        var resizedUrl = $"https://transformation.unity.com/api/images?url={Uri.EscapeDataString(url.ToString())}&width={width}";
                        entry.Texture2D = await DownloadTexture(resizedUrl);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error getting thumbnail for {asset.Name}: {e}");
                }
                finally
                {
                    OnTextureDownloaded(entry);
                }
            }
            else if (entry.IsDownloading)
            {
                // Texture is being downloaded
                lock (entry.Listeners)
                {
                    entry.Listeners.Add(thumbnailReadyCallback);
                }
            }
            else
            {
                thumbnailReadyCallback?.Invoke(entry.Texture2D);
            }
        }

        public static void SetProjectIconUrl(string projectId, string iconUrl)
        {
            s_ProjectIconUrl[projectId] = iconUrl;
        }

        public static async Task GetProjectIcon(string projectId, Action<Texture2D> textureReadyCallback)
        {
            if (!s_ProjectIconUrl.TryGetValue(projectId, out var iconUrl) || string.IsNullOrEmpty(iconUrl))
            {
                textureReadyCallback?.Invoke(null);
                return;
            }

            if (!s_TextureCache.TryGetValue(iconUrl, out var entry))
            {
                // Create new download request
                entry = new TextureDownloadEntry(textureReadyCallback);
                s_TextureCache.Add(iconUrl, entry);

                try
                {
                    entry.Texture2D = await DownloadTexture(iconUrl);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error getting project icon for {iconUrl}: {e}");
                }
                finally
                {
                    OnTextureDownloaded(entry);
                }
            }
            else if (entry.IsDownloading)
            {
                // Texture is being downloaded
                lock (entry.Listeners)
                {
                    entry.Listeners.Add(textureReadyCallback);
                }
            }
            else
            {
                textureReadyCallback?.Invoke(entry.Texture2D);
            }
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
            uwr.disposeDownloadHandlerOnDispose = true;

            var textureHandler = new DownloadHandlerTexture();
            uwr.downloadHandler = textureHandler;

            var operation = uwr.SendWebRequest();

            // Ensure that the operation and texture download are completed before attempting to use the texture
            while (!operation.isDone || !textureHandler.isDone)
            {
                await Task.Yield();
            }

            return textureHandler.texture;
        }
    }
}
