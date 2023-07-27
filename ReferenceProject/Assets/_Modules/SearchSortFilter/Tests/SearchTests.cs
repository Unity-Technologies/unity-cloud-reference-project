using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
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
        public void Correctness()
        {
            PerformSearch(m_SearchModule, "1", m_CashedList, m_TestList);
            Assert.AreEqual(2, m_CashedList.Count);

            PerformSearch(m_SearchModule, "2", m_CashedList, m_TestList);
            Assert.AreEqual(2, m_CashedList.Count);
        }

        [Test]
        public void List_NullOrZeroCount()
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
        public void String_NullOrEmptyOrSpace()
        {
            PerformSearch(m_SearchModule, null, m_CashedList, m_TestList);
            Assert.AreEqual(8, m_CashedList.Count);

            PerformSearch(m_SearchModule, "", m_CashedList, m_TestList);
            Assert.AreEqual(8, m_CashedList.Count);

            PerformSearch(m_SearchModule, " ", m_CashedList, m_TestList);
            Assert.AreEqual(8, m_CashedList.Count);
        }

        [Test]
        public void AllContainSearchString()
        {
            PerformSearch(m_SearchModule, "test", m_CashedList, m_TestList);
            Assert.AreEqual(8, m_CashedList.Count);
        }

        [Test]
        public void EmptyResult()
        {
            PerformSearch(m_SearchModule, "emptyResult", m_CashedList, m_TestList);
            Assert.AreEqual(0, m_CashedList.Count);
        }
        
        [Test]
        public async Task AsyncCancellation()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            m_SearchModule.searchString = "test 4";
            m_CashedList.Clear();
            m_CashedList.AddRange(CreateHugeTestList());
            
            tokenSource.Cancel();
            
            try
            {
                await m_SearchModule.PerformSearch(m_CashedList, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // We are expecting to catch this
                Debug.LogWarning($"Operation has been canceled");
            }
            
            tokenSource.Dispose();

            Assert.AreNotEqual(0, m_CashedList.Count);
            
            tokenSource = new CancellationTokenSource();
            await m_SearchModule.PerformSearch(m_CashedList, tokenSource.Token);
            tokenSource.Dispose();
            
            Assert.AreNotEqual(2, m_CashedList.Count);
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
        
        List<CustomClass> CreateHugeTestList()
        {
            var list = new List<CustomClass>();

            for (int i = 0; i < 1000000; i++)
            {
                list.Add(new CustomClass { key = "test 1" });
                list.Add(new CustomClass { key = "test 2" });
                list.Add(new CustomClass { key = "test 3" });
                list.Add(new CustomClass { key = "test 4" });
            }
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
