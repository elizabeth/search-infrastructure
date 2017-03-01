using System;
using System.Collections.Generic;

namespace HybridTrie
{
    // Trie Node data structure
    public class Node
    {
        public char value { get; private set; }
        public List<string> words { get; set; }     //for hybrid
        public List<Node> children { get; set; }
        public Boolean isEnd { get; set; }

        public Node(char value)
        {
            this.value = value;
            this.words = new List<string>();
            this.children = new List<Node>();
            this.isEnd = false;
        }
    }
}
