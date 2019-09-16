using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInputdata
{
    public class Vertex
    {
        public int I;
        public int T;
        public int STN_tag;
        public List<Arc> Connected_Arc_List = new List<Arc>();

        public Vertex(int II,int TT,int TAG)
        {
            this.I = II;
            this.T = TT;
            this.STN_tag = TAG;
        }
        public Vertex(int II, int TT)
        {
            this.I = II;
            this.T = TT;
        }

    }
}
