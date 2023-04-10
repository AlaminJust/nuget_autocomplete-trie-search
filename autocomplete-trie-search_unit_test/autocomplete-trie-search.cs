using autocomplete_trie_search;
using autocomplete_trie_search.Interface;
using System.Diagnostics;

namespace autocomplete_trie_search_unit_test
{
    public class Tests
    {
        private AutoCompleteTrieSearch search;
        
        [SetUp]
        public void Setup()
        {
            search = new AutoCompleteTrieSearch();
        }

        [Test]
        public void InsertBySingleElement()
        {
            INodeValue node = new NodeValueOptions()
            {
                Text = "Some text",
                Value = new {Id = 1, Text = "I am okay"},
                Weight = 10
            };

            Assert.IsTrue(search.InsertOrUpdate(node));
        }

        [Test]
        public void InsertByMultipleElement()
        {
            List<INodeValue> nodes = new List<INodeValue>();
           
            for(int i = 0; i<=100000; i++)
            {
                INodeValue node = new NodeValueOptions()
                {
                    Text = Guid.NewGuid().ToString(),
                    Value = new { Id = 1, Text = "I am okay" },
                    Weight = 10
                };
                
                nodes.Add(node);
            }

            Assert.IsTrue(search.InsertOrUpdate(nodes));
        }


        [Test]
        public void Insert100000ElementTimeComplexity()
        {
            List<INodeValue> nodes = new List<INodeValue>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i <= 100000; i++)
            {
                INodeValue node = new NodeValueOptions()
                {
                    Text = Guid.NewGuid().ToString(),
                    Value = new { Id = 1, Text = "I am okay" },
                    Weight = 10
                };

                nodes.Add(node);
            }
            
            search.InsertOrUpdate(nodes);
            stopwatch.Stop();

            
            Assert.LessOrEqual(stopwatch.ElapsedMilliseconds, 12000);
        }

        [Test]
        public void Insert10000ElementTimeComplexity()
        {
            List<INodeValue> nodes = new List<INodeValue>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i <= 10000; i++)
            {
                INodeValue node = new NodeValueOptions()
                {
                    Text = Guid.NewGuid().ToString(),
                    Value = new { Id = 1, Text = "I am okay" },
                    Weight = 10
                };

                nodes.Add(node);
            }

            search.InsertOrUpdate(nodes);
            stopwatch.Stop();


            Assert.LessOrEqual(stopwatch.ElapsedMilliseconds, 2000);
        }


        [Test]
        public void NodeCountFor10000Element()
        {
            List<INodeValue> nodes = new List<INodeValue>();

            for (int i = 0; i <= 100000; i++)
            {
                INodeValue node = new NodeValueOptions()
                {
                    Text = Guid.NewGuid().ToString().Substring(0,16),
                    Value = new { Id = 1, Text = "I am okay" },
                    Weight = 10
                };

                nodes.Add(node);
            }

            search.InsertOrUpdate(nodes);
            Assert.LessOrEqual(search.GetNodeCount(), 800000);
        }

        [Test]
        public void MemoryUsageFor10000Element()
        {
            Process process = Process.GetCurrentProcess();
            long startMemory = process.WorkingSet64;

            List<INodeValue> nodes = new List<INodeValue>();

            for (int i = 0; i <= 100000; i++)
            {
                INodeValue node = new NodeValueOptions()
                {
                    Text = Guid.NewGuid().ToString().Substring(0, 16),
                    Value = new { Id = 1, Text = "I am okay" },
                    Weight = 10
                };

                nodes.Add(node);
            }

            search.InsertOrUpdate(nodes);

            process = Process.GetCurrentProcess();
            long endMemory = process.WorkingSet64;
            long memoryUsed = endMemory - startMemory;

            Console.WriteLine("Memory used: {0:N0} bytes", memoryUsed);

            Assert.LessOrEqual(memoryUsed, 700 * 1024 * 1024);
        }
    }
}