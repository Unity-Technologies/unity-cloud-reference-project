using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.UIPanel;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.AssetList
{
    public class TransformationWorkflowUIController : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_VisualTreeAsset;
        
        TransformationWorkflowController m_TransformationWorkflowController;

        IDataset m_SelectedDataset;
        string m_SelectedFile;

        IMainUIPanel m_MainUIPanel;
        IAppMessaging m_AppMessaging;

        AlertDialog m_Dialog;

        Dropdown m_DatasetDropdown;
        Dropdown m_FilesDropdown;

        VisualElement m_Transformations;
        Coroutine m_RefreshTransformationState;

        VisualElement m_TransformationRunning;

        static readonly float k_RefreshTransformationsStateInterval = 2.0f;

        [Inject]
        public void Setup(IServiceHttpClient serviceHttpClient, IServiceHostResolver serviceHostResolver, IMainUIPanel mainUIPanel, IAppMessaging appMessaging)
        {
            m_TransformationWorkflowController = new TransformationWorkflowController(serviceHttpClient, serviceHostResolver);
            m_MainUIPanel = mainUIPanel;
            m_AppMessaging = appMessaging;
        }

        static async Task<List<string>> GetFiles(IDataset dataset)
        {
            var cancellationToken = new CancellationToken();

            var files = new List<string>();

            await foreach (var file in dataset.ListFilesAsync(Range.All, cancellationToken))
            {
                files.Add(file.Descriptor.Path);
            }

            return files;
        }

        static async Task GetDatasets(IAsset asset, Action<List<IDataset>> datasetsCallback)
        {
            var cancellationToken = new CancellationToken();

            var datasets = new List<IDataset>();

            await foreach (var dataset in asset.ListDatasetsAsync(Range.All, cancellationToken))
            {
                datasets.Add(dataset);
            }

            datasetsCallback?.Invoke(datasets);
        }

        AlertDialog CreateDialog()
        {
            var root = m_VisualTreeAsset.Instantiate();

            m_DatasetDropdown = root.Q<Dropdown>("DatasetDropdown");
            m_FilesDropdown = root.Q<Dropdown>("FileDropdown");

            m_Transformations = root.Q("TransformationContainer");

            m_TransformationRunning = root.Q("TransformationRunningMessage");
            
            Utils.SetVisible(m_TransformationRunning, false);

            var dialog = new AlertDialog
            {
                title =  "@AssetList:GenerateStreamable"
            };
            
            dialog.SetCancelAction(0, "@ReferenceProject:Dismiss");
            
            dialog.SetPrimaryAction(1, "@AssetList:StartTransformation", async () =>
            {
                dialog.isPrimaryActionDisabled = false;
                
                try
                {
                    await m_TransformationWorkflowController.StartTransformation(m_SelectedDataset, m_SelectedFile);
                    m_AppMessaging.ShowSuccess("@AssetList:TransformationStarted");
                }
                catch (Exception e)
                {
                    m_AppMessaging.ShowError($"Failed to start transformation: {e.Message}");
                }
            });
            
            
            m_DatasetDropdown.bindItem = (item, i) => item.label = i < m_DatasetDropdown.sourceItems.Count ? ((IDataset)m_DatasetDropdown.sourceItems[i]).Name : null;

            m_DatasetDropdown.RegisterValueChangedCallback(b =>
            {
                _ = OnDatasetValueChanged(b.newValue.First());
            });

            m_FilesDropdown.bindItem = (item, i) => item.label = i < m_FilesDropdown.sourceItems.Count ? (string)m_FilesDropdown.sourceItems[i] : null;

            m_FilesDropdown.RegisterValueChangedCallback(v => { OnFileValueChanged(v.newValue.First()); });

            dialog.dismissRequested += (_) => { StopRefreshTransformationsState(); };

            dialog.Add(root);

            return dialog;
        }
        
        async Task OnDatasetValueChanged(int index)
        {
            m_SelectedDataset = (IDataset)m_DatasetDropdown.sourceItems[index];

            var files = await GetFiles(m_SelectedDataset);
            m_FilesDropdown.sourceItems = files;
            m_FilesDropdown.SetEnabled(true);

            if (files.Count > 0)
            {
                m_FilesDropdown.selectedIndex = 0;
                OnFileValueChanged(0);
            }
        }

        void OnFileValueChanged(int index)
        {
            m_SelectedFile = (string)m_FilesDropdown.sourceItems[index];
        }

        public void ShowDialog(IAsset asset)
        {
            try
            {
                m_Dialog ??= CreateDialog();

                m_Dialog.title = asset.Name;

                m_DatasetDropdown.SetEnabled(false);
                m_FilesDropdown.SetEnabled(false);
                
                _ = GetDatasets(asset, datasets =>
                {
                    m_DatasetDropdown.SetEnabled(true);
                    m_DatasetDropdown.sourceItems = datasets;
                    if (datasets.Count > 0)
                    {
                        m_DatasetDropdown.selectedIndex = 0;
                        _ = OnDatasetValueChanged(0);
                    }    
                });

                m_Transformations.Clear();

                Modal.Build(m_MainUIPanel.Panel, m_Dialog).Show();

                StartRefreshTransformationsState(asset);
            }
            catch (Exception e)
            {
                m_AppMessaging.ShowException(e);
            }
        }

        void StartRefreshTransformationsState(IAsset asset)
        {
            if (m_RefreshTransformationState == null)
            {
                m_RefreshTransformationState = StartCoroutine(RefreshTransformationsStateCoroutine(asset));
            }
        }

        void StopRefreshTransformationsState()
        {
            if (m_RefreshTransformationState != null)
            {
                StopCoroutine(m_RefreshTransformationState);
                m_RefreshTransformationState = null;
            }
        }

        async Task RefreshTransformationsState(IAsset asset)
        {
            if (asset == null || m_Transformations == null)
                return;

            var currentJobs = await m_TransformationWorkflowController.CurrentTransformations(asset.Descriptor.ProjectId);

            try
            {
                m_Transformations.Clear();
                m_Dialog.isPrimaryActionDisabled = true;
                Utils.SetVisible(m_TransformationRunning, false);

                var transformations = JsonConvert.DeserializeObject<List<TransformationData>>(currentJobs);

                var assetTransformations = transformations.Where(t => t.assetId == asset.Descriptor.AssetId.ToString());

                if (assetTransformations.Any())
                {
                    foreach (var t in assetTransformations)
                    {
                        if (t.IsRunning() && m_Dialog.isPrimaryActionDisabled)
                        {
                            m_Dialog.isPrimaryActionDisabled = false;
                            Utils.SetVisible(m_TransformationRunning, true);
                        }

                        m_Transformations.Add(new TransformationDataElement(t));
                    }
                }
                else
                {
                    m_Transformations.Add(new Heading("@AssetList:NoTransformation") { size = HeadingSize.XXS });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                m_AppMessaging.ShowError($"Failed to deserialize transformation data : {e.Message}");
            }
        }

        IEnumerator RefreshTransformationsStateCoroutine(IAsset asset)
        {
            while (asset != null && m_Transformations != null)
            {
                var task = RefreshTransformationsState(asset);
                yield return new WaitUntil(() => task.IsCompleted);

                yield return new WaitForSeconds(k_RefreshTransformationsStateInterval);
            }
        }

        class TransformationDataElement : VisualElement
        {
            readonly Heading m_Info;
            
            public TransformationDataElement(TransformationData data)
            {
                var container = new VisualElement();
                Add(container);

                m_Info = new Heading { size = HeadingSize.XXS };
                container.Add(m_Info);

                Refresh(data);
            }

            void Refresh(TransformationData data)
            {
                if (data.IsRunning())
                {
                    m_Info.text = $"{data.inputFiles.FirstOrDefault()} | {data.status} ({data.progress}%)";
                }
                else
                {
                    m_Info.text = $"{data.inputFiles.FirstOrDefault()} | {data.status}";
                }
            }
        }
    }
}
