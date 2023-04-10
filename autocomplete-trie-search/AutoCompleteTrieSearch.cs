using autocomplete_trie_search.Interface;

namespace autocomplete_trie_search
{
    public class AutoCompleteTrieSearch
    {
        private const int DEFAULT_MISMATCH_ALLOW = 3;
        private const int DEFAULT_MAX_SUGGESION = 10;
        private const bool DEFAULT_IGNORE_CASE = true;
        private const int MAX_WEIGHT = 1000000000;

        private TrieNode _root;
        private int _maxSuggestion;
        private IDictionary<string, object> _valueByKey;
        private int _allowedMismatchCount;
        private bool _ignoreCase;
        private int _nodeCount;
        private Action<object> _onUpdateCallback;

        public AutoCompleteTrieSearch()
        {
            _valueByKey = new Dictionary<string, object>();
            _root = new TrieNode();
            _nodeCount = 0;
            _maxSuggestion = DEFAULT_MAX_SUGGESION;
            _ignoreCase = DEFAULT_IGNORE_CASE;
            _allowedMismatchCount = DEFAULT_MISMATCH_ALLOW;
        }

        public AutoCompleteTrieSearch(AutoCompleteTrieSearchOptions options)
        {
            _valueByKey = new Dictionary<string, object>();
            _root = new TrieNode();
            _nodeCount = 0;
            _maxSuggestion = options?.MaxSuggestion ?? DEFAULT_MAX_SUGGESION;
            _ignoreCase = options?.IgnoreCase ?? DEFAULT_IGNORE_CASE;
            _allowedMismatchCount = options?.AllowedMismatchCount ?? DEFAULT_MISMATCH_ALLOW;
        }

        public void UpdateOptions(AutoCompleteTrieSearchOptions options)
        {
            MaxSuggestion = options.MaxSuggestion ?? MaxSuggestion;
            AllowedMismatchCount = options.AllowedMismatchCount ?? AllowedMismatchCount;
        }

        public void OnUpdate(Action<object> callback)
        {
            _onUpdateCallback = callback;
        }
        
        private void onUpdateCallback(object value)
        {
            if (_onUpdateCallback != null)
            {
                _onUpdateCallback.Invoke(value);
            }
        }

        private int NodeCount
        {
            set => _nodeCount = value;
        }

        public int GetNodeCount()
        {
            return _nodeCount;
        }

        private int AllowedMismatchCount
        {
            set
            {
                if (DEFAULT_MISMATCH_ALLOW < value || value == 0)
                {
                    value = DEFAULT_MISMATCH_ALLOW;
                }

                _allowedMismatchCount = value;
            }
            get => _allowedMismatchCount;
        }

        public bool IgnoreCase => _ignoreCase;

        private TrieNode Root
        {
            set => _root = value;
        }

        public TrieNode GetRoot()
        {
            return _root;
        }

        private int MaxSuggestion
        {
            set => _maxSuggestion = value;
            get => _maxSuggestion;
        }

        public int GetMaxSuggestion()
        {
            return _maxSuggestion;
        }

        private IDictionary<string, object> ValueByKey
        {
            set
            {
                _valueByKey = value;
            }
            get
            {
                return _valueByKey;
            }
        }
        

        private void AddValueById(string key, object value)
        {
            if (!ValueByKey.ContainsKey(key))
            {
                ValueByKey.Add(key, value);
            }
        }

        private bool RemoveValueById(string key)
        {
            return ValueByKey.Remove(key);
        }

        public bool InsertOrUpdate(List<INodeValue> node)
        {
            var isInserted = false;
            foreach (var n in node)
            {
                if (Add(n))
                {
                    isInserted = true;
                }
            }
            return isInserted;
        }

        public bool InsertOrUpdate(INodeValue node)
        {
            return Add(node);
        }

        public bool Delete(INodeValue node)
        {
            var newNode = new NodeValue(node);
            return Remove(GetRoot(), newNode);
        }

