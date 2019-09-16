using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInputdata
{
    public class Truck
    {
        public int id;
        public int Type;//=1 loading =2 unloading
        public int ContainerBayID;
        public int ContainerBay_stateID;
        public int HandlingCost;
        public List<Arc> ArcList_Pool = new List<Arc>();
        public List<Arc> ArcList = new List<Arc>();
        public List<Arc> arclist_ADMM_path = new List<Arc>();
        public List<Arc> arclist_LR_path = new List<Arc>();
        public List<TruckLink> available_link_list = new List<TruckLink>();
        public List<TruckNode> available_node_list = new List<TruckNode>();
        public List<TruckLink> shortestpath_link_list = new List<TruckLink>();
        public List<TruckNode> shortestpath_node_list = new List<TruckNode>();
        public int OriginI;
        public int OriginT;
        public int DestinationI;
        public int DestinationT;
        public int lower_bound_travel_time;

        public List<TruckLink> path_link_list = new List<TruckLink>();
        public int ready_to_move_time;
        public int move_arrive_time;
        public int link_path_index;
        public Crane corresponding_crane = new Crane();
        public CraneLink handling_coupling_link = new CraneLink();
        public List<Vertex> sim_output_vertex_list = new List<Vertex>();
        public List<Arc> sim_arc_list_for_drawing = new List<Arc>();
        public bool process_finished=false;

        public Truck(int ID,int OI,int OT,int DI,int DT, int CONBAYID,int CONSTAGEID,int HANCOST)
        {
            this.id = ID;
            this.OriginI = OI;
            this.OriginT = OT;
            this.DestinationI = DI;
            this.DestinationT = DT;
            this.ContainerBayID = CONBAYID;
            this.ContainerBay_stateID = CONSTAGEID;
            this.HandlingCost = HANCOST;
        }
        public Truck()
        { }
    }
}
