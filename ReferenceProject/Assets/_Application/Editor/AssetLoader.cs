#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Identity;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Editor
{
    interface IStatusReceiver
    {
        void SetStatus(string status);
    }

    public class AssetLoader : MonoBehaviour
    {
        [SerializeField]
        string m_AssetName;

        IOrganizationRepository m_OrganizationRepository;
        IAssetRepository m_AssetRepository;
        IDataStreamController m_DataStreamController;
        ICloudSession m_Session;

        internal static IStatusReceiver StatusReceiver { get; set; }

        public string AssetName
        {
            get => m_AssetName;
            set => m_AssetName = value;
        }

        [Inject]
        public void Setup(ICloudSession session, IDataStreamController dataStreamController,
            IOrganizationRepository organizationRepository, IAssetRepository assetRepository)
        {
            m_OrganizationRepository = organizationRepository;
            m_AssetRepository = assetRepository;
            m_DataStreamController = dataStreamController;
            m_Session = session;

            StatusReceiver.SetStatus("Waiting authentication...");
            session.RegisterLoggedInCallback(OnLoggedIn);

            session.Initialize();
        }

        async Task OnLoggedIn()
        {
            StatusReceiver.SetStatus($"{ObjectNames.NicifyVariableName(m_Session.State.ToString())}");
            IAsset foundAsset = null;
            IAsset firstFoundAsset = null;
            var cancellationToken = new CancellationTokenSource().Token;
            var organizations = await m_OrganizationRepository.ListOrganizationsAsync();

            foreach (var organization in organizations)
            {
                var projects = m_AssetRepository.ListAssetProjectsAsync(
                    organization.Id,
                    new(nameof(IProject.Name), Range.All),
                    cancellationToken);

                Tuple<IAsset, IAsset> assetsFound = await SearchProjects(projects, cancellationToken);
                foundAsset = assetsFound.Item1;
                firstFoundAsset = assetsFound.Item2;

                if (foundAsset != null)
                {
                    break;
                }
            }

            if (foundAsset == null && firstFoundAsset != null)
            {
                foundAsset = firstFoundAsset;
            }

            if (foundAsset != null)
            {
                StatusReceiver.SetStatus($"Streaming '{foundAsset.Name}' ...'");
                m_DataStreamController.Load(foundAsset);
            }
            else
            {
                StatusReceiver.SetStatus("Unable to find a asset to open'");
            }
        }

        async Task<Tuple<IAsset,IAsset>> SearchProjects(IAsyncEnumerable<IAssetProject> projects, CancellationToken cancellationToken)
        {
            IAsset foundAsset = null;
            IAsset firstFoundAsset = null;

            await foreach (var project in projects)
            {
                var assets = project.SearchAssetsAsync(
                    new AssetSearchFilter(),
                    new Pagination(nameof(IAsset.Name), Range.All),
                    cancellationToken);

                if (!string.IsNullOrEmpty(m_AssetName))
                {
                    await foreach (var asset in assets)
                    {
                        if (asset.Name.ToLowerInvariant().Contains(m_AssetName.ToLowerInvariant()))
                        {
                            foundAsset = asset;
                            return new Tuple<IAsset, IAsset>(foundAsset, firstFoundAsset);
                        }
                    }
                }
                else if (firstFoundAsset == null)
                {
                    await using (var enumerator = assets.GetAsyncEnumerator(cancellationToken))
                    {
                        firstFoundAsset = await enumerator.MoveNextAsync() ? enumerator.Current : default;
                    }
                }
            }

            return new Tuple<IAsset, IAsset>(foundAsset, firstFoundAsset);
        }
    }
}

#endif
