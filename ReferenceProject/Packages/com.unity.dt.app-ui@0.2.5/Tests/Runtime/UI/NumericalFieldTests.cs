using System;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    class NumericalFieldTests<T, U> : VisualElementTests<T>
        where T : NumericalField<U>, new()
        where U : struct, IComparable, IComparable<U>, IFormattable
    {

    }
}
