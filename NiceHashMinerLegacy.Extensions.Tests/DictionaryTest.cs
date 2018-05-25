using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NiceHashMinerLegacy.Extensions.Tests
{
    [TestClass]
    public class DictionaryTest
    {
        [TestMethod]
        public void ConcatGenericDict_ShouldMatch()
        {
            var dict1 = new Dictionary<int, string>
            {
                { 1, "a" },
                { 2, "b" }
            };
            var dict2 = new Dictionary<int, string>
            {
                { 3, "c" },
                { 4, "b" }
            };
            var dict3 = new Dictionary<int, string>
            {
                { 5, "e" },
                { 6, "c" }
            };

            var dicts = new List<Dictionary<int, string>>
            {
                dict1,
                dict3
            };

            var dict = dict1.ConcatDict(dict2, dict3);
            Assert.AreEqual(6, dict.Count);
            var dicte = dict2.ConcatDict(dicts);
            Assert.AreEqual(6, dicte.Count);

            dicts.Add(dict2);

            foreach (var key in dict.Keys)
            {
                Assert.AreEqual(dict[key], dicte[key]);
            }

            foreach (var d in dicts)
            {
                foreach (var key in d.Keys)
                {
                    Assert.AreEqual(d[key], dict[key]);
                }
            }
        }

        [TestMethod]
        public void ConcatListDict_ShouldMatch()
        {
            var dict1 = new Dictionary<int, List<string>>
            {
                { 1, new List<string> { "a", "b" } },
                { 2, new List<string> { "c", "d" } }
            };
            var dict2 = new Dictionary<int, List<string>>
            {
                { 3, new List<string> { "h", "z" } },
                { 2, new List<string> { "j", "s" } }
            };
            var dict3 = new Dictionary<int, List<string>>
            {
                { 1, new List<string> { "q", "b" } },
                { 6, new List<string> { "yh", "asdf" } }
            };

            var dicts = new List<Dictionary<int, List<string>>>
            {
                dict2,
                dict3
            };

            var list1 = new List<string>
            {
                "a",
                "b",
                "q",
                "b"
            };
            var list2 = new List<string>
            {
                "c",
                "d",
                "j",
                "s"
            };

            var dict = dict1.ConcatDictList(dict2, dict3);
            Assert.AreEqual(4, dict.Count);
            var dicte = dict1.ConcatDictList(dicts);
            Assert.AreEqual(4, dicte.Count);

            foreach (var key in dict.Keys)
            {
                CollectionAssert.AreEqual(dict[key], dicte[key]);
            }

            foreach (var d in dicts)
            {
                foreach (var key in d.Keys)
                {
                    if (key == 1)
                    {
                        CollectionAssert.AreEqual(list1, dict[key]);
                    }
                    else if (key == 2)
                    {
                        CollectionAssert.AreEqual(list2, dict[key]);
                    }
                    else
                    {
                        CollectionAssert.AreEqual(d[key], dict[key]);
                    }
                }
            }
        }
    }
}
