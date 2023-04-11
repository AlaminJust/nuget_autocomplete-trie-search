namespace autocomplete_trie_search.Interface
{
    /// <summary>
    /// Interface for a rank.
    /// </summary>
    public interface IRank
    {
        /// <summary>
        /// Gets or sets the weight of the rank.
        /// </summary>
        int Weight { get; set; }

        /// <summary>
        /// Gets or sets the ID associated with the rank.
        /// </summary>
        string Id { get; set; }
    }
    
    /// <summary>
    /// Interface for a node value.
    /// </summary>
    public interface INodeValue
    {
        /// <summary>
        /// Gets or sets the text associated with the node value.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets the value associated with the node.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Gets or sets the weight of the node value.
        /// </summary>
        int Weight { get; set; }
    }

    /// <summary>
    /// Interface for a trie node.
    /// </summary>
    public interface ITrieNode
    {
        /// <summary>
        /// Gets or sets the rank list for the node.
        /// </summary>
        List<IRank> RankList { get; set; }

        /// <summary>
        /// Gets or sets the map of child nodes for the node.
        /// </summary>
        Dictionary<string, ITrieNode> Map { get; set; }

        /// <summary>
        /// Gets or sets the node value for the node.
        /// </summary>
        INodeValue NodeValue { get; set; }

        /// <summary>
        /// Gets or sets the own rank of the node.
        /// </summary>
        IRank OwnRank { get; set; }
    }
    
    public interface AutoCompleteTrieSearchOptions
    {
        public int? MaxSuggestion { get; set; }
        public int? AllowedMismatchCount { get; set; }
        public bool? IgnoreCase { get; set; }
    }

    /// <summary>
    /// Interface for an auto-complete trie search.
    /// </summary>
    public interface IAutoCompleteTrieSearch
    {
        /// <summary>
        /// Gets the root node of the trie.
        /// </summary>
        /// <returns>The root node of the trie.</returns>
        TrieNode GetRoot();
        
        /// <summary>
        /// Gets the number of nodes in the trie.
        /// </summary>
        /// <returns>The number of nodes in the trie.</returns>
        int GetNodeCount();

        /// <summary>
        /// Inserts or updates a list of nodes in the trie.
        /// </summary>
        /// <param name="node">The list of nodes to insert or update.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        bool InsertOrUpdate(List<INodeValue> node);

        /// <summary>
        /// Inserts or updates a node in the trie.
        /// </summary>
        /// <param name="node">The node to insert or update.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        bool InsertOrUpdate(INodeValue node);

        /// <summary>
        /// Deletes a node from the trie.
        /// </summary>
        /// <param name="node">The node to delete.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>  
        bool Delete(INodeValue node);

        /// <summary>
        /// Gets a list of suggestions for a given search text.
        /// </summary>
        /// <param name="text">The search text.</param>
        /// <returns>A list of suggestions.</returns>
        List<object> GetSuggestions(string text);

        /// <summary>
        /// Updates the options for the auto-complete trie search.
        /// </summary>
        /// <param name="options">The new options.</param>
        void UpdateOptions(AutoCompleteTrieSearchOptions options);

        /// <summary>
        /// Clears the trie of all nodes.
        /// </summary>
        void Clear();

        /// <summary>
        /// Sets a callback to be invoked whenever the trie is updated.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        void OnUpdate(Action<object> callback);
    }
}
