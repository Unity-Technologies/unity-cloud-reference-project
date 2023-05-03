using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A PageView is a container that displays a single child at a time and provides a UI to
    /// navigate between them. It is similar to a <see cref="ScrollView"/> but here children are
    /// snapped to the container's edges.
    /// </summary>
    public class PageView : VisualElement
    {
        /// <summary>
        /// The main styling class of the PageView. This is the class that is used in the USS file.
        /// </summary>
        public static readonly string ussClassName = "appui-pageview";

        /// <summary>
        /// The styling class applied to the SwipeView.
        /// </summary>
        public static readonly string swipeViewUssClassName = ussClassName + "__swipeview";

        /// <summary>
        /// The styling class applied to the PageIndicator.
        /// </summary>
        public static readonly string pageIndicatorUssClassName = ussClassName + "__page-indicator";

        /// <summary>
        /// The styling class applied to the PageView depending on its direction.
        /// </summary>
        public static readonly string variantUssClassName = ussClassName + "--";

        readonly SwipeView m_SwipeView;

        readonly PageIndicator m_PageIndicator;

        /// <summary>
        /// The content container of the PageView.
        /// </summary>
        public override VisualElement contentContainer => m_SwipeView.contentContainer;

        /// <summary>
        /// The speed of the animation when snapping to a page.
        /// </summary>
        public float snapAnimationSpeed
        {
            get => m_SwipeView.snapAnimationSpeed;
            set => m_SwipeView.snapAnimationSpeed = value;
        }

        /// <summary>
        /// A limit number of pages to keep animating the transition between pages.
        /// </summary>
        public int skipAnimationThreshold
        {
            get => m_SwipeView.skipAnimationThreshold;
            set => m_SwipeView.skipAnimationThreshold = value;
        }

        /// <summary>
        /// Whether the PageView should wrap around when reaching the end of the list.
        /// </summary>
        public bool wrap
        {
            get => m_SwipeView.wrap;
            set => m_SwipeView.wrap = value;
        }

        /// <summary>
        /// The orientation of the PageView.
        /// </summary>
        public Direction direction
        {
            get => m_SwipeView.direction;
            set
            {
                RemoveFromClassList(variantUssClassName + m_SwipeView.direction.ToString().ToLower());
                m_SwipeView.direction = value;
                m_PageIndicator.direction = value;
                AddToClassList(variantUssClassName + m_SwipeView.direction.ToString().ToLower());
            }
        }

        /// <summary>
        /// The number of pages that are visible at the same time.
        /// </summary>
        public int visibilityCount
        {
            get => m_SwipeView.visibleItemCount;
            set => m_SwipeView.visibleItemCount = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PageView()
        {
            AddToClassList(ussClassName);

            m_SwipeView = new SwipeView { name = swipeViewUssClassName };
            m_SwipeView.AddToClassList(swipeViewUssClassName);

            m_PageIndicator = new PageIndicator { name = pageIndicatorUssClassName };
            m_PageIndicator.AddToClassList(pageIndicatorUssClassName);

            hierarchy.Add(m_SwipeView);
            hierarchy.Add(m_PageIndicator);

            m_SwipeView.RegisterValueChangedCallback(OnSwipeValueChanged);
            m_PageIndicator.RegisterValueChangedCallback(OnPageIndicatorValueChanged);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            direction = Direction.Horizontal;
        }

        void OnPageIndicatorValueChanged(ChangeEvent<int> evt)
        {
            m_SwipeView.SetValueWithoutNotify(evt.newValue);
        }

        void OnSwipeValueChanged(ChangeEvent<int> evt)
        {
            m_PageIndicator.count = m_SwipeView.childCount;
            m_PageIndicator.SetValueWithoutNotify(evt.newValue);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            m_PageIndicator.count = m_SwipeView.childCount;
            m_PageIndicator.SetValueWithoutNotify(m_SwipeView.value);
        }

        /// <summary>
        /// Class used to create a PageView from UXML.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<PageView, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="PageView"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlEnumAttributeDescription<Direction> m_Direction = new UxmlEnumAttributeDescription<Direction>
            {
                name = "direction",
                defaultValue = Direction.Horizontal,
            };

            readonly UxmlFloatAttributeDescription m_AnimationSpeed = new UxmlFloatAttributeDescription()
            {
                name = "animation-speed",
                defaultValue = 0.5f,
            };

            readonly UxmlIntAttributeDescription m_SkipAnim = new UxmlIntAttributeDescription()
            {
                name = "skip-animation-threshold",
                defaultValue = 2,
            };

            readonly UxmlBoolAttributeDescription m_Wrap = new UxmlBoolAttributeDescription()
            {
                name = "wrap",
                defaultValue = false,
            };

            readonly UxmlIntAttributeDescription m_VisibilityCount = new UxmlIntAttributeDescription()
            {
                name = "visibility-count",
                defaultValue = 1,
            };

            /// <summary>
            /// Returns an enumerable containing UxmlChildElementDescription(typeof(VisualElement)), since VisualElements can contain other VisualElements.
            /// </summary>
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription =>
                new[]
                {
                    new UxmlChildElementDescription(typeof(SwipeViewItem))
                };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var el = (PageView)ve;
                el.direction = m_Direction.GetValueFromBag(bag, cc);
                el.wrap = m_Wrap.GetValueFromBag(bag, cc);
                el.visibilityCount = m_VisibilityCount.GetValueFromBag(bag, cc);
                el.skipAnimationThreshold = m_SkipAnim.GetValueFromBag(bag, cc);
                el.snapAnimationSpeed = m_AnimationSpeed.GetValueFromBag(bag, cc);
            }
        }
    }
}