        private bool Remove(TrieNode rootNode, NodeValue node, int index = 0)
        {
            // If the root node doesn't exist or the index is out of bounds, return false
            if (rootNode == null || index > node.Text.Length)
            {
                return false;
            }

            // If we've reached the end of the node's text, remove the node value from the root node
            if (index == node.Text.Length)
            {
                if (rootNode.NodeValue == null)
                {
                    return false;
                }
                RemoveValueById(rootNode.NodeValue.Id);
                node.Id = rootNode.NodeValue.Id;
                rootNode.NodeValue = null;
                RemoveFromRankList(rootNode, node);
                return true;
            }

            // Get the current character being removed
            char c = node.Text[index];

            // Get the child node corresponding to the current character
            TrieNode childNode = rootNode.Map.ContainsKey(c) ? rootNode.Map[c] : null;
            // If the child node doesn't exist, return false
            if (childNode == null)
            {
                return false;
            }

            // Recursively remove the remaining characters from the child node
            bool isDeleted = Remove(childNode, node, index + 1);

            // If the child node was successfully deleted, update the root node's `map`
            if (isDeleted)
            {
                if (childNode.NodeValue == null && childNode.Map.Count == 0)
                {
                    rootNode.Map.Remove(c);
                    NodeCount = GetNodeCount() - 1;
                }
                RemoveFromRankList(rootNode, node);
            }

            // Return the updated delete status
            return isDeleted;
        }

        private bool Add(INodeValue node)
        {
            // If node or node text is falsy or only whitespace, return false
            if (node == null || string.IsNullOrWhiteSpace(node.Text))
            {
                return false;
            }

            if (this.IgnoreCase)
            {
                node.Text = node.Text.Trim().ToLower();
            }

            // Create a new node value object from the given node and assign a unique ID
            var nodeValue = new NodeValue(node);
            // Insert the node value into the trie
            this.InsertInToTrie(this.GetRoot(), nodeValue);

            // Return true to indicate successful insertion
            return true;
        }

        private TrieNode InsertInToTrie(TrieNode rootNode, NodeValue node, int index = 0)
        {
            // If the root node doesn't exist or the index is out of bounds, return the root node
            if (rootNode == null || index > node.Text.Length)
            {
                return rootNode;
            }

            // If we've reached the end of the node's text, set the node value in the root node, update the rank list,
            // and return the root node
            if (index == node.Text.Length)
            {
                if (rootNode.NodeValue != null)
                {
                    RemoveValueById(rootNode.NodeValue.Id);
                    rootNode.OwnRank.Weight++;
                    rootNode.NodeValue.Value = node.Value;
                    rootNode.NodeValue.Weight = rootNode.OwnRank.Weight;
                    AddValueById(rootNode.NodeValue.Id, node.Value);
                    node.Weight = rootNode.OwnRank.Weight;
                    this.onUpdateCallback(node);
                }
                else
                {
                    rootNode.NodeValue = node;
                    rootNode.OwnRank.Id = node.Id;
                    rootNode.OwnRank.Weight = node.Weight;
                    AddValueById(node.Id, node.Value);
                }

                UpdateRankList(rootNode);
                return rootNode;
            }

            // Get the current character being inserted
            var c = node.Text[index];

            // Get the child node corresponding to the current character
            TrieNode childNode = rootNode.Map.ContainsKey(c) ? rootNode.Map[c] : null;

            // If the child node doesn't exist, create a new one and add it to the root node's map
            if (childNode == null)
            {
                childNode = new TrieNode();
                rootNode.Map[c] = childNode;
                NodeCount = GetNodeCount() + 1;
            }

            // Recursively insert the remaining characters into the child node
            childNode = InsertInToTrie(childNode, node, index + 1);

            // If the child node was successfully updated, merge its rank list with the root node's rank list
            if (childNode != null)
            {
                rootNode.RankList = MergeRankList(rootNode.RankList, childNode.RankList);
            }

            // Return the updated root node
            return rootNode;
        }


        public void RemoveFromRankList(TrieNode node, NodeValue ignoreNode)
        {
            if (ignoreNode == null)
            {
                return;
            }

            node.RankList = MergeRankList(node.RankList, new List<IRank>() , ignoreNode);
        }

        private void UpdateRankList(TrieNode node)
        {
            List<IRank> rankList = new List<IRank>();

            // Push the node's id and weight as a new object into the rank list
            if(node.OwnRank != null)
            {
                rankList.Add(node.OwnRank);
            }

            // Update the node's rank list with the new rank list
            node.RankList = MergeRankList(node.RankList, rankList);
        }

        public List<object> GetSuggestions(string text)
        {
            List<IRank> rankList = new List<IRank>();

            // If the text is empty or consists only of whitespace characters, use the root node's rank list
            if (string.IsNullOrWhiteSpace(text))
            {
                rankList = GetRoot().RankList;
            }
            else
            {
                // Otherwise, search for the node corresponding to the given text and get its rank list
                rankList = Search(GetRoot(), text);
            }

            List<object> suggestions = new List<object>();

            // For each rank in the rank list, get the corresponding node value and add it to the suggestions list
            foreach (IRank rank in rankList)
            {
                if (ValueByKey.TryGetValue(rank.Id, out object value))
                {
                    suggestions.Add(value);
                }
            }

            return suggestions;
        }

