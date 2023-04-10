namespace autocomplete_trie_search.Interface
{
    public interface IRank
    {
        int Weight { get; set; }
        string Id { get; set; }
    }
    
    public interface INodeValue
    {
        string Text { get; set; }
        object Value { get; set; }
        int Weight { get; set; }
    }

    public interface ITrieNode
    {
        List<IRank> RankList { get; set; }
        Dictionary<string, ITrieNode> Map { get; set; }
        INodeValue NodeValue { get; set; }
        IRank OwnRank { get; set; }
    }
    
    public interface AutoCompleteTrieSearchOptions
    {
        public int? MaxSuggestion { get; set; }
        public int? AllowedMismatchCount { get; set; }
        public bool? IgnoreCase { get; set; }
    }
}
