using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.ReferenceProject.SearchSortFilter.Tests
{
    public class FilterTests
    {
        [Test]
        public void Filter_Correctness_Equals()
        {
            var list = new List<CustomClass>();

            list.Add(new CustomClass { key = "test1" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test1" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test1" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test1" });
            list.Add(new CustomClass { key = "test0" });

            // Test0 = 6
            // Test1 = 4
            var module = new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.Equals);

            var listCached = new List<CustomClass>();

            module.SelectedOption = "test0";
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(6, listCached.Count);

            module.SelectedOption = "test1";
            listCached.Clear();
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(4, listCached.Count);
        }

        [Test]
        public void Filter_Correctness_NonEquals()
        {
            var list = new List<CustomClass>();

            list.Add(new CustomClass { key = "test1" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test1" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test1" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test0" });
            list.Add(new CustomClass { key = "test1" });
            list.Add(new CustomClass { key = "test0" });

            // Test0 = 6
            // Test1 = 4
            var module = new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.NotEquals);

            var listCached = new List<CustomClass>();

            module.SelectedOption = "test0";
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(4, listCached.Count);

            module.SelectedOption = "test1";
            listCached.Clear();
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(6, listCached.Count);
        }

        [Test]
        public void FilterList_NullOrZeroCount()
        {
            List<CustomClass> list = null;

            var module = new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.Equals);

            module.SelectedOption = "0";
            module.PerformFiltering(list);
            Assert.AreEqual(null, list);

            list = new List<CustomClass>();
            module.PerformFiltering(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void SearchString_NullOrEmptyOrSpace()
        {
            var list = new List<CustomClass>();

            list.Add(new CustomClass { key = "test 1" });
            list.Add(new CustomClass { key = "test 2" });

            var module = new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.Equals);

            var listCached = new List<CustomClass>();

            module.SelectedOption = null;
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(2, listCached.Count);

            module.SelectedOption = "";
            listCached.Clear();
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(2, listCached.Count);

            module.SelectedOption = " ";
            listCached.Clear();
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(0, listCached.Count);
        }

        [Test]
        public void Filter_EmptyResult()
        {
            var list = new List<CustomClass>();

            list.Add(new CustomClass { key = "test 1" });
            list.Add(new CustomClass { key = "test 2" });
            list.Add(new CustomClass { key = "test 3" });
            list.Add(new CustomClass { key = "test 4" });

            var module = new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.Equals);

            var listCached = new List<CustomClass>();

            module.SelectedOption = "emptyResult";
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(0, listCached.Count);
        }

        class CustomClass
        {
            public string key;
        }
    }
}
