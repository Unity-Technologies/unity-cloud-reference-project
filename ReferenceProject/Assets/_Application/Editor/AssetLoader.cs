#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Assets;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.AssetList;
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
        IAssetListController m_AssetListController;
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
            IOrganizationRepository organizationRepository, IAssetRepository assetRepository, IAssetListController assetListController)
        {
            m_OrganizationRepository = organizationRepository;
            m_AssetRepository = assetRepository;
            m_AssetListController = assetListController;
            m_DataStreamController = dataStreamController;
            m_Session = session;

            StatusReceiver.SetStatus("Waiting authentication...");
            session.RegisterLoggedInCallback(OnLoggedIn);

            session.Initialize();
        }

        async Task OnLoggedIn()
        {
            if (m_AssetListController.SelectedOrganization != null)
            {
                return;
            }

            StatusReceiver.SetStatus($"{ObjectNames.NicifyVariableName(m_Session.State.ToString())}");
            IAsset foundAsset = null;
            IAsset firstFoundAsset = null;
            var cancellationToken = new CancellationTokenSource().Token;

            var organizations = new List<IOrganization>();

            await foreach (var organization in m_OrganizationRepository.ListOrganizationsAsync(Range.All, cancellationToken))
            {
                organizations.Add(organization);
            }

            foreach (var organization in organizations)
            {
                var projects = m_AssetRepository.ListAssetProjectsAsync(
                    organization.Id,
                    Range.All,
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
                var assetQueryBuilder = project.QueryAssets().LimitTo(Range.All);
                var assets = assetQueryBuilder.ExecuteAsync(cancellationToken);

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
