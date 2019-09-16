using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInputdata
{
    public class Link
    {
        public string name;
        public int id;
        public int from_node_id;
        public int to_node_id;
        public double cost;
        public int travle_time;
        public int capacity;
        public int type; //TruckLink: 1-moving 2-gate transaction 3-handling(coupling)
                         //CraneLink: 1-moving 2-handling(coupling) 3-recovery
    }

    public class TruckLink : Link
    {
        public TruckNode from_node;
        public TruckNode to_node;
        public int coupled_crane_link_id;
        public List<int> LR_at_t_same_num = new List<int>();

        public int avaiable_time;
        public int current_volume;

        public List<double> time_dependent_ADMM_LR_coupling_multiplier = new List<double>();//coupling pai
        public List<double> time_dependent_ADMM_LR_transaction_multiplier = new List<double>();//gate capacity lamda

        public List<double> time_dependent_pure_LR_coupling_multiplier = new List<double>();//coupling pai
        public List<double> time_dependent_pure_LR_trasaction_multiplier = new List<double>();//gate capacity lamda

        public List<double> time_dependent_ADMM_coupling_sub_grandient = new List<double>();
        public List<double> time_dependent_ADMM_coupling_last_sub_grandient = new List<double>();
        public List<double> time_dependent_ADMM_transaction_sub_grandient = new List<double>();
        public List<double> time_dependent_ADMM_transaction_last_sub_grandient = new List<double>();

    }

    public class CraneLink : Link
    {
        public CraneNode from_node;
        public CraneNode to_node;
        public int coupled_truck_link_id;

        public int avaiable_time;
        public int current_volume;

        public List<Crane_conflict_arc> CCA_list = new List<Crane_conflict_arc>();
        public List<List<Arc>> incompatible_arc_list_t = new List<List<Arc>>();

        public List<double> time_denpendent_ADMM_LR_coupling_multiplier = new List<double>();//coupling pai
        public List<List<double>> time_dependent_ADMM_LR_non_crossing_multiplier_each_c = new List<List<double>>();//non-crossing miu

        public List<double> time_dependent_pure_LR_coupling_multiplier = new List<double>();//coupling pai
        public List<List<double>> time_dependent_pure_LR_no_crossing_multiplier_c = new List<List<double>>();//non-crossing miu

        public List<List<double>> time_dependent_ADMM_sub_grandient = new List<List<double>>();
        public List<List<double>> time_dependent_ADMM_last_sub_grandient = new List<List<double>>();
    }
}
