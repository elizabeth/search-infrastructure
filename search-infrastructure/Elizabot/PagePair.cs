using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elizabot
{
    public class PagePair
    {
        public PageEntity page;
        public int count;
        public HashSet<string> queryWords;

        public PagePair(PageEntity page, int count, string word)
        {
            this.page = page;
            this.count = count;
            queryWords = new HashSet<string>();
            queryWords.Add(word);
        }
    }
}
