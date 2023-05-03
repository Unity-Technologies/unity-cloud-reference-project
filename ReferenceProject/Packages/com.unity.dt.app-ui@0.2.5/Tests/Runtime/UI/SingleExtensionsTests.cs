using System.Globalization;
using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    class SingleExtensionsTests
    {
        static readonly (float val, CultureInfo culture, string expected)[] k_Cases = new[]
        {
            (1f, CultureInfo.InvariantCulture, "1"),
            (1.0000f, CultureInfo.InvariantCulture, "1"),
            (1.01f, CultureInfo.InvariantCulture, "1.01"),
            (-1.01f, CultureInfo.InvariantCulture, "-1.01"),
            (100.01f, CultureInfo.InvariantCulture, "100.01"),
            (-100.01f, CultureInfo.InvariantCulture, "-100.01"),
            (1000.01f, CultureInfo.InvariantCulture, "1000"),
            (10000.01f, CultureInfo.InvariantCulture, "10000"),
        };

        [Test]
        public void SingleExtensions_ToStringWithVariableDecimalCount_ReturnValidString(
            [ValueSource(nameof(k_Cases))] (float val, CultureInfo culture, string expected) args)
        {
            Assert.AreEqual(args.expected, args.val.ToStringWithVariableDecimalCount(args.culture));
        }
    }
}
