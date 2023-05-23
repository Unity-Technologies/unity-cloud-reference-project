using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.ReferenceProject.SearchSortFilter.Tests
{
    public class SearchTests
    {
        SearchModule<CustomClass> m_SearchModule;
        List<CustomClass> m_TestList;
        readonly List<CustomClass> m_CashedList = new List<CustomClass>();

        [SetUp]
        public void Setup()
        {
            m_SearchModule = new SearchModule<CustomClass>((nameof(CustomClass.key), new SearchBindNode<CustomClass>(c => c.key)));
            m_TestList = CreateTestList();
        }

        [Test]
        public void Search_Correctness()
        {
            PerformSearch(m_SearchModule, "1", m_CashedList, m_TestList);
            Assert.AreEqual(2, m_CashedList.Count);

            PerformSearch(m_SearchModule, "2", m_CashedList, m_TestList);
            Assert.AreEqual(2, m_CashedList.Count);
        }

        [Test]
        public void SearchList_NullOrZeroCount()
        {
            List<CustomClass> list = null;

            m_SearchModule.searchString = "0";
            m_SearchModule.PerformSearch(list);
            Assert.AreEqual(null, list);

            list = new List<CustomClass>();
            m_SearchModule.PerformSearch(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void SearchString_NullOrEmptyOrSpace()
        {
            PerformSearch(m_SearchModule, null, m_CashedList, m_TestList);
            Assert.AreEqual(8, m_CashedList.Count);

            PerformSearch(m_SearchModule, "", m_CashedList, m_TestList);
            Assert.AreEqual(8, m_CashedList.Count);

            PerformSearch(m_SearchModule, " ", m_CashedList, m_TestList);
            Assert.AreEqual(8, m_CashedList.Count);
        }

        [Test]
        public void Search_AllContainSearchString()
        {
            PerformSearch(m_SearchModule, "test", m_CashedList, m_TestList);
            Assert.AreEqual(8, m_CashedList.Count);
        }

        [Test]
        public void Search_EmptyResult()
        {
            PerformSearch(m_SearchModule, "emptyResult", m_CashedList, m_TestList);
            Assert.AreEqual(0, m_CashedList.Count);
        }

        List<CustomClass> CreateTestList()
        {
            var list = new List<CustomClass>();

            list.Add(new CustomClass { key = "test 1" });
            list.Add(new CustomClass { key = "test 2" });
            list.Add(new CustomClass { key = "test 3" });
            list.Add(new CustomClass { key = "test 4" });
            list.Add(new CustomClass { key = "test 1" });
            list.Add(new CustomClass { key = "test 2" });
            list.Add(new CustomClass { key = "test 3" });
            list.Add(new CustomClass { key = "test 4" });

            return list;
        }

        void PerformSearch<T>(SearchModule<T> searchModule, string searchString, List<T> listCached, List<T> list)
        {
            searchModule.searchString = searchString;
            listCached.Clear();
            listCached.AddRange(list);
            searchModule.PerformSearch(listCached);
        }

        class CustomClass
        {
            public string key;
        }
    }
}
