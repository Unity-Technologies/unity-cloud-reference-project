using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
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

        static readonly string k_SourceDatasetName = "Source"; // This is a convention in the Asset Manager

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
        public void Setup(IMainUIPanel mainUIPanel, IAppMessaging appMessaging)
        {
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
                dialog.isPrimaryActionDisabled = true;

                try
                {
                    await StartTransformation();
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
                    m_DatasetDropdown.sourceItems = m_HideNonSourceDatasets ? datasets.Where(d => d.Name != null && d.Name.Contains(k_SourceDatasetName)).ToList() : datasets;

                    if (datasets.Count > 0)
                    {
                        m_DatasetDropdown.selectedIndex = 0;
                        _ = OnDatasetValueChanged(0);
                    }
                });

                m_Transformations.Clear();
                m_Dialog.isPrimaryActionDisabled = true;
                m_MessageUIHelper.ShowMessage(MessageType.None);

                Modal.Build(m_MainUIPanel.Panel, m_Dialog).Show();

                StartRefreshTransformationsState(asset);
            }
            catch (Exception e)
            {
                m_AppMessaging.ShowException(e);
            }
        }

        async Task<ITransformation> StartTransformation()
        {
            var transformationCreation = new TransformationCreation
            {
                WorkflowType = WorkflowType.Data_Streaming,
                InputFilePaths = new[] { m_SelectedFile }
            };

            try
            {
                return await m_SelectedDataset.StartTransformationAsync(transformationCreation, default);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
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
            if (m_SelectedDataset == null || m_Transformations == null)
            {
                m_Transformations?.Clear();
                return;
            }

            try
            {
                var isRunning = false;

                var transformationsAsync = m_SelectedDataset.ListTransformationsAsync(Range.All, default);
                await foreach (var transformation in transformationsAsync)
                {
                    if (transformation.WorkflowType != WorkflowType.Data_Streaming)
                    {
                        continue;
                    }

                    isRunning |= transformation.Status is TransformationStatus.Pending or TransformationStatus.Running;

                    AddOrUpdateTransformation(transformation);
                }

                if (m_Transformations.childCount == 0)
                {
                    m_Transformations.Add(new Heading("@AssetList:NoTransformation") { size = HeadingSize.XXS });
                }

                m_Dialog.isPrimaryActionDisabled = isRunning;

                await UpdateMessageUI(isRunning, asset);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                m_AppMessaging.ShowError($"Failed to fetch transformations for the selected dataset : {e.Message}", true);
            }
        }

        void AddOrUpdateTransformation(ITransformation transformation)
        {
            if (m_Transformations.Children().FirstOrDefault(e => e is TransformationDataElement t && t.Descriptor == transformation.Descriptor) is TransformationDataElement transformationElement)
            {
                transformationElement.Refresh(transformation);
            }
            else
            {
                m_Transformations.Add(new TransformationDataElement(transformation));
            }
        }

        async Task UpdateMessageUI(bool isRunning, IAsset asset)
        {
            if (isRunning)
            {
                m_MessageUIHelper.ShowMessage(MessageType.IsRunning);
            }
            else
            {
                var isStreamable = await StreamableAssetHelper.IsStreamable(asset);
                m_MessageUIHelper.ShowMessage(isStreamable ? MessageType.IsOverride : MessageType.Help);
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
            public TransformationDescriptor Descriptor { get; }

            readonly Heading m_Info;

            public TransformationDataElement(ITransformation transformation)
            {
                Descriptor = transformation.Descriptor;

                var container = new VisualElement();
                Add(container);

                m_Info = new Heading { size = HeadingSize.XXS };
                container.Add(m_Info);

                Refresh(transformation);
            }

            public void Refresh(ITransformation transformation)
            {
                m_Info.text = $"{transformation.InputFiles.FirstOrDefault()} | {transformation.Status}";
            }
        }
    }
}
