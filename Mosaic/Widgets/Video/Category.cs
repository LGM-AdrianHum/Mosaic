using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Video
{
    public class Category
    {
        public char Title;
        public List<string> Files;

        public Category()
        {
            Files = new List<string>();
        }
    }
}
