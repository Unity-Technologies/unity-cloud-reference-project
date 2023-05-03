using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A SplitView is a container that displays two children at a time and provides a UI to
    /// navigate between them. The split bar can be dragged to resize the two children.
    /// </summary>
    public class SplitView : TwoPaneSplitView
    {
        /// <summary>
        /// The main styling class of the SplitView. This is the class that is used in the USS file.
        /// </summary>
        public static readonly string ussClassName = "appui-splitview";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SplitView()
        {
            AddToClassList(ussClassName);
        }

        /// <summary>
        /// Defines the UxmlFactory for the SplitView.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="SplitView"/>.
        /// </summary>
        public new class UxmlTraits : TwoPaneSplitView.UxmlTraits
        {

        }
    }
}
