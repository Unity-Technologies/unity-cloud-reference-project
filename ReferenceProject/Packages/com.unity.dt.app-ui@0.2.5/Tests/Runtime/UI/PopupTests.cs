using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    class PopupTests<T> where T : Popup
    {
        Popup m_Popup;

        protected T popup => m_Popup as T;
    }
}
