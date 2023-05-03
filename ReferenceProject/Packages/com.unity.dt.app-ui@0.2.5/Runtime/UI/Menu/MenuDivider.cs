using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// A special <see cref="Divider"/> intended to be used inside <see cref="Menu"/> elements.
    /// </summary>
    public class MenuDivider : Divider
    {
        /// <summary>
        /// The MenuDivider main styling class.
        /// </summary>
        public static readonly string dividerClassName = "appui-menu__divider";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuDivider()
        {
            AddToClassList(dividerClassName);
        }

        /// <summary>
        /// UXML factory for the <see cref="MenuDivider"/>.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<MenuDivider, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UIElements.UxmlTraits"/> for the <see cref="MenuDivider"/>.
        /// </summary>
        public new class UxmlTraits : Divider.UxmlTraits { }
    }
}
