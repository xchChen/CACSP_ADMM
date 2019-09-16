using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInputdata
{
    public class Arc
    {
        public int I;
        public int J;
        public int T;
        public int S;
        public double cost;
        public int Tag;

        public List<Vertex> CVF_vertex_list = new List<Vertex>();

        public Arc(int II, int JJ, int TT, int SS)
        {
            this.I = II;
            this.J = JJ;
            this.T = TT;
            this.S = SS;
        }
    }
}
