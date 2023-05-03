using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// ActionBar UI element.
    /// </summary>
    public class ActionBar : VisualElement
    {
        /// <summary>
        /// The ActionBar main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-actionbar";

        /// <summary>
        /// The ActionBar action group styling class.
        /// </summary>
        public static readonly string actionGroupUssClassName = ussClassName + "__actiongroup";

        /// <summary>
        /// The ActionBar checkbox styling class.
        /// </summary>
        public static readonly string checkboxUssClassName = ussClassName + "__checkbox";

        readonly ActionGroup m_ActionGroup;

        readonly Checkbox m_SelectAllCheckbox;

        BaseVerticalCollectionView m_CollectionView;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActionBar()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_SelectAllCheckbox = new Checkbox { name = checkboxUssClassName, emphasized = true };
            m_SelectAllCheckbox.AddToClassList(checkboxUssClassName);
            hierarchy.Add(m_SelectAllCheckbox);
            m_SelectAllCheckbox.RegisterValueChangedCallback(OnCheckboxValueChanged);

            m_ActionGroup = new ActionGroup { name = actionGroupUssClassName };
            m_ActionGroup.AddToClassList(actionGroupUssClassName);
            hierarchy.Add(m_ActionGroup);

            collectionView = null;
        }

        /// <summary>
        /// The collection view attached to this <see cref="ActionBar"/>.
        /// </summary>
        public BaseVerticalCollectionView collectionView
        {
            get => m_CollectionView;

            set
            {
                if (m_CollectionView != null)
#if UNITY_2022_2_SIC
                    m_CollectionView.selectedIndicesChanged -= OnSelectedIndicesChanged;
#else
                    m_CollectionView.onSelectedIndicesChange -= OnSelectedIndicesChanged;
#endif
                m_CollectionView = value;
                if (m_CollectionView != null)
#if UNITY_2022_2_SIC
                    m_CollectionView.selectedIndicesChanged += OnSelectedIndicesChanged;
#else
                    m_CollectionView.onSelectedIndicesChange += OnSelectedIndicesChanged;
#endif

                RefreshUI();
            }
        }

        /// <summary>
        /// The list of selected indices from the Collection View.
        /// </summary>
        public IEnumerable<int> selectedIndices => m_CollectionView?.selectedIndices ?? new List<int>();

        /// <summary>
        /// The items source from the Collection View.
        /// </summary>
        public IList itemsSource => m_CollectionView?.itemsSource;

        /// <summary>
        /// Text used for item selection message.
        /// <remarks>We recommend to use a SmartString text in order to adjust the
        /// text based on the number of selected items.</remarks>
        /// </summary>
        public string message
        {
            get => m_SelectAllCheckbox.label;
            set => m_SelectAllCheckbox.label = value;
        }

        /// <summary>
        /// The content container of the <see cref="ActionBar"/>.
        /// </summary>
        public override VisualElement contentContainer => m_ActionGroup.contentContainer;

        void OnCheckboxValueChanged(ChangeEvent<CheckboxState> evt)
        {
            var val = evt.newValue;

            if (m_CollectionView == null)
                return;

            switch (val)
            {
                case CheckboxState.Unchecked:
                    m_CollectionView.ClearSelection();
                    break;
                case CheckboxState.Intermediate:
                    // do nothing
                    break;
                case CheckboxState.Checked:
                    var range = new int[itemsSource.Count];
                    for (var i = 0; i < itemsSource.Count; i++)
                    {
                        range[i] = i;
                    }
                    m_CollectionView.SetSelection(range);
                    break;
                default:
                    throw new ValueOutOfRangeException(nameof(val), val);
            }
        }

        void OnSelectedIndicesChanged(IEnumerable<int> _)
        {
            RefreshUI();
        }

        void RefreshUI()
        {
            var selectionCount = 0;
            foreach (var unused in selectedIndices)
            {
                selectionCount++;
            }
            
            var checkboxValue = CheckboxState.Unchecked;
            if (selectionCount > 0)
            {
                checkboxValue = selectionCount == itemsSource.Count
                    ? CheckboxState.Checked
                    : CheckboxState.Intermediate;
            }
            m_SelectAllCheckbox.SetValueWithoutNotify(checkboxValue);
        }

        /// <summary>
        /// The UXML factory for the <see cref="ActionBar"/>.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ActionBar, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="ActionBar"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Message = new UxmlStringAttributeDescription
            {
                name = "message",
                defaultValue = null
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                m_PickingMode.defaultValue = PickingMode.Ignore;
                base.Init(ve, bag, cc);
                var el = (ActionBar)ve;
                el.message = m_Message.GetValueFromBag(bag, cc);
            }
        }
    }
}