        private List<IRank> Search(TrieNode rootNode, string text, int mismatchCount = 0, int index = 0)
        {
            // If the rootNode is null, return null
            if (rootNode == null)
            {
                return null;
            }

            // If index has reached the end of the text and mismatchCount is zero,
            // return the node's rank as a single element list with the node's ID
            if (index == text.Length && mismatchCount == 0)
            {
                if (rootNode.NodeValue != null)
                {
                    Rank ranke = new Rank()
                    {
                        Weight = rootNode.OwnRank.Weight,
                        Id = rootNode.NodeValue.Id
                    };

                    return MergeRankList(new List<IRank>() { ranke }, rootNode.RankList);
                }
                else
                {
                    return rootNode.RankList;
                }
            }

            // If index has reached the end of the text, return the node's rankList
            if (index == text.Length)
            {
                return rootNode.RankList;
            }

            // Initialize an empty list for the rootRankList and get the current character
            List<IRank> rootRankList = new List<IRank>();
            char current = text[index];

            if (mismatchCount == this._allowedMismatchCount)
            {
                if (rootNode.Map.TryGetValue(current, out TrieNode childNode))
                {
                    List<IRank> resultRankList = Search(childNode, text, mismatchCount, index + 1);
                    if (resultRankList != null)
                    {
                        rootRankList = MergeRankList(rootRankList, resultRankList);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<char, TrieNode> entry in rootNode.Map)
                {
                    char key = entry.Key;
                    TrieNode childNode = entry.Value;

                    if (mismatchCount < this._allowedMismatchCount)
                    {
                        List<IRank> resultRankList = Search(childNode, text, key == current ? mismatchCount : mismatchCount + 1, index + 1);
                        if (resultRankList != null)
                        {
                            rootRankList = MergeRankList(rootRankList, resultRankList);
                        }
                    }
                }
            }

            // Return the rootRankList
            return rootRankList;
        }

        private List<IRank> MergeRankList(List<IRank> list1, List<IRank> list2, NodeValue ignoreNode = null)
        {
            var mergedList = new List<IRank>();

            if(list1 == null)
            {
                list1 = new List<IRank>();
            }
            if(list2 == null)
            {
                list2 = new List<IRank>();
            }
            
            var i = 0; // Pointer for list1
            var j = 0; // Pointer for list2

            // Merge the two lists until we reach the maximum number of suggestions
            while (mergedList.Count < this.MaxSuggestion && i < list1.Count() && j < list2.Count())
            {
                // Compare the rank of the elements at the current position of the pointers
                if (list1[i].Id == list2[j].Id)
                {
                    i++;
                }
                else if (ignoreNode != null && list1[i].Id == ignoreNode.Id)
                {
                    i++;
                }
                else if (ignoreNode != null && list2[j].Id == ignoreNode.Id)
                {
                    j++;
                }
                else if (list1[i].Weight >= list2[j].Weight)
                {
                    mergedList.Add(list1[i]);
                    i++;
                }
                else
                {
                    mergedList.Add(list2[j]);
                    j++;
                }
            }

            // Add the remaining elements from list1, if any
            while (mergedList.Count < this.MaxSuggestion && i < list1.Count())
            {
                if (ignoreNode?.Id == list1[i].Id)
                {
                    i++;
                    continue;
                }

                mergedList.Add(list1[i]);
                i++;
            }

            // Add the remaining elements from list2, if any
            while (mergedList.Count < this.MaxSuggestion && j < list2.Count())
            {
                if (list2[j].Id == ignoreNode?.Id)
                {
                    j++;
                    continue;
                }

                mergedList.Add(list2[j]);
                j++;
            }

            return mergedList;
        }

        public void Clear()
        {
            Root = new TrieNode();
            ValueByKey = new Dictionary<string, object>();
            NodeCount = 0;
            MaxSuggestion = MaxSuggestion > 0 ? MaxSuggestion : DEFAULT_MAX_SUGGESION;
            AllowedMismatchCount = AllowedMismatchCount > 0 ? AllowedMismatchCount : DEFAULT_MISMATCH_ALLOW;
        }
    }
}