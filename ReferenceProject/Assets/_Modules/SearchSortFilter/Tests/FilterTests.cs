using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Unity.ReferenceProject.SearchSortFilter.Tests
{
    public class FilterTests
    {
        [Test]
        public void Correctness_Equals()
        {
            var list = CreateList();

            // Test0 = 6
            // Test1 = 4
            var module = new FilterModule<CustomClass>((nameof(CustomClass.key), new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.Equals)));

            var listCached = new List<CustomClass>();

            module.AddSelectedOption(nameof(CustomClass.key), "test0");
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(6, listCached.Count);

            module.ClearAllOptions();
            module.AddSelectedOption(nameof(CustomClass.key), "test1");
            listCached.Clear();
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(4, listCached.Count);
        }

        [Test]
        public void List_NullOrZeroCount()
        {
            List<CustomClass> list = null;

            var module = new FilterModule<CustomClass>((nameof(CustomClass.key), new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.Equals)));

            module.AddSelectedOption(nameof(CustomClass.key), "0");
            module.PerformFiltering(list);
            Assert.AreEqual(null, list);

            list = new List<CustomClass>();
            module.PerformFiltering(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void String_NullOrEmptyOrSpace()
        {
            var list = new List<CustomClass>();

            list.Add(new CustomClass { key = "test 1" });
            list.Add(new CustomClass { key = "test 2" });

            var module = new FilterModule<CustomClass>((nameof(CustomClass.key), new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.Equals)));

            var listCached = new List<CustomClass>();

            module.AddSelectedOption(nameof(CustomClass.key), null);
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(2, listCached.Count);

            module.ClearAllOptions();
            module.AddSelectedOption(nameof(CustomClass.key), "");
            listCached.Clear();
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(2, listCached.Count);

            module.ClearAllOptions();
            module.AddSelectedOption(nameof(CustomClass.key), " ");
            listCached.Clear();
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(0, listCached.Count);
        }

        [Test]
        public void EmptyResult()
        {
            var list = new List<CustomClass>();

            list.Add(new CustomClass { key = "test 1" });
            list.Add(new CustomClass { key = "test 2" });
            list.Add(new CustomClass { key = "test 3" });
            list.Add(new CustomClass { key = "test 4" });

            var module = new FilterModule<CustomClass>((nameof(CustomClass.key), new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.Equals)));

            var listCached = new List<CustomClass>();

            module.AddSelectedOption(nameof(CustomClass.key), "emptyResult");
            listCached.AddRange(list);
            module.PerformFiltering(listCached);
            Assert.AreEqual(0, listCached.Count);
        }
        
        [Test]
        public async Task AsyncCancellation()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var list = CreateList();

            var module = new FilterModule<CustomClass>(("filterKey", new FilterBindNode<CustomClass>(c => c.key, FilterCompareType.Equals)));
            module.AddSelectedOption("filterKey", "test1");

            int listInitialCount = list.Count;
            
            tokenSource.Cancel();
            
            try
            {
                await module.PerformFiltering(list, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // We are expecting to catch this
                Debug.LogWarning($"Operation has been canceled");
            }
            
            tokenSource.Dispose();

            Assert.AreEqual(listInitialCount, list.Count);

            tokenSource = new CancellationTokenSource();
            await module.PerformFiltering(list, tokenSource.Token);
            tokenSource.Dispose();
            Assert.AreEqual(4, list.Count);
            
        }
        
        // test1 - 4
        // test0 - 6
        List<CustomClass> CreateList()
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

            return list;
        }

        class CustomClass
        {
            public string key;
        }
    }
}
