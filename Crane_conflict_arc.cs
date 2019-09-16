using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInputdata
{
    public class Crane_conflict_arc
    {
        public List<CraneNode> crane_node_conflict_list = new List<CraneNode>();
        public List<int> crane_node_conflict_time_index_list = new List<int>();
        public List<CraneLink> crane_link_conflict_list = new List<CraneLink>();
        public List<int> crane_link_conflict_time_index_list = new List<int>();
    }
}
