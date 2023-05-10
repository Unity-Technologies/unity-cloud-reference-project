using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.ReferenceProject.SearchSortFilter.Tests
{
    public class SortTests
    {
        [Test]
        public void SortString_Correctness_Ascending()
        {
            var list = new List<CustomClass>
            {
                // 1, 2, 3, 4, 5
                new() { key = "1" },
                new() { key = "0" },
                new() { key = "3" },
                new() { key = "4" },
                new() { key = "2" }
            };

            var module = new SortModule<CustomClass>((nameof(CustomClass.key),
                new SortBindNodeString<CustomClass>(x => x.key)));

            var listCached = new List<CustomClass>();
            listCached.AddRange(list);
            module.SortOrder = SortOrder.Ascending;
            module.CurrentSortPathName = nameof(CustomClass.key);
            module.PerformSort(listCached);

            Assert.AreEqual("0", listCached[0].key);
            Assert.AreEqual("1", listCached[1].key);
            Assert.AreEqual("2", listCached[2].key);
            Assert.AreEqual("3", listCached[3].key);
            Assert.AreEqual("4", listCached[4].key);
        }

        [Test]
        public void SortString_Correctness_Descending()
        {
            var list = new List<CustomClass>
            {
                // 1, 2, 3, 4, 5
                new() { key = "1" },
                new() { key = "0" },
                new() { key = "3" },
                new() { key = "4" },
                new() { key = "2" }
            };

            var module = new SortModule<CustomClass>((nameof(CustomClass.key),
                new SortBindNodeString<CustomClass>(x => x.key)));
            module.SortOrder = SortOrder.Descending;
            module.CurrentSortPathName = nameof(CustomClass.key);

            var listCached = new List<CustomClass>();
            listCached.AddRange(list);
            module.PerformSort(listCached);

            Assert.AreEqual("4", listCached[0].key);
            Assert.AreEqual("3", listCached[1].key);
            Assert.AreEqual("2", listCached[2].key);
            Assert.AreEqual("1", listCached[3].key);
            Assert.AreEqual("0", listCached[4].key);
        }

        [Test]
        public void SortStringList_NullOrZeroCount()
        {
            List<CustomClass> list = null;

            var module = new SortModule<CustomClass>((nameof(CustomClass.key),
                new SortBindNodeString<CustomClass>(x => x.key)))
            {
                SortOrder = SortOrder.Ascending,
                CurrentSortPathName = nameof(CustomClass.key)
            };

            module.PerformSort(list);
            Assert.AreEqual(null, list);

            list = new List<CustomClass>();
            module.PerformSort(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void SortStringList_OneCount()
        {
            var list = new List<CustomClass> { new() { key = "1" } };

            var module = new SortModule<CustomClass>((nameof(CustomClass.key),
                new SortBindNodeString<CustomClass>(x => x.key)))
            {
                SortOrder = SortOrder.Ascending,
                CurrentSortPathName = nameof(CustomClass.key)
            };

            module.PerformSort(list);
            Assert.AreEqual("1", list[0].key);
        }

        class CustomClass
        {
            public string key;
        }
    }
}
