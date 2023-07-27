using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Unity.ReferenceProject.Presence.Editor
{
    static class PackageManagerUtils
    {
        public static void AddPackages(params string[] packs)
        {
            var request = Client.List(true);
            while (!request.IsCompleted)
            {
                // Wait for the request to complete
            }

            if (request.Status != StatusCode.Success)
            {
                Debug.LogError($"Could not obtain the list of packages. Can not continue");
                return;
            }

            foreach (var pack in packs)
            {
                if (request.Result.Any(x => x.name.Equals(pack)))
                {
                    Debug.LogWarning($"{pack} is already in the project and cannot be added. Skipping");
                    continue;
                }

                var removeRequest = Client.Add(pack);
                while (!removeRequest.IsCompleted)
                {
                    // Wait for the request to complete
                }

                switch (removeRequest.Status)
                {
                    case StatusCode.Success:
                    {
                        Debug.Log($"{pack} was added");
                        break;
                    }
                    default:
                    {
                        Debug.LogWarning($"{pack} cannot be added: errorCode:{removeRequest.Error.errorCode}; message: {removeRequest.Error.errorCode}");
                        break;
                    }
                }
            }
        }
        
        public static void RemovePackages(params string[] packs)
        {
            var request = Client.List(true);
            while (!request.IsCompleted)
            {
                // Wait for the request to complete
            }

            if (request.Status != StatusCode.Success)
            {
                Debug.LogError($"Could not obtain the list of packages. Can not continue");
                return;
            }

            foreach (var pack in packs)
            {
                if (!request.Result.Any(x => x.name.Equals(pack)))
                {
                    Debug.LogWarning($"{pack} is not in the project and cannot be removed. Skipping");
                    continue;
                }

                var removeRequest = Client.Remove(pack);
                while (!removeRequest.IsCompleted)
                {
                    // Wait for the request to complete
                }

                switch (removeRequest.Status)
                {
                    case StatusCode.Success:
                    {
                        Debug.Log($"{pack} was removed");
                        break;
                    }
                    default:
                    {
                        Debug.LogWarning($"{pack} cannot be removed: errorCode:{removeRequest.Error.errorCode}; message: {removeRequest.Error.errorCode}");
                        break;
                    }
                }
            }
        }
    }
}
