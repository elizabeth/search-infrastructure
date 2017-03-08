using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridTrie
{
    // Trie data structure
    public class Trie
    {
        private Node root;
        private Dictionary<string, int> searches;
        private readonly int _max = 20;     //max number of strings in hybrid trie per node
        private readonly int _maxList = 10; //max number of suggestions to return
        private readonly int _maxDist = 2;  //max distance misspelling between two words

        //constructor
        public Trie()
        {
            root = new Node('\0');
            searches = new Dictionary<string, int>();
        }

        //build trie
        public void insert(string term)
        {
            term = term.Trim().ToLower().Replace('_', ' ');
            char[] chars = term.ToCharArray();
            Node curr = root;

            if (curr.words.Count < _max && curr.children.Count == 0)
            {
                curr.words.Add(term);
            }
            else if (curr.words.Count == _max)
            {
                foreach (string word in curr.words)
                {
                    insertTrieWord(curr, word);
                }
                curr.words.Clear();
                insertTrieWord(curr, term);
            } else
            {
                insertTrieWord(curr, term);
            }
        }

        //build trie
        private void insertTrieWord(Node current, string word)
        {
            Node curr = current;
            char[] chars = word.ToCharArray();
            char letter;
            int index;
            Node temp;
            string sub;

            for (int i = 0; i < chars.Length; i++)
            {
                letter = chars[i];

                index = curr.children.FindIndex(node => node.value == letter);
                if (index >= 0)
                {
                    curr = curr.children[index];
                }
                else
                {
                    temp = new Node(letter);
                    curr.children.Add(temp);
                    curr = temp;
                }

                if (i == chars.Length - 1)
                {
                    curr.isEnd = true;
                }
                else
                {
                    sub = word.Substring(i+1);
                    if (curr.words.Count < _max && curr.children.Count == 0)
                    {
                        curr.words.Add(sub);
                        break;
                    } else if (curr.words.Count == _max)
                    {
                        foreach (string each in curr.words)
                        {
                            insertTrieWord(curr, each);
                        }
                        curr.words.Clear();
                        insertTrieWord(curr, sub);
                        break;
                    }
                }
            }
        }

        //save user's search
        public void saveSearch(string term)
        {
            term = term.Trim().ToLower();

            //make sure same word is not added twice
            if (searches.ContainsKey(term))
            {
                searches[term]++;
            }
            else
            {
                searches.Add(term, 1);
            }
        }

        //search for term in trie
        public Dictionary<string, Boolean> query(string term)
        {
            term = term.Trim().ToLower();
            char[] chars = term.ToCharArray();
            //List<string> list = new List<string>();
            Dictionary<string, Boolean> dict = new Dictionary<string, Boolean>();
            string word = "";
            Node curr = root;
            Node last = null;
            char letter;
            int index;

            foreach (var found in (searches.Where(s => s.Key.StartsWith(term))).ToList().OrderByDescending(key => key.Value))
            {
                //make sure duplicate words are not added
                if (!dict.ContainsKey(word))
                {
                    dict.Add(found.Key, true);
                }

                if (dict.Count >= 10)
                {
                    return dict;
                }
            }

            //go through each character in the search term
            for (int i = 0; i < chars.Length; i++)
            {
                letter = chars[i];

                index = curr.children.FindIndex(node => node.value == letter);
                if (index >= 0)
                {
                    curr = curr.children[index];

                    if (i == chars.Length - 1)
                    {
                        last = curr;
                    } else
                    {
                        word += curr.value;
                    }
                }
                else if (curr.words.Any())
                {
                    string temp;
                    foreach (string each in curr.words)
                    {
                        temp = word + each;
                        if (temp.StartsWith(term))
                        {
                            if (!dict.ContainsKey(temp))
                            {
                                dict.Add(temp, false);

                                if (dict.Count >= _maxList)
                                {
                                    return dict;
                                }
                            }
                        }
                    }
                    break;
                } else
                {
                    break;
                }
            }

            //if prefix is found
            if (last != null)
            {
                //search
                dict = queryNode(last, dict, word, term);
            }

            //get additional possible misspelled words
            if (dict.Count < _maxList)
            {
                var temp = "";
                curr = root;
                //assume first few letters is not mistyped based on word length
                for (int i = 0; i <= term.Count() / 4; i++)
                {
                    letter = chars[i];
                    if (root.children.Exists(node => node.value == chars[i]))
                    {
                        curr = curr.children.Find(node => node.value == chars[i]);
                        if (i != term.Count() / 4)
                        {
                            temp += letter;
                        }
                    } else
                    {
                        break;
                    }
                }
                checkMisspelled(curr, dict, temp, term);
            }

            //Dictionary<string, int> dict = new Dictionary<string, int>();
            ////check user searches first and add
            //foreach (string listWord in list)
            //{
            //    dict.Add(listWord, searches.ContainsKey(listWord) ? searches[listWord] : 0);
            //}

            return dict;        
        } 

        //do dfs to get query suggestions
        private Dictionary<string, Boolean> queryNode(Node node, Dictionary<string, Boolean> dict, string word, string search)
        {
            if (dict.Count < _maxList)
            {
                word += node.value;

                if (node.isEnd)
                {
                    if (!dict.ContainsKey(word))
                    {
                        dict.Add(word, false);
                    }
                }

                foreach (Node child in node.children)
                {
                    if (dict.Count >= _maxList)
                    {
                        break;
                    } else
                    {
                        dict = queryNode(child, dict, word, search);
                    }
                }

                string temp;
                foreach (string term in node.words)
                {
                    if (dict.Count >= _maxList)
                    {
                        break;
                    } else
                    {
                        temp = word + term;
                        if (temp.StartsWith(search))
                        {
                            if (!dict.ContainsKey(word))
                            {
                                dict.Add(temp, false);
                            }
                        }
                    }
                }
            }
            return dict;
        }

        //get possible misspelling suggestions
        private Dictionary<string, Boolean> checkMisspelled(Node node, Dictionary<string, Boolean> dict, string word, string search)
        {
            if (dict.Count < _maxList)
            {
                word += node.value;

                int dist = distance(word, search);

                if (node.isEnd)
                {
                    if (dist <= _maxDist)
                    {
                        //check duplicated words are not added
                        if (!dict.ContainsKey(word))
                        {
                            dict.Add(word, false);
                        }
                    }
                }

                foreach (Node child in node.children)
                {
                    if (dict.Count >= _maxList)
                    {
                        break;
                    } else
                    { 
                        dict = checkMisspelled(child, dict, word, search);
                    }
                }

                string temp;
                foreach (string term in node.words)
                {
                    temp = word + term;
                    if (dict.Count >= _maxList)
                    {
                        break;
                    }
                    else
                    {
                        if (distance(temp, search) <= _maxDist)
                        {
                            //check duplicated words are not added
                            if (!dict.ContainsKey(temp))
                            {
                                dict.Add(temp, false);
                            }
                        }
                    }
                }
            }

            return dict;
        }

        //from https://gist.github.com/Davidblkx/e12ab0bb2aff7fd8072632b396538560
        //returns the edit distance between two words
        private int distance(string source1, string source2) //O(n*m)
        {
            var source1Length = source1.Length;
            var source2Length = source2.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

            // Calculate rows and collumns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            // return result
            return matrix[source1Length, source2Length];
        }
    }
}
