using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInputdata
{
    public class Crane
    {
        public int id;
        public List<Arc> ArcList = new List<Arc>();
        public int OriginI;
        public int OriginT;
        public int DestinationI;
        public int DestinationT;
        public List<CraneNode> available_node_list = new List<CraneNode>();
        public List<Arc> arc_list_ADMM_path = new List<Arc>();
        public List<Arc> arc_list_LR_path = new List<Arc>();
        public List<Arc> arc_moving_list = new List<Arc>();
        public int lower_bound_travel_time;
        public int ready_to_move_time;
        public int move_arrive_time;
        public List<Truck> truck_service_list = new List<Truck>();
        public int truck_service_list_index;
        public CraneNode current_node = new CraneNode();
        public List<CraneLink> path_link_list = new List<CraneLink>();
        public int path_link_list_index;
        public List<Vertex> sim_output_vertex_list = new List<Vertex>();
        public List<Arc> sim_arc_list_for_drawing = new List<Arc>();
        public bool process_finished = false;

        public Crane(int ID, int OI, int OT, int DI, int DT)
        {
            this.id = ID;
            this.OriginI = OI;
            this.OriginT = OT;
            this.DestinationI = DI;
            this.DestinationT = DT;
        }
        public Crane()
        { }

    }
}
