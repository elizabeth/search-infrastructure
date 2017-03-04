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

        public PagePair(PageEntity page, int count)
        {
            this.page = page;
            this.count = count;
        }
    }
}
