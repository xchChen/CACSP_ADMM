using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInputdata
{
    public class Node
    {
        public int id;
        public int name_code;
        public string description;
        public double x;
        public double y;
        public List<Node> Ingoing_NodeList = new List<Node>();
        public List<Node> Outgoing_NodeList = new List<Node>();
        public List<Link> Ingoing_LinkList = new List<Link>();
        public List<Link> Outgoing_LinkList = new List<Link>();
    }

    public class TruckNode : Node
    {
        public double cost;
    }

    public class CraneNode : Node
    {
        public double cost;

        public List<List<Arc>> incompatible_arc_list_t = new List<List<Arc>>();

        public List<List<double>> time_dependent_ADMM_LR_non_crossing_multiplier_each_c = new List<List<double>>();//non-crossing miu
        public List<List<double>> time_dependent_pure_LR_no_crossing_multiplier_c = new List<List<double>>();//non-crossing miu

        public List<List<double>> time_dependent_ADMM_sub_grandient = new List<List<double>>();
        public List<List<double>> time_dependent_ADMM_last_sub_grandient = new List<List<double>>();
    }
}
