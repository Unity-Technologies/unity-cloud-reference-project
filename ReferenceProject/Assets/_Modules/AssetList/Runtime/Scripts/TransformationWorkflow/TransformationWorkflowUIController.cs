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
using Unity.ReferenceProject.DataStreaming;
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

        [SerializeField]
        bool m_HideNonSourceDatasets = true;
        
        static readonly string k_SourceDatasetName = "Sources"; // This is a convention in the Asset Manager
        
        TransformationWorkflowController m_TransformationWorkflowController;

        IDataset m_SelectedDataset;
        string m_SelectedFile;

        IMainUIPanel m_MainUIPanel;
        IAppMessaging m_AppMessaging;

        AlertDialog m_Dialog;

        Label m_AssetName;
        
        Dropdown m_DatasetDropdown;
        Dropdown m_FilesDropdown;

        VisualElement m_Transformations;
        Coroutine m_RefreshTransformationState;

        MessageUIHelper m_MessageUIHelper;
        

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

        enum MessageType
        {
            None,
            Help,
            IsRunning,
            IsOverride
        }
        
        class MessageUIHelper
        {
            readonly VisualElement m_TransformationMessageHelp;
            readonly VisualElement m_TransformationMessageIsRunning;
            readonly VisualElement m_TransformationMessageIsOverride;
            
            public MessageUIHelper(VisualElement root)
            {
                m_TransformationMessageHelp = root.Q("TransformationHelpMessage");
                m_TransformationMessageIsRunning = root.Q("TransformationIsRunningMessage");
                m_TransformationMessageIsOverride = root.Q("TransformationIsOverrideMessage");
            }
            
            public void ShowMessage(MessageType messageType)
            {
                Utils.SetVisible(m_TransformationMessageHelp, messageType == MessageType.Help);
                Utils.SetVisible(m_TransformationMessageIsRunning, messageType == MessageType.IsRunning);
                Utils.SetVisible(m_TransformationMessageIsOverride, messageType == MessageType.IsOverride);
            } 
        }
        
        AlertDialog CreateDialog()
        {
            var root = m_VisualTreeAsset.Instantiate();

            m_AssetName = root.Q<Label>("AssetName");
            
            m_DatasetDropdown = root.Q<Dropdown>("DatasetDropdown");
            m_FilesDropdown = root.Q<Dropdown>("FileDropdown");

            m_Transformations = root.Q("TransformationContainer");

            m_MessageUIHelper = new MessageUIHelper(root);
            m_MessageUIHelper.ShowMessage(MessageType.None);

            var dialog = new AlertDialog
            {
                title = "@AssetList:PrepareForDataStreaming"
            };
            
            dialog.SetCancelAction(0, "@ReferenceProject:Close");
            
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
                    m_AppMessaging.ShowError($"Failed to start transformation: {e.Message}", true);
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

                m_AssetName.text = asset.Name;

                m_DatasetDropdown.SetEnabled(false);
                m_FilesDropdown.SetEnabled(false);
                
                _ = GetDatasets(asset, datasets =>
                {
                    m_DatasetDropdown.SetEnabled(true);
                    m_DatasetDropdown.sourceItems = m_HideNonSourceDatasets ?
                        datasets.Where(d => d.Name == k_SourceDatasetName).ToList() : datasets;
                    
                    if (datasets.Count > 0)
                    {
                        m_DatasetDropdown.selectedIndex = 0;
                        _ = OnDatasetValueChanged(0);
                    }    
                });

                m_Transformations.Clear();
                m_MessageUIHelper.ShowMessage(MessageType.None);

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
                m_MessageUIHelper.ShowMessage(MessageType.None);

                var transformations = JsonConvert.DeserializeObject<List<TransformationData>>(currentJobs);

                var assetTransformations = transformations.Where(t => t.assetId == asset.Descriptor.AssetId.ToString());

                var isStreamable = await StreamableAssetHelper.IsStreamable(asset);
                var isRunning = false;

                if (assetTransformations.Any())
                {
                    foreach (var t in assetTransformations)
                    {
                        if (t.IsRunning() && m_Dialog.isPrimaryActionDisabled)
                        {
                            m_Dialog.isPrimaryActionDisabled = false;
                            isRunning = true;
                        }

                        m_Transformations.Add(new TransformationDataElement(t));
                    }
                }
                else
                {
                    m_Transformations.Add(new Heading("@AssetList:NoTransformation") { size = HeadingSize.XXS });
                }

                if (isRunning)
                {
                    m_MessageUIHelper.ShowMessage(MessageType.IsRunning);
                }
                else
                {
                    m_MessageUIHelper.ShowMessage(isStreamable ? MessageType.IsOverride : MessageType.Help);
                }
                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                m_AppMessaging.ShowError($"Failed to deserialize transformation data : {e.Message}", true);
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
                m_Info.text = $"{data.inputFiles.FirstOrDefault()} | {data.status}";
            }
        }
    }
}
