using autocomplete_trie_search.Interface;

namespace autocomplete_trie_search
{
    public class TrieNode 
    { 
        public List<IRank> RankList { get; set; }
        public Dictionary<char, TrieNode> Map { get; set; }
        public NodeValue NodeValue { get; set; }
        public Rank OwnRank { get; set; }

        public TrieNode()
        {
            RankList = new List<IRank>();
            Map = new Dictionary<char, TrieNode>();
            NodeValue = null;
            OwnRank = new Rank();
        }
    }
    

    public class NodeValue: INodeValue
    {
        private const int DEFAULT_WEIGHT = 1;
        private string _id;

        public string Text { get; set; }
        public object Value { get; set; }
        public int Weight { get; set; }

        public NodeValue(INodeValue options)
        {
            if (string.IsNullOrWhiteSpace(options.Text))
            {
                throw new Exception("Text can't be empty");
            }

            Text = options.Text;
            Value = options.Value ?? options.Text;
            Weight = options.Weight == 0 ? DEFAULT_WEIGHT : options.Weight;
            Id = Guid.NewGuid().ToString();
        }

        public string Id
        {
            set { _id = value; }
            get { return _id; }
        }
    }

    public class Rank: IRank
    {
        public int Weight { get; set; }
        public string Id
        {
            get; set;
        }

        public Rank()
        {
            Weight = 0;
            Id = Guid.NewGuid().ToString();
        }
    }

    public class NodeValueOptions: INodeValue
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public int Weight { get; set; }
    }
}