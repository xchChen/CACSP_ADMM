using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenerateInputdata
{
    public partial class ADMM_OPT_Frm : Form
    {
        #region global elements

        int g_number_of_time_intervals = 100;
        List<TruckNode> truck_node_list = new List<TruckNode>();
        int truck_node_num = 0;
        List<CraneNode> crane_node_list = new List<CraneNode>();
        int crane_node_num = 0;
        List<TruckLink> truck_link_list = new List<TruckLink>();
        int truck_link_number = 0;
        List<CraneLink> crane_link_list = new List<CraneLink>();
        int crane_link_number = 0;
        List<Truck> truck_list = new List<Truck>();
        int truck_num = 0;
        List<Crane> crane_list = new List<Crane>();
        int crane_num = 0;
        int GateCap1 = 2;
        int GateCap2 = 2;
        int MAX_Iteration = 300;
        double crane_time_weight = 0.01;
        double MAX_LABEL_COST = 1000000000000;
        double step_size_LR = 1;

        //rolling horizon
        int T_intervel = 10;//time region for store the K best solution
        int k_best_num = 5;
        double prime_cost_weight = 0.5;
        double dual_cost_weight = 0.5;

        //coupling
        double ADMM_coupling_theta_initialization = 10;
        double ADMM_coupling_theta = 10; 
        double ADMM_coupling_theta_beta = 6;
        double ADMM_coupling_theta_gamma = 0.5;
        double ADMM_LR_coupling_multiplier_initialization = 0;
        double pure_LR_coupling_multiplier_initialization = 0;
        //inspection
        double ADMM_multiplier_rho_initialization = 2;
        double ADMM_multiplier_rho = 2;
        double ADMM_truck_update_beta = 2;
        double ADMM_truck_update_gamma = 0.5;
        double ADMM_LR_inspection_multiplier_initialization = 0;
        double pure_LR_inspection_multiplier_initialization = 0;
        //non_crossing
        double ADMM_multiplier_sigma_initialization = 2;
        double ADMM_multiplier_sigma = 2;
        double ADMM_crane_update_beta = 2;
        double ADMM_crane_update_gamma = 0.5;
        double ADMM_LR_non_crossing_multiplier_each_c_initialization = 0;
        double pure_LR_no_crossing_multiplier_c_initialization = 0;

        #endregion

        public ADMM_OPT_Frm()
        {
            InitializeComponent();
        }

        public void read_input_files()
        {
            //read input_truck_node.csv
            DataTable input_truck_node_dt = new DataTable();
            input_truck_node_dt = CSVFileHelper.OpenCSV("input_truck_node.csv");
            foreach (DataRow row in input_truck_node_dt.Rows)
            {
                TruckNode t_node = new TruckNode();
                t_node.id = Convert.ToInt32(row[0]);
                t_node.name_code = Convert.ToInt32(row[1]);
                t_node.description = Convert.ToString(row[2]);
                t_node.cost = Convert.ToDouble(row[3]);
                truck_node_list.Add(t_node);
            }
            truck_node_num = truck_node_list.Count();

            //read input_crane_node.csv
            DataTable input_crane_node_dt = new DataTable();
            input_crane_node_dt = CSVFileHelper.OpenCSV("input_crane_node.csv");
            foreach (DataRow row in input_crane_node_dt.Rows)
            {
                CraneNode c_node = new CraneNode();
                c_node.id = Convert.ToInt32(row[0]);
                c_node.name_code = Convert.ToInt32(row[1]);
                c_node.description = Convert.ToString(row[2]);
                c_node.cost = Convert.ToDouble(row[3]);
                crane_node_list.Add(c_node);
            }
            crane_node_num = crane_node_list.Count();

            //read input_truck_link.csv
            DataTable input_truck_link_dt = new DataTable();
            input_truck_link_dt = CSVFileHelper.OpenCSV("input_truck_link.csv");
            foreach (DataRow row in input_truck_link_dt.Rows)
            {
                TruckLink t_link = new TruckLink();
                t_link.id = Convert.ToInt32(row[0]);
                t_link.name = Convert.ToString(row[1]);
                t_link.type = Convert.ToInt32(row[2]);
                t_link.from_node_id = Convert.ToInt32(row[3]);
                t_link.to_node_id = Convert.ToInt32(row[4]);
                t_link.from_node = truck_node_list[t_link.from_node_id - 1];
                t_link.to_node = truck_node_list[t_link.to_node_id - 1];
                t_link.travle_time = Convert.ToInt32(row[5]);
                t_link.cost = Convert.ToDouble(row[6]);
                t_link.capacity = Convert.ToInt32(row[7]);
                truck_link_list.Add(t_link);
            }
            truck_link_number = truck_link_list.Count();

            //read input_crane_link.csv
            DataTable input_crane_link_dt = new DataTable();
            input_crane_link_dt = CSVFileHelper.OpenCSV("input_crane_link.csv");
            foreach (DataRow row in input_crane_link_dt.Rows)
            {
                CraneLink c_link = new CraneLink();
                c_link.id = Convert.ToInt32(row[0]);
                c_link.name = Convert.ToString(row[1]);
                c_link.type = Convert.ToInt32(row[2]);
                c_link.from_node_id = Convert.ToInt32(row[3]);
                c_link.to_node_id = Convert.ToInt32(row[4]);
                //get the name_code of each crane node by searching the corresponding truck_node
                int from_node_name_code = truck_node_list[c_link.from_node_id - 1].name_code;
                int to_node_name_code = truck_node_list[c_link.to_node_id - 1].name_code;//
                c_link.from_node = Get_crane_node(from_node_name_code, crane_node_list);
                c_link.to_node = Get_crane_node(to_node_name_code, crane_node_list);
                c_link.travle_time = Convert.ToInt32(row[5]);
                c_link.cost = Convert.ToDouble(row[6]);
                c_link.capacity = Convert.ToInt32(row[7]);
                //initailize the moving_arc_conflicting_arc_set
                for (int t = 0; t < g_number_of_time_intervals * 2; t++)//add more cca in case of exceed time limit
                {
                    Crane_conflict_arc cca = new Crane_conflict_arc();
                    c_link.CCA_list.Add(cca);
                }
                crane_link_list.Add(c_link);
            }
            crane_link_number = crane_link_list.Count();

            //read input_truck.csv
            DataTable input_truck_dt = new DataTable();
            input_truck_dt = CSVFileHelper.OpenCSV("input_truck.csv");
            foreach (DataRow row in input_truck_dt.Rows)
            {
                Truck truck = new Truck();
                truck.id = Convert.ToInt32(row[0]);
                truck.Type = Convert.ToInt32(row[1]);
                truck.OriginI = Convert.ToInt32(row[2]);
                truck.DestinationI = Convert.ToInt32(row[3]);
                truck.ContainerBayID = Convert.ToInt32(row[4]);
                truck.ContainerBay_stateID = Convert.ToInt32(row[5]);
                truck.OriginT = Convert.ToInt32(row[6]);
                truck.DestinationT = Convert.ToInt32(row[7]);
                string[] available_node_str_array = row[8].ToString().Split(';');
                for (int i = 0; i < available_node_str_array.Length; i++)
                {
                    int node_id = Convert.ToInt32(available_node_str_array[i]);
                    truck.available_node_list.Add(truck_node_list[node_id - 1]);
                }
                //truck handling_coupling_crane_link
                foreach (CraneLink c_link in crane_link_list)
                {
                    if (c_link.from_node_id == truck.ContainerBayID && c_link.to_node_id == truck.ContainerBay_stateID)
                    {
                        truck.handling_coupling_link = c_link;
                        break;
                    }
                }
                truck_list.Add(truck);
            }
            truck_num = truck_list.Count();

            //read input_crane.csv
            DataTable input_crane_dt = new DataTable();
            input_crane_dt = CSVFileHelper.OpenCSV("input_crane.csv");
            foreach (DataRow row in input_crane_dt.Rows)
            {
                Crane crane = new Crane();
                crane.id = Convert.ToInt32(row[0]);
                crane.OriginI = Convert.ToInt32(row[1]);
                crane.DestinationI = Convert.ToInt32(row[2]);
                crane.OriginT = Convert.ToInt32(row[3]);
                crane.DestinationT = Convert.ToInt32(row[4]);
                string[] available_node_str_array = row[5].ToString().Split(';');
                for (int i = 0; i < available_node_str_array.Length; i++)
                {
                    int node_id = Convert.ToInt32(available_node_str_array[i]);
                    int name_code = truck_node_list[node_id - 1].name_code;
                    crane.available_node_list.Add(Get_crane_node(name_code, crane_node_list));
                }

                crane_list.Add(crane);
            }
            crane_num = crane_list.Count();
        }

        public void in_out_going_links_for_node()
        {
            foreach (TruckLink t_link in truck_link_list)
            {
                t_link.from_node.Outgoing_LinkList.Add(t_link);
                t_link.from_node.Outgoing_NodeList.Add(t_link.to_node);
                t_link.to_node.Ingoing_LinkList.Add(t_link);
                t_link.to_node.Ingoing_NodeList.Add(t_link.from_node);
            }
            foreach (CraneLink c_link in crane_link_list)
            {
                c_link.from_node.Outgoing_LinkList.Add(c_link);
                c_link.from_node.Outgoing_NodeList.Add(c_link.to_node);
                c_link.to_node.Ingoing_LinkList.Add(c_link);
                c_link.to_node.Ingoing_NodeList.Add(c_link.from_node);
            }
        }

        public void dynamic_programing_for_trucks()
        {
            foreach (Truck truck in truck_list)
            {
                #region intialization

                truck.arclist_ADMM_path.Clear();
                int truck_availabel_node_number = truck.available_node_list.Count();
                truck.lower_bound_travel_time = 0;
                //set pre_node_id[,] pre_time_interval[,] and label_cost[,] 
                int[,] pre_node_id_array = new int[truck_availabel_node_number, g_number_of_time_intervals];
                int[,] pre_time_interval_array = new int[truck_availabel_node_number, g_number_of_time_intervals];
                double[,] label_cost_array = new double[truck_availabel_node_number, g_number_of_time_intervals];
                for (int l = 0; l < truck_availabel_node_number; l++)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        pre_node_id_array[l, t] = -1;
                        pre_time_interval_array[l, t] = -1;
                        label_cost_array[l, t] = MAX_LABEL_COST;
                    }
                }
                //orgin initialization
                pre_node_id_array[truck.available_node_list[0].id - 1, truck.OriginT - 1] = 0;
                pre_time_interval_array[truck.available_node_list[0].id - 1, truck.OriginT - 1] = 0;
                label_cost_array[truck.available_node_list[0].id - 1, truck.OriginT - 1] = 0;

                #endregion

                #region dynamic programming begin

                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    for (int n = 0; n < truck.available_node_list.Count; n++)
                    {
                        if (pre_node_id_array[n, t] != -1)
                        {

                            TruckNode this_from_node = truck.available_node_list[n];
                            double cost_label = 0;
                            int travel_time_label = 0;

                            //1-label the node in next time intervel
                            if (this_from_node.name_code == 10 || this_from_node.name_code == 100 || this_from_node.name_code == 20 || this_from_node.name_code == 1000)
                            {
                                cost_label = this_from_node.cost;
                                travel_time_label = 1;
                                //compare and label
                                if ((t + travel_time_label < g_number_of_time_intervals))
                                {
                                    if (label_cost_array[n, t] + cost_label < label_cost_array[n, t + travel_time_label])
                                    {
                                        label_cost_array[n, t + travel_time_label] = label_cost_array[n, t] + cost_label;
                                        pre_node_id_array[n, t + travel_time_label] = this_from_node.id;
                                        pre_time_interval_array[n, t + travel_time_label] = t + 1;
                                    }
                                }
                            }

                            //2-label the outgoing node
                            foreach (TruckLink t_link in this_from_node.Outgoing_LinkList)
                            {
                                if (truck.available_node_list.Contains(t_link.to_node))
                                {
                                    TruckNode this_to_node = t_link.to_node;
                                    int this_to_node_index = 0;
                                    for (int i = 0; i < truck.available_node_list.Count; i++)
                                    {
                                        if (this_to_node.id == truck.available_node_list[i].id)
                                        {
                                            this_to_node_index = i;
                                            break;
                                        }
                                    }
                                    if (t_link.type == 1)
                                    {
                                        cost_label = t_link.cost;
                                        travel_time_label = t_link.travle_time;
                                    }
                                    if (t_link.type == 2)
                                    {
                                        int t_max = Math.Min(g_number_of_time_intervals, (t + 1 + (t_link.travle_time - 1)));
                                        cost_label = t_link.cost;
                                        double ADMM_cost = 0;
                                        for (int s = t + 1; s <= t_max; s++)
                                        {
                                            int nv = 0;//num of other truck occupy the arc set
                                            int t_min = Math.Max(1, s - (t_link.travle_time - 1));
                                            //calculate nv
                                            for (int j = t_min; j <= s; j++)
                                            {
                                                Arc transaction_arc = new Arc(t_link.from_node.id, t_link.to_node.id, j, j + t_link.travle_time);
                                                foreach (Truck tk in truck_list)
                                                {
                                                    if (tk.OriginT > transaction_arc.T)
                                                    {
                                                        break;
                                                    }
                                                    if (tk.OriginT <= transaction_arc.T)
                                                    {
                                                        if (tk.id != truck.id)
                                                        {
                                                            foreach (Arc t_arc in tk.arclist_ADMM_path)
                                                            {
                                                                if (t_arc.T < transaction_arc.T)
                                                                {
                                                                    break;
                                                                }
                                                                if (t_arc.T == transaction_arc.T)
                                                                {
                                                                    if (t_arc.I == transaction_arc.I && t_arc.J == transaction_arc.J)
                                                                    {
                                                                        nv++;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            int gatecap = 0;
                                            if (t_link.from_node.name_code == 10)
                                            {
                                                gatecap = GateCap1;
                                            }
                                            if (t_link.from_node.name_code == 20)
                                            {
                                                gatecap = GateCap2;
                                            }
                                            double sub_gradient_admm = (2 * nv - 2 * gatecap + 1);
                                            sub_gradient_admm = Math.Max(0, sub_gradient_admm);
                                            double admm_cost_this = t_link.time_dependent_ADMM_LR_transaction_multiplier[s - 1] + ADMM_multiplier_rho * 0.5 * sub_gradient_admm;
                                            ADMM_cost += admm_cost_this;
                                        }
                                        cost_label += ADMM_cost;
                                        travel_time_label = t_link.travle_time;
                                    }
                                    if (t_link.type == 3)
                                    {
                                        Arc coupling_arc = new Arc(t_link.from_node_id, t_link.to_node_id, t + 1, (t + 1 + t_link.travle_time));
                                        int sum_truck_use_link_at_t = 0;
                                        foreach (Truck tk in truck_list)
                                        {
                                            if (tk.OriginT >= coupling_arc.T)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                if (tk.id != truck.id)
                                                {
                                                    foreach (Arc t_arc in tk.arclist_ADMM_path)
                                                    {
                                                        if (t_arc.T < coupling_arc.T)
                                                        {
                                                            break;
                                                        }
                                                        if (t_arc.T == coupling_arc.T)
                                                        {
                                                            if (t_arc.I == coupling_arc.I && t_arc.J == coupling_arc.J)
                                                            {
                                                                sum_truck_use_link_at_t++;
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                        int sum_crane_use_link_at_t = 0;
                                        foreach (Crane crane in crane_list)
                                        {
                                            foreach (Arc c_arc in crane.arc_list_ADMM_path)
                                            {
                                                if (c_arc.T < coupling_arc.T)
                                                {
                                                    break;
                                                }
                                                if (c_arc.T == coupling_arc.T)
                                                {
                                                    if (c_arc.I == coupling_arc.I && c_arc.J == coupling_arc.J)
                                                    {
                                                        sum_crane_use_link_at_t++;
                                                    }
                                                }
                                            }
                                        }
                                        double sub_gradient = (sum_truck_use_link_at_t - sum_crane_use_link_at_t + 0.5);
                                        double cost_admm = t_link.time_dependent_ADMM_LR_coupling_multiplier[t] + ADMM_coupling_theta * sub_gradient;
                                        cost_label = t_link.cost + cost_admm;
                                        travel_time_label = t_link.travle_time;
                                    }
                                    //compare and label
                                    if (t + travel_time_label < g_number_of_time_intervals)
                                    {
                                        if (label_cost_array[n, t] + cost_label < label_cost_array[this_to_node_index, t + travel_time_label])
                                        {
                                            label_cost_array[this_to_node_index, t + travel_time_label] = label_cost_array[n, t] + cost_label;
                                            pre_node_id_array[this_to_node_index, t + travel_time_label] = this_from_node.id;
                                            pre_time_interval_array[this_to_node_index, t + travel_time_label] = t + 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region back trace

                int arc_to_node_id = truck.DestinationI;
                int arc_to_time = truck.DestinationT;
                do
                {
                    int to_node_index = 0;
                    for (int i = 0; i < truck.available_node_list.Count; i++)
                    {
                        if (truck.available_node_list[i].id == arc_to_node_id)
                        {
                            to_node_index = i;
                            break;
                        }
                    }
                    int lowerbound_arc_from_node_id = pre_node_id_array[to_node_index, arc_to_time - 1];
                    int lowerbound_arc_from_time = pre_time_interval_array[to_node_index, arc_to_time - 1];
                    Arc lowerbound_arc = new Arc(lowerbound_arc_from_node_id, arc_to_node_id, lowerbound_arc_from_time, arc_to_time);
                    truck.arclist_ADMM_path.Add(lowerbound_arc);
                    arc_to_node_id = lowerbound_arc_from_node_id;
                    arc_to_time = lowerbound_arc_from_time;
                }
                while (arc_to_node_id != 0);

                #endregion
            }
        }

        public void dynamic_programing_for_cranes()
        {
            foreach (Crane crane in crane_list)
            {
                #region intialization

                crane.arc_list_ADMM_path.Clear();
                int crane_availabel_node_number = crane.available_node_list.Count();
                crane.lower_bound_travel_time = 0;
                //set pre_node_id[,] pre_time_interval[,] and label_cost[,] 
                int[,] pre_node_id_array = new int[crane_availabel_node_number, g_number_of_time_intervals];
                int[,] pre_time_interval_array = new int[crane_availabel_node_number, g_number_of_time_intervals];
                double[,] label_cost_array = new double[crane_availabel_node_number, g_number_of_time_intervals];
                for (int n = 0; n < crane_availabel_node_number; n++)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        pre_node_id_array[n, t] = -1;
                        pre_time_interval_array[n, t] = -1;
                        label_cost_array[n, t] = MAX_LABEL_COST;
                    }
                }
                //orgin initialization
                pre_node_id_array[Get_index_in_crane_availabel_node_list(crane.OriginI, crane), crane.OriginT - 1] = 0;
                pre_time_interval_array[Get_index_in_crane_availabel_node_list(crane.OriginI, crane), crane.OriginT - 1] = 0;
                label_cost_array[Get_index_in_crane_availabel_node_list(crane.OriginI, crane), crane.OriginT - 1] = 0;

                #endregion

                #region dynamic programming begin

                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    for (int n = 0; n < crane.available_node_list.Count; n++)
                    {
                        if (pre_node_id_array[n, t] != -1)
                        {
                            CraneNode this_from_node = crane.available_node_list[n];
                            double cost_label = 0;
                            int travel_time_label = 0;
                            //create the arc set φ(i,j,t,s) and mc
                            List<Arc> imcompatible_crane_arc_list = new List<Arc>();
                            int mc = 0;
                            double sub_gradinent_without_c;
                            int physical_crane_name_code = this_from_node.name_code / 10 * 10;
                            CraneNode physical_crane_node = Get_crane_node(physical_crane_name_code, crane_node_list);
                            int moving_link_travel_time = 0;
                            foreach (Link ll in crane_link_list)
                            {
                                if (ll.type == 1)
                                {
                                    moving_link_travel_time = ll.travle_time;
                                    break;
                                }
                            }

                            //1-label the node in next time intervel
                            travel_time_label = 1;
                            if (this_from_node.description == "crane node0")
                            {
                                imcompatible_crane_arc_list = this_from_node.incompatible_arc_list_t[t];
                                //calculate mc
                                foreach (Crane c in crane_list)
                                {
                                    if (c.id != crane.id)
                                    {
                                        foreach (Arc arc in imcompatible_crane_arc_list)
                                        {
                                            foreach (Arc c_arc in c.arc_list_ADMM_path)
                                            {
                                                if (c_arc.T < arc.T)
                                                {
                                                    break;
                                                }
                                                if (c_arc.T == arc.T)
                                                {
                                                    if (c_arc.I == arc.I && c_arc.J == arc.J)
                                                    {
                                                        mc++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                sub_gradinent_without_c = 2 * mc - 1;
                                sub_gradinent_without_c = Math.Max(0, sub_gradinent_without_c);
                                double admm_cost = crane_time_weight * this_from_node.cost + this_from_node.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[crane.id - 1][t] + ADMM_multiplier_sigma * 0.5 * sub_gradinent_without_c;
                                admm_cost = Math.Max(0, admm_cost);
                                cost_label = admm_cost;
                                //compare and label
                                if ((t + travel_time_label < g_number_of_time_intervals))
                                {
                                    if (label_cost_array[n, t] + cost_label < label_cost_array[n, t + travel_time_label])
                                    {
                                        label_cost_array[n, t + travel_time_label] = label_cost_array[n, t] + cost_label;
                                        pre_node_id_array[n, t + travel_time_label] = this_from_node.id;
                                        pre_time_interval_array[n, t + travel_time_label] = t + 1;
                                    }
                                }
                            }

                            //2-label the outgoing node
                            foreach (CraneLink c_link in this_from_node.Outgoing_LinkList)
                            {
                                if (crane.available_node_list.Contains(c_link.to_node))
                                {
                                    travel_time_label = c_link.travle_time;
                                    CraneNode this_to_node = c_link.to_node;
                                    int this_to_node_index = 0;
                                    for (int i = 0; i < crane.available_node_list.Count; i++)
                                    {
                                        if (this_to_node.id == crane.available_node_list[i].id)
                                        {
                                            this_to_node_index = i;
                                            break;
                                        }
                                    }

                                    imcompatible_crane_arc_list = c_link.incompatible_arc_list_t[t];
                                    //calculate mc
                                    mc = 0;
                                    foreach (Crane c in crane_list)
                                    {
                                        if (c.id != crane.id)
                                        {
                                            foreach (Arc arc in imcompatible_crane_arc_list)
                                            {
                                                foreach (Arc c_arc in c.arc_list_ADMM_path)
                                                {
                                                    if (c_arc.T < arc.T)
                                                    {
                                                        break;
                                                    }
                                                    if (c_arc.T == arc.T)
                                                    {
                                                        if (c_arc.I == arc.I && c_arc.J == arc.J)
                                                        {
                                                            mc++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    sub_gradinent_without_c = 2 * mc - 1;
                                    sub_gradinent_without_c = Math.Max(0, sub_gradinent_without_c);

                                    if (c_link.type == 1)
                                    {
                                        double admm_cost = c_link.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[crane.id - 1][t] + ADMM_multiplier_sigma * 0.5 * sub_gradinent_without_c;

                                        //Distance cost
                                        int bay_id = this_to_node.name_code / 10 - 10;
                                        int section_id = 0;
                                        int section_dif = 0;
                                        //average distributed cranes
                                        int each_crane_bay_num = crane_node_num / 3 / crane_num;
                                        for (int i = 1; i <= crane_num; i++)
                                        {
                                            if (bay_id <= i * each_crane_bay_num)
                                            {
                                                section_id = i;
                                                break;
                                            }
                                        }
                                        section_dif = Math.Abs(crane.id - section_id);
                                        double dis_cost = ADMM_multiplier_sigma / 5 * section_dif;

                                        cost_label = crane_time_weight * c_link.cost + admm_cost + dis_cost;
                                    }
                                    if (c_link.type == 3)
                                    {
                                        double admm_cost = c_link.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[crane.id - 1][t] + ADMM_multiplier_sigma * 0.5 * sub_gradinent_without_c;
                                        cost_label = crane_time_weight * c_link.cost + admm_cost;
                                    }
                                    if (c_link.type == 2)
                                    {
                                        TruckLink t_link_coupling = Get_truck_link(c_link.from_node.id, c_link.to_node.id, truck_link_list);
                                        c_link.time_denpendent_ADMM_LR_coupling_multiplier[t] = t_link_coupling.time_dependent_ADMM_LR_coupling_multiplier[t];
                                        Arc coupling_arc = new Arc(c_link.from_node_id, c_link.to_node_id, t + 1, (t + 1 + c_link.travle_time));
                                        int sum_truck_use_link_at_t = 0;
                                        foreach (Truck tk in truck_list)
                                        {
                                            if (tk.OriginT >= coupling_arc.T)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                foreach (Arc t_arc in tk.arclist_ADMM_path)
                                                {
                                                    if (t_arc.T < coupling_arc.T)
                                                    {
                                                        break;
                                                    }
                                                    if (t_arc.T == coupling_arc.T)
                                                    {
                                                        if (t_arc.I == coupling_arc.I && t_arc.J == coupling_arc.J)
                                                        {
                                                            sum_truck_use_link_at_t++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        int sum_crane_use_link_at_t = 0;
                                        foreach (Crane c in crane_list)
                                        {
                                            if (c.id != crane.id)
                                            {
                                                foreach (Arc c_arc in c.arc_list_ADMM_path)
                                                {
                                                    if (c_arc.T < coupling_arc.T)
                                                    {
                                                        break;
                                                    }
                                                    if (c_arc.T == coupling_arc.T)
                                                    {
                                                        if (c_arc.I == coupling_arc.I && c_arc.J == coupling_arc.J)
                                                        {
                                                            sum_crane_use_link_at_t++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        double sub_gradient_coupling = (sum_crane_use_link_at_t - sum_truck_use_link_at_t + 0.5);
                                        double admm_cost_coupling = -c_link.time_denpendent_ADMM_LR_coupling_multiplier[t] + ADMM_coupling_theta * sub_gradient_coupling;

                                        double admm_cost_non_crossing = c_link.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[crane.id - 1][t] + ADMM_multiplier_sigma * 0.5 * sub_gradinent_without_c;
                                        double admm_cost = admm_cost_coupling + admm_cost_non_crossing;

                                        //Distance cost
                                        int bay_id = this_from_node.name_code / 10 - 10;
                                        int section_id = 0;
                                        int section_dif = 0;
                                        int each_crane_bay_num = crane_node_num / 3 / crane_num;
                                        for (int i = 1; i <= crane_num; i++)
                                        {
                                            if (bay_id <= i * each_crane_bay_num)
                                            {
                                                section_id = i;
                                                break;
                                            }
                                        }
                                        section_dif = Math.Abs(crane.id - section_id);
                                        double dis_cost = ADMM_coupling_theta / 5 * section_dif;

                                        cost_label = crane_time_weight * c_link.cost + admm_cost + dis_cost;
                                    }
                                    //compare and label
                                    if (t + travel_time_label < g_number_of_time_intervals)
                                    {
                                        if (label_cost_array[n, t] + cost_label < label_cost_array[this_to_node_index, t + travel_time_label])
                                        {
                                            label_cost_array[this_to_node_index, t + travel_time_label] = label_cost_array[n, t] + cost_label;
                                            pre_node_id_array[this_to_node_index, t + travel_time_label] = this_from_node.id;
                                            pre_time_interval_array[this_to_node_index, t + travel_time_label] = t + 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region bace trace

                int arc_to_node_id = crane.DestinationI;
                int arc_to_time = crane.DestinationT;
                do
                {
                    int to_node_index = Get_index_in_crane_availabel_node_list(arc_to_node_id, crane);
                    int lowerbound_arc_from_node_id = pre_node_id_array[to_node_index, arc_to_time - 1];
                    int lowerbound_arc_from_time = pre_time_interval_array[to_node_index, arc_to_time - 1];
                    Arc lowerbound_arc = new Arc(lowerbound_arc_from_node_id, arc_to_node_id, lowerbound_arc_from_time, arc_to_time);
                    crane.arc_list_ADMM_path.Add(lowerbound_arc);
                    arc_to_node_id = lowerbound_arc_from_node_id;
                    arc_to_time = lowerbound_arc_from_time;
                }
                while (arc_to_node_id != 0);

                #endregion
            }
        }

        public void update_ADMM_coupling_multiplier_for_trucks_and_cranes(int iteration_k)
        {
            foreach (TruckLink t_link in truck_link_list)
            {
                if (t_link.type == 3)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        Arc coupling_arc = new Arc(t_link.from_node_id, t_link.to_node_id, t + 1, (t + 1 + t_link.travle_time));
                        int from_name_code = t_link.from_node.name_code;
                        int to_name_code = t_link.to_node.name_code;
                        int sum_truck_use_link_at_t = 0;
                        foreach (Truck truck in truck_list)
                        {
                            if (truck.OriginT >= coupling_arc.T)
                            {
                                break;
                            }
                            else
                            {
                                foreach (Arc t_arc in truck.arclist_ADMM_path)
                                {
                                    if (t_arc.T < coupling_arc.T)
                                    {
                                        break;
                                    }
                                    if (t_arc.T == coupling_arc.T)
                                    {
                                        if (t_arc.I == coupling_arc.I && t_arc.J == coupling_arc.J)
                                        {
                                            sum_truck_use_link_at_t++;
                                        }
                                    }
                                }
                            }
                        }

                        int sum_crane_use_link_at_t = 0;
                        foreach (Crane crane in crane_list)
                        {
                            foreach (Arc c_arc in crane.arc_list_ADMM_path)
                            {
                                if (c_arc.T < coupling_arc.T)
                                {
                                    break;
                                }
                                if (c_arc.T == coupling_arc.T)
                                {
                                    if (c_arc.I == coupling_arc.I && c_arc.J == coupling_arc.J)
                                    {
                                        sum_crane_use_link_at_t++;
                                    }
                                }
                            }
                        }

                        int sub_grandient = sum_truck_use_link_at_t - sum_crane_use_link_at_t;
                        t_link.time_dependent_ADMM_coupling_sub_grandient[t] = sub_grandient;
                        t_link.time_dependent_ADMM_LR_coupling_multiplier[t] = t_link.time_dependent_ADMM_LR_coupling_multiplier[t] + ADMM_coupling_theta * (sub_grandient);
                    }
                }
            }
            if (iteration_k == 1)
            {
                foreach (TruckLink t_link in truck_link_list)
                {
                    if (t_link.type == 3)
                    {
                        for (int t = 0; t < g_number_of_time_intervals; t++)
                        {
                            t_link.time_dependent_ADMM_coupling_last_sub_grandient[t] = t_link.time_dependent_ADMM_coupling_sub_grandient[t];
                        }
                    }
                }
            }
            if (iteration_k > 1)
            {
                double sum_sub_grandient_quadratic_k = 0;
                double sum_sub_grandient_quadratic_last_k = 0;
                foreach (TruckLink t_link in truck_link_list)
                {
                    if (t_link.type == 3)
                    {
                        for (int t = 0; t < g_number_of_time_intervals; t++)
                        {
                            sum_sub_grandient_quadratic_k += Math.Pow(t_link.time_dependent_ADMM_coupling_sub_grandient[t], 2);
                            sum_sub_grandient_quadratic_last_k += Math.Pow(t_link.time_dependent_ADMM_coupling_last_sub_grandient[t], 2);
                        }
                    }
                }
                if (sum_sub_grandient_quadratic_k == 0)
                {
                    //ADMM_coupling_theta = ADMM_coupling_theta_initialization;
                    ADMM_coupling_theta -= ADMM_coupling_theta_beta;
                }
                if (sum_sub_grandient_quadratic_k > sum_sub_grandient_quadratic_last_k * ADMM_coupling_theta_gamma && (iteration_k) > MAX_Iteration / 3)
                {
                    ADMM_coupling_theta += ADMM_coupling_theta_beta;
                }
                foreach (TruckLink t_link in truck_link_list)
                {
                    if (t_link.type == 3)
                    {
                        for (int t = 0; t < g_number_of_time_intervals; t++)
                        {
                            t_link.time_dependent_ADMM_coupling_last_sub_grandient[t] = t_link.time_dependent_ADMM_coupling_sub_grandient[t];
                        }
                    }
                }
            }
        }

        public void update_ADMM_multipliers_for_trucks(int iteration_k)
        {
            foreach (TruckLink t_link in truck_link_list)
            {
                if (t_link.type == 2)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        int sum_truck_use_link_set_at_t = 0;
                        List<Arc> gate_transaction_compatible_arc_list = new List<Arc>();
                        for (int tt = t + 1 - (t_link.travle_time - 1); tt <= t + 1; tt++)
                        {
                            Arc gate_transaction_arc = new Arc(t_link.from_node_id, t_link.to_node_id, tt, tt + t_link.travle_time);
                            foreach (Truck truck in truck_list)
                            {
                                if (truck.OriginT > gate_transaction_arc.T)
                                {
                                    break;
                                }
                                else
                                {
                                    foreach (Arc t_arc in truck.arclist_ADMM_path)
                                    {
                                        if (t_arc.T < gate_transaction_arc.T)
                                        {
                                            break;
                                        }
                                        if (t_arc.T == gate_transaction_arc.T)
                                        {
                                            if (t_arc.I == gate_transaction_arc.I && t_arc.J == gate_transaction_arc.J)
                                            {
                                                sum_truck_use_link_set_at_t++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        int cap = 0;
                        if (t_link.from_node.name_code == 10)
                        {
                            cap = GateCap1;
                        }
                        if (t_link.from_node.name_code == 20)
                        {
                            cap = GateCap2;
                        }
                        int sub_grandient = sum_truck_use_link_set_at_t - cap;

                        t_link.time_dependent_ADMM_transaction_sub_grandient[t] = sub_grandient;
                        t_link.time_dependent_ADMM_LR_transaction_multiplier[t] = Math.Max(0, t_link.time_dependent_ADMM_LR_transaction_multiplier[t] + ADMM_multiplier_rho * sub_grandient);
                    }
                }
            }
            if (iteration_k == 1)
            {
                foreach (TruckLink t_link in truck_link_list)
                {
                    if (t_link.type == 2)
                    {
                        for (int t = 0; t < g_number_of_time_intervals; t++)
                        {
                            t_link.time_dependent_ADMM_transaction_last_sub_grandient[t] = t_link.time_dependent_ADMM_transaction_sub_grandient[t];
                        }
                    }
                }
            }
            if (iteration_k > 1)
            {
                double sum_sub_grandient_quadratic_k = 0;
                double sum_sub_grandient_quadratic_last_k = 0;
                foreach (TruckLink t_link in truck_link_list)
                {
                    if (t_link.type == 2)
                    {
                        for (int t = 0; t < g_number_of_time_intervals; t++)
                        {
                            sum_sub_grandient_quadratic_k += Math.Pow(t_link.time_dependent_ADMM_transaction_sub_grandient[t], 2);
                            sum_sub_grandient_quadratic_last_k += Math.Pow(t_link.time_dependent_ADMM_transaction_last_sub_grandient[t], 2);
                        }
                    }
                }
                if (sum_sub_grandient_quadratic_k == 0)
                {
                    ADMM_multiplier_rho = ADMM_multiplier_rho_initialization;
                }
                if (sum_sub_grandient_quadratic_k > sum_sub_grandient_quadratic_last_k * ADMM_truck_update_gamma && (iteration_k > MAX_Iteration / 3))
                {
                    ADMM_multiplier_rho += ADMM_truck_update_beta;
                }
                foreach (TruckLink t_link in truck_link_list)
                {
                    if (t_link.type == 2)
                    {
                        for (int t = 0; t < g_number_of_time_intervals; t++)
                        {
                            t_link.time_dependent_ADMM_transaction_last_sub_grandient[t] = t_link.time_dependent_ADMM_transaction_sub_grandient[t];
                        }
                    }
                }
            }
        }

        public void update_ADMM_multipliers_for_cranes(int iteration_k)
        {
            foreach (CraneLink c_link in crane_link_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    Arc crane_arc = new Arc(c_link.from_node_id, c_link.to_node_id, t + 1, (t + 1 + c_link.travle_time));
                    List<Arc> incompatible_crane_arc_list = c_link.incompatible_arc_list_t[t];
                    for (int c = 0; c < crane_list.Count; c++)
                    {
                        int y_ijts_c = 0;
                        foreach (Arc c_arc in crane_list[c].arc_list_ADMM_path)
                        {
                            if (c_arc.T < crane_arc.T)
                            {
                                break;
                            }
                            if (c_arc.T == crane_arc.T)
                            {
                                if (c_arc.I == crane_arc.I && c_arc.J == crane_arc.J)
                                {
                                    y_ijts_c += 1;
                                }
                            }
                        }
                        int sum_crane_without_c_use_arc = 0;
                        foreach (Arc incompatible_arc in incompatible_crane_arc_list)
                        {
                            foreach (Crane crane in crane_list)
                            {
                                if (crane.id != crane_list[c].id)
                                {
                                    foreach (Arc c_arc in crane.arc_list_ADMM_path)
                                    {
                                        if (c_arc.T < incompatible_arc.T)
                                        {
                                            break;
                                        }
                                        if (c_arc.T == incompatible_arc.T)
                                        {
                                            if (c_arc.I == incompatible_arc.I && c_arc.J == incompatible_arc.J)
                                            {
                                                sum_crane_without_c_use_arc++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        int sub_grandient_c = 0;
                        sub_grandient_c = y_ijts_c + sum_crane_without_c_use_arc - 1;
                        c_link.time_dependent_ADMM_sub_grandient[c][t] = sub_grandient_c;
                        c_link.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[c][t] = Math.Max(0, c_link.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[c][t] + ADMM_multiplier_sigma * sub_grandient_c);
                    }
                }
            }

            foreach (CraneNode c_node in crane_node_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    Arc crane_arc = new Arc(c_node.id, c_node.id, t + 1, t + 2);
                    List<Arc> incompatible_crane_arc_list = c_node.incompatible_arc_list_t[t];
                    for (int c = 0; c < crane_list.Count; c++)
                    {
                        int y_ijts_c = 0;
                        foreach (Arc c_arc in crane_list[c].arc_list_ADMM_path)
                        {
                            if (c_arc.T < crane_arc.T)
                            {
                                break;
                            }
                            if (c_arc.T == crane_arc.T)
                            {
                                if (c_arc.I == crane_arc.I && c_arc.J == crane_arc.J)
                                {
                                    y_ijts_c += 1;
                                }
                            }
                        }

                        //calculate mc
                        int sum_crane_without_c_use_arc = 0;
                        foreach (Arc incompatible_arc in incompatible_crane_arc_list)
                        {
                            foreach (Crane crane in crane_list)
                            {
                                if (crane.id != crane_list[c].id)
                                {
                                    foreach (Arc c_arc in crane.arc_list_ADMM_path)
                                    {
                                        if (c_arc.T < incompatible_arc.T)
                                        {
                                            break;
                                        }
                                        if (c_arc.T == incompatible_arc.T)
                                        {
                                            if (c_arc.I == incompatible_arc.I && c_arc.J == incompatible_arc.J)
                                            {
                                                sum_crane_without_c_use_arc++;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //calculate sub_grandient_c
                        int sub_grandient_c = 0;
                        //if (y_ijts_c >= 1)
                        //{
                        //    sub_grandient_c = y_ijts_c + sum_crane_without_c_use_arc - 1;
                        //}
                        //else
                        //{
                        //    sub_grandient_c = 0;
                        //}
                        sub_grandient_c = y_ijts_c + sum_crane_without_c_use_arc - 1;

                        c_node.time_dependent_ADMM_sub_grandient[c][t] = sub_grandient_c;
                        //update time_dependent_ADMM_multiplier_miu_c
                        c_node.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[c][t] = Math.Max(0, c_node.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[c][t] + ADMM_multiplier_sigma * sub_grandient_c);
                    }
                }
            }

            if (iteration_k == 1)
            {
                foreach (CraneLink c_link in crane_link_list)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        for (int c = 0; c < crane_list.Count; c++)
                        {
                            c_link.time_dependent_ADMM_last_sub_grandient[c][t] = c_link.time_dependent_ADMM_sub_grandient[c][t];
                        }
                    }
                }
                foreach (CraneNode c_node in crane_node_list)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        for (int c = 0; c < crane_list.Count; c++)
                        {
                            c_node.time_dependent_ADMM_last_sub_grandient[c][t] = c_node.time_dependent_ADMM_sub_grandient[c][t];
                        }
                    }
                }
            }
            if (iteration_k > 1)
            {
                double sum_sub_grandient_quadratic_k = 0;
                double sum_sub_grandient_quadratic_last_k = 0;
                foreach (CraneLink c_link in crane_link_list)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        for (int c = 0; c < crane_list.Count; c++)
                        {
                            sum_sub_grandient_quadratic_k += Math.Pow(c_link.time_dependent_ADMM_sub_grandient[c][t], 2);
                            sum_sub_grandient_quadratic_last_k += Math.Pow(c_link.time_dependent_ADMM_last_sub_grandient[c][t], 2);
                        }
                    }
                }
                foreach (CraneNode c_node in crane_node_list)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        for (int c = 0; c < crane_list.Count; c++)
                        {
                            sum_sub_grandient_quadratic_k += Math.Pow(c_node.time_dependent_ADMM_sub_grandient[c][t], 2);
                            sum_sub_grandient_quadratic_last_k += Math.Pow(c_node.time_dependent_ADMM_last_sub_grandient[c][t], 2);
                        }
                    }
                }
                if (sum_sub_grandient_quadratic_k == 0)
                {
                    ADMM_multiplier_sigma = ADMM_multiplier_sigma_initialization;
                }
                if (sum_sub_grandient_quadratic_k > sum_sub_grandient_quadratic_last_k * ADMM_crane_update_gamma && (iteration_k > MAX_Iteration / 3))
                {
                    ADMM_multiplier_sigma += ADMM_crane_update_beta;
                }
                foreach (CraneLink c_link in crane_link_list)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        for (int c = 0; c < crane_list.Count; c++)
                        {
                            c_link.time_dependent_ADMM_last_sub_grandient[c][t] = c_link.time_dependent_ADMM_sub_grandient[c][t];
                        }
                    }

                }
                foreach (CraneNode c_node in crane_node_list)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        for (int c = 0; c < crane_list.Count; c++)
                        {
                            c_node.time_dependent_ADMM_last_sub_grandient[c][t] = c_node.time_dependent_ADMM_sub_grandient[c][t];
                        }
                    }
                }
            }
        }

        public double calculate_uper_bound()
        {
            double total_cost = 0;
            foreach (Truck truck in truck_list)
            {
                foreach (Arc arc in truck.arclist_ADMM_path)
                {
                    if (arc.I != 0)
                    {
                        if (arc.I == arc.J)
                        {
                            if (truck_node_list[arc.I - 1].cost != 0)
                            {
                                total_cost += 1;
                            }
                        }
                        else
                        {
                            TruckLink t_link = Get_truck_link(arc.I, arc.J, truck_link_list);
                            total_cost += t_link.travle_time;
                        }
                    }
                }
            }
            return total_cost;
        }

        public double calculate_lower_bound_using_pure_lagrangian()
        {
            double lower_bound_cost = 0;
            //dynamic programming of trucks using pure lagrangian
            foreach (Truck truck in truck_list)
            {
                #region iniliazation

                truck.arclist_LR_path.Clear();
                int truck_availabel_node_number = truck.available_node_list.Count();
                truck.lower_bound_travel_time = 0;
                //set pre_node_id[,] pre_time_interval[,] and label_cost[,] 
                int[,] pre_node_id_array = new int[truck_availabel_node_number, g_number_of_time_intervals];
                int[,] pre_time_interval_array = new int[truck_availabel_node_number, g_number_of_time_intervals];
                double[,] label_cost_array = new double[truck_availabel_node_number, g_number_of_time_intervals];
                for (int l = 0; l < truck_availabel_node_number; l++)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        pre_node_id_array[l, t] = -1;
                        pre_time_interval_array[l, t] = -1;
                        label_cost_array[l, t] = MAX_LABEL_COST;
                    }
                }
                //orgin initialization
                pre_node_id_array[truck.available_node_list[0].id - 1, truck.OriginT - 1] = 0;
                pre_time_interval_array[truck.available_node_list[0].id - 1, truck.OriginT - 1] = 0;
                label_cost_array[truck.available_node_list[0].id - 1, truck.OriginT - 1] = 0;

                #endregion

                #region dynamic programming begin

                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    for (int n = 0; n < truck.available_node_list.Count; n++)
                    {
                        if (pre_node_id_array[n, t] != -1)
                        {
                            TruckNode this_from_node = truck.available_node_list[n];
                            double cost_label = 0;
                            int travel_time_label = 0;

                            //1-label the node in next time intervel
                            if (this_from_node.name_code == 10 || this_from_node.name_code == 100 || this_from_node.name_code == 20 || this_from_node.name_code == 1000)
                            {
                                cost_label = this_from_node.cost;
                                travel_time_label = 1;
                                if ((t + travel_time_label < g_number_of_time_intervals))
                                {
                                    if (label_cost_array[n, t] + cost_label < label_cost_array[n, t + travel_time_label])
                                    {
                                        label_cost_array[n, t + travel_time_label] = label_cost_array[n, t] + cost_label;
                                        pre_node_id_array[n, t + travel_time_label] = this_from_node.id;
                                        pre_time_interval_array[n, t + travel_time_label] = t + 1;
                                    }
                                }
                            }

                            //2-label the outgoing node
                            foreach (TruckLink t_link in this_from_node.Outgoing_LinkList)
                            {
                                if (truck.available_node_list.Contains(t_link.to_node))//juge if this t_link exist in truck.outgoing_link_list
                                {
                                    TruckNode this_to_node = t_link.to_node;
                                    int this_to_node_index = 0;
                                    for (int i = 0; i < truck.available_node_list.Count; i++)
                                    {
                                        if (this_to_node.id == truck.available_node_list[i].id)
                                        {
                                            this_to_node_index = i;
                                            break;
                                        }
                                    }
                                    if (t_link.type == 1)
                                    {
                                        cost_label = t_link.cost;
                                        travel_time_label = t_link.travle_time;
                                    }
                                    if (t_link.type == 2)
                                    {
                                        int t_max = Math.Min(g_number_of_time_intervals, (t + 1 + (t_link.travle_time - 1)));
                                        double LR_transaction_cost = 0;
                                        for (int s = t + 1; s <= t_max; s++)//multiplier time region s is the id of time_intervel
                                        {
                                            LR_transaction_cost += t_link.time_dependent_pure_LR_trasaction_multiplier[s - 1];
                                        }
                                        cost_label = t_link.cost + LR_transaction_cost;
                                        travel_time_label = t_link.travle_time;
                                    }
                                    if (t_link.type == 3)
                                    {
                                        double LR_handling_cost = t_link.time_dependent_pure_LR_coupling_multiplier[t];
                                        cost_label = t_link.cost + LR_handling_cost;
                                        travel_time_label = t_link.travle_time;
                                    }
                                    if (t + travel_time_label < g_number_of_time_intervals)
                                    {
                                        if (label_cost_array[n, t] + cost_label < label_cost_array[this_to_node_index, t + travel_time_label])
                                        {
                                            label_cost_array[this_to_node_index, t + travel_time_label] = label_cost_array[n, t] + cost_label;
                                            pre_node_id_array[this_to_node_index, t + travel_time_label] = this_from_node.id;
                                            pre_time_interval_array[this_to_node_index, t + travel_time_label] = t + 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region back trace

                int lowerbound_arc_to_node_id = truck.DestinationI;
                int lowerbound_arc_to_time = truck.DestinationT;
                do
                {
                    int to_node_index = 0;
                    for (int i = 0; i < truck.available_node_list.Count; i++)
                    {
                        if (truck.available_node_list[i].id == lowerbound_arc_to_node_id)
                        {
                            to_node_index = i;
                            break;
                        }
                    }

                    int lowerbound_arc_from_node_id = pre_node_id_array[to_node_index, lowerbound_arc_to_time - 1];
                    int lowerbound_arc_from_time = pre_time_interval_array[to_node_index, lowerbound_arc_to_time - 1];
                    Arc lowerbound_arc = new Arc(lowerbound_arc_from_node_id, lowerbound_arc_to_node_id, lowerbound_arc_from_time, lowerbound_arc_to_time);
                    truck.arclist_LR_path.Add(lowerbound_arc);
                    lowerbound_arc_to_node_id = lowerbound_arc_from_node_id;
                    lowerbound_arc_to_time = lowerbound_arc_from_time;
                }
                while (lowerbound_arc_to_node_id != 0);

                #endregion
            }

            //dynamic programming of cranes using pure lagrangian
            foreach (Crane crane in crane_list)
            {
                #region intialization

                crane.arc_list_LR_path.Clear();
                List<Crane> crane_list_without_c = Get_crane_list_without_c(crane);
                int crane_availabel_node_number = crane.available_node_list.Count();
                crane.lower_bound_travel_time = 0;
                //set pre_node_id[,] pre_time_interval[,] and label_cost[,] 
                int[,] pre_node_id_array = new int[crane_availabel_node_number, g_number_of_time_intervals];
                int[,] pre_time_interval_array = new int[crane_availabel_node_number, g_number_of_time_intervals];
                double[,] label_cost_array = new double[crane_availabel_node_number, g_number_of_time_intervals];
                for (int n = 0; n < crane_availabel_node_number; n++)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        pre_node_id_array[n, t] = -1;
                        pre_time_interval_array[n, t] = -1;
                        label_cost_array[n, t] = MAX_LABEL_COST;
                    }
                }
                pre_node_id_array[Get_index_in_crane_availabel_node_list(crane.OriginI, crane), crane.OriginT - 1] = 0;
                pre_time_interval_array[Get_index_in_crane_availabel_node_list(crane.OriginI, crane), crane.OriginT - 1] = 0;
                label_cost_array[Get_index_in_crane_availabel_node_list(crane.OriginI, crane), crane.OriginT - 1] = 0;

                #endregion

                #region dynamic programming begin

                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    for (int n = 0; n < crane.available_node_list.Count; n++)
                    {
                        if (pre_node_id_array[n, t] != -1)
                        {
                            CraneNode this_from_node = crane.available_node_list[n];
                            double cost_label = 0;
                            int travel_time_label = 0;

                            //1-label the node in next time intervel: waiting arc 
                            travel_time_label = 1;
                            if (this_from_node.description == "crane node0")
                            {
                                double LR_non_crossing_cost = this_from_node.time_dependent_pure_LR_no_crossing_multiplier_c[crane.id - 1][t];
                                cost_label = LR_non_crossing_cost;
                                if ((t + travel_time_label < g_number_of_time_intervals))
                                {
                                    if (label_cost_array[n, t] + cost_label < label_cost_array[n, t + travel_time_label])
                                    {
                                        label_cost_array[n, t + travel_time_label] = label_cost_array[n, t] + cost_label;
                                        pre_node_id_array[n, t + travel_time_label] = this_from_node.id;
                                        pre_time_interval_array[n, t + travel_time_label] = t + 1;
                                    }
                                }
                            }

                            //2-label the outgoing node
                            foreach (CraneLink c_link in this_from_node.Outgoing_LinkList)
                            {
                                if (crane.available_node_list.Contains(c_link.to_node))
                                {
                                    travel_time_label = c_link.travle_time;
                                    CraneNode this_to_node = c_link.to_node;
                                    int this_to_node_index = 0;
                                    for (int i = 0; i < crane.available_node_list.Count; i++)
                                    {
                                        if (this_to_node.id == crane.available_node_list[i].id)
                                        {
                                            this_to_node_index = i;
                                            break;
                                        }
                                    }
                                    if (c_link.type == 1)
                                    {
                                        double LR_non_crossing_cost = c_link.time_dependent_pure_LR_no_crossing_multiplier_c[crane.id - 1][t];
                                        Arc this_moving_arc = new Arc(c_link.from_node_id, c_link.to_node_id, t + 1, t + 1 + c_link.travle_time);
                                        for (int i = 0; i < c_link.CCA_list[t].crane_node_conflict_list.Count; i++)
                                        {
                                            CraneNode crane_node_other = c_link.CCA_list[t].crane_node_conflict_list[i];
                                            int tt = c_link.CCA_list[t].crane_node_conflict_time_index_list[i];
                                            foreach (Crane other_crane in crane_list_without_c)
                                            {
                                                LR_non_crossing_cost += crane_node_other.time_dependent_pure_LR_no_crossing_multiplier_c[other_crane.id - 1][tt];
                                            }
                                        }
                                        for (int i = 0; i < c_link.CCA_list[t].crane_link_conflict_list.Count; i++)
                                        {
                                            CraneLink crane_link_other = c_link.CCA_list[t].crane_link_conflict_list[i];
                                            int tt = c_link.CCA_list[t].crane_link_conflict_time_index_list[i];
                                            foreach (Crane other_crane in crane_list_without_c)
                                            {
                                                LR_non_crossing_cost += crane_link_other.time_dependent_pure_LR_no_crossing_multiplier_c[other_crane.id - 1][tt];
                                            }
                                        }

                                        cost_label = LR_non_crossing_cost;
                                    }
                                    if (c_link.type == 3)
                                    {
                                        double LR_non_crossing_cost = c_link.time_dependent_pure_LR_no_crossing_multiplier_c[crane.id - 1][t];
                                        cost_label = LR_non_crossing_cost;
                                    }
                                    if (c_link.type == 2)
                                    {
                                        TruckLink t_link_coupling = Get_truck_link(c_link.from_node.id, c_link.to_node.id, truck_link_list);
                                        c_link.time_dependent_pure_LR_coupling_multiplier[t] = t_link_coupling.time_dependent_pure_LR_coupling_multiplier[t];
                                        double LR_handling_cost = c_link.time_dependent_pure_LR_coupling_multiplier[t];
                                        double LR_non_crossing_cost = c_link.time_dependent_pure_LR_no_crossing_multiplier_c[crane.id - 1][t];
                                        cost_label = -LR_handling_cost + LR_non_crossing_cost;
                                    }
                                    //compare and label
                                    if (t + travel_time_label < g_number_of_time_intervals)
                                    {
                                        if (label_cost_array[n, t] + cost_label < label_cost_array[this_to_node_index, t + travel_time_label])
                                        {
                                            label_cost_array[this_to_node_index, t + travel_time_label] = label_cost_array[n, t] + cost_label;
                                            pre_node_id_array[this_to_node_index, t + travel_time_label] = this_from_node.id;
                                            pre_time_interval_array[this_to_node_index, t + travel_time_label] = t + 1;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

                #endregion

                #region bace trace

                int lowerbound_arc_to_node_id = crane.DestinationI;
                int lowerbound_arc_to_time = crane.DestinationT;
                do
                {
                    int to_node_index = Get_index_in_crane_availabel_node_list(lowerbound_arc_to_node_id, crane);

                    int lowerbound_arc_from_node_id = pre_node_id_array[to_node_index, lowerbound_arc_to_time - 1];
                    int lowerbound_arc_from_time = pre_time_interval_array[to_node_index, lowerbound_arc_to_time - 1];
                    Arc lowerbound_arc = new Arc(lowerbound_arc_from_node_id, lowerbound_arc_to_node_id, lowerbound_arc_from_time, lowerbound_arc_to_time);
                    crane.arc_list_LR_path.Add(lowerbound_arc);
                    lowerbound_arc_to_node_id = lowerbound_arc_from_node_id;
                    lowerbound_arc_to_time = lowerbound_arc_from_time;
                }
                while (lowerbound_arc_to_node_id != 0);

                #endregion
            }

            foreach (Truck truck in truck_list)
            {
                foreach (Arc t_arc in truck.arclist_LR_path)
                {
                    TruckNode t_from_node = Get_truck_node_by_id(t_arc.I, truck_node_list);
                    TruckNode t_to_node = Get_truck_node_by_id(t_arc.J, truck_node_list);
                    if (t_arc.I == t_arc.J)
                    {
                        lower_bound_cost += t_from_node.cost;
                    }
                    if (t_arc.I != t_arc.J)
                    {
                        TruckLink t_link = Get_truck_link(t_from_node.id, t_to_node.id, truck_link_list);
                        lower_bound_cost += t_link.cost;
                    }
                }
            }
            foreach (TruckLink t_link in truck_link_list)
            {
                if (t_link.type == 2)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        Arc arc = new Arc(t_link.from_node_id, t_link.to_node_id, t + 1, t + 1 + t_link.travle_time);
                        int sum_x_ijts_v_in_clicue = 0;
                        int trasaction_time = t_link.travle_time;
                        int t_min = Math.Max(0, t - (trasaction_time - 1));
                        for (int tt = t_min; tt <= t; tt++)
                        {
                            Arc arc_clicue = new Arc(t_link.from_node_id, t_link.to_node_id, tt + 1, tt + 1 + t_link.travle_time);
                            foreach (Truck truck in truck_list)
                            {
                                if (truck.OriginT > arc_clicue.T)
                                {
                                    break;
                                }
                                if (truck.OriginT <= arc_clicue.T)
                                {
                                    foreach (Arc t_arc in truck.arclist_LR_path)
                                    {
                                        if (t_arc.T < arc_clicue.T)
                                        {
                                            break;
                                        }
                                        if (t_arc.T == arc_clicue.T)
                                        {
                                            if (t_arc.I == arc_clicue.I && t_arc.J == arc_clicue.J)
                                            {
                                                sum_x_ijts_v_in_clicue += 1;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        int cap = 0;
                        if (t_link.from_node.name_code == 10)
                        { cap = GateCap1; }
                        if (t_link.from_node.name_code == 20)
                        { cap = GateCap2; }
                        int sub_grandient = sum_x_ijts_v_in_clicue - cap;
                        lower_bound_cost += t_link.time_dependent_pure_LR_trasaction_multiplier[t] * sub_grandient;
                    }
                }
                if (t_link.type == 3)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        Arc handling_arc = new Arc(t_link.from_node_id, t_link.to_node_id, t + 1, t + 1 + t_link.travle_time);
                        int sum_x_ijts_v = 0;
                        int sum_y_ijts_c = 0;
                        foreach (Truck truck in truck_list)
                        {
                            if (truck.OriginT >= handling_arc.T)
                            {
                                break;
                            }
                            if (truck.OriginT < handling_arc.T)
                            {
                                foreach (Arc t_arc in truck.arclist_LR_path)
                                {
                                    if (t_arc.T < handling_arc.T)
                                    {
                                        break;
                                    }
                                    if (t_arc.T == handling_arc.T)
                                    {
                                        if (t_arc.I == handling_arc.I && t_arc.J == handling_arc.J)
                                        {
                                            sum_x_ijts_v += 1;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        foreach (Crane crane in crane_list)
                        {
                            foreach (Arc c_arc in crane.arc_list_LR_path)
                            {
                                if (c_arc.T < handling_arc.T)
                                {
                                    break;
                                }
                                if (c_arc.T == handling_arc.T)
                                {
                                    if (c_arc.I == handling_arc.I && c_arc.J == handling_arc.J)
                                    {
                                        sum_y_ijts_c += 1;
                                        break;
                                    }
                                }
                            }
                        }
                        int sub_grandient = sum_x_ijts_v - sum_y_ijts_c;
                        lower_bound_cost += t_link.time_dependent_pure_LR_coupling_multiplier[t] * sub_grandient;
                    }
                }
            }
            foreach (CraneNode c_node in crane_node_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    Arc waiting_arc = new Arc(c_node.id, c_node.id, t + 1, t + 2);
                    List<Arc> incompatible_arc_list = c_node.incompatible_arc_list_t[t];
                    foreach (Crane crane in crane_list)
                    {
                        int y_ijts = 0;
                        foreach (Arc c_arc in crane.arc_list_LR_path)
                        {
                            if (c_arc.T < waiting_arc.T)
                            { break; }
                            if (c_arc.T == waiting_arc.T)
                            {
                                if (c_arc.I == waiting_arc.I && c_arc.J == waiting_arc.J)
                                {
                                    y_ijts += 1;
                                    break;
                                }
                            }
                        }
                        int sum_crane_without_c_use_incompatible_arc = 0;
                        foreach (Arc incompatible_arc in incompatible_arc_list)
                        {
                            foreach (Crane other_crane in crane_list)
                            {
                                if (other_crane.id != crane.id)
                                {
                                    foreach (Arc c_arc in other_crane.arc_list_LR_path)
                                    {
                                        if (c_arc.T < incompatible_arc.T)
                                        {
                                            break;
                                        }
                                        if (c_arc.T == incompatible_arc.T)
                                        {
                                            if (c_arc.I == incompatible_arc.I && c_arc.J == incompatible_arc.J)
                                            {
                                                sum_crane_without_c_use_incompatible_arc += 1;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        int sub_gradient = y_ijts + sum_crane_without_c_use_incompatible_arc - 1;
                        lower_bound_cost += c_node.time_dependent_pure_LR_no_crossing_multiplier_c[crane.id - 1][t] * sub_gradient;
                    }
                }
            }
            foreach (CraneLink c_link in crane_link_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    Arc arc = new Arc(c_link.from_node_id, c_link.to_node_id, t + 1, t + 1 + c_link.travle_time);
                    List<Arc> incompatible_arc_list = c_link.incompatible_arc_list_t[t];
                    foreach (Crane crane in crane_list)
                    {
                        int y_ijts = 0;
                        foreach (Arc c_arc in crane.arc_list_LR_path)
                        {
                            if (c_arc.T < arc.T)
                            { break; }
                            if (c_arc.T == arc.T)
                            {
                                if (c_arc.I == arc.I && c_arc.J == arc.J)
                                {
                                    y_ijts += 1;
                                    break;
                                }
                            }
                        }
                        int sum_crane_without_c_use_incompatible_arc = 0;
                        foreach (Arc incompatible_arc in incompatible_arc_list)
                        {
                            foreach (Crane other_crane in crane_list)
                            {
                                if (other_crane.id != crane.id)
                                {
                                    foreach (Arc c_arc in other_crane.arc_list_LR_path)
                                    {
                                        if (c_arc.T < incompatible_arc.T)
                                        {
                                            break;
                                        }
                                        if (c_arc.T == incompatible_arc.T)
                                        {
                                            if (c_arc.I == incompatible_arc.I && c_arc.J == incompatible_arc.J)
                                            {
                                                sum_crane_without_c_use_incompatible_arc += 1;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        int sub_gradient = y_ijts + sum_crane_without_c_use_incompatible_arc - 1;
                        lower_bound_cost += c_link.time_dependent_pure_LR_no_crossing_multiplier_c[crane.id - 1][t] * sub_gradient;
                    }
                }
            }

            return lower_bound_cost;
        }

        public bool feasible_juge()
        {
            bool Is_feasible = true;

            #region gate capacity constraint check

            TruckLink arrival_transaction_link = new TruckLink();
            TruckLink departure_transaction_link = new TruckLink();
            foreach (TruckLink t_link in truck_link_list)
            {
                if (t_link.type == 2)
                {
                    if (t_link.from_node.name_code == 10)
                    {
                        arrival_transaction_link = t_link;
                    }
                    if (t_link.from_node.name_code == 20)
                    {
                        departure_transaction_link = t_link;
                    }
                }
            }
            List<Arc> transaction_arc_list_arrival = new List<Arc>();
            List<Arc> transaction_arc_list_departure = new List<Arc>();
            foreach (Truck truck in truck_list)
            {
                foreach (Arc t_arc in truck.arclist_ADMM_path)
                {
                    if (t_arc.I == arrival_transaction_link.from_node_id && t_arc.J == arrival_transaction_link.to_node_id)
                    { transaction_arc_list_arrival.Add(t_arc); }
                    if (t_arc.I == departure_transaction_link.from_node_id && t_arc.J == departure_transaction_link.to_node_id)
                    { transaction_arc_list_departure.Add(t_arc); }
                }
            }
            foreach (Arc t_arc in transaction_arc_list_arrival)
            {
                int t_e = t_arc.T;
                int t_s = t_e - (arrival_transaction_link.travle_time - 1);
                int volume = 0;
                for (int t = t_s; t <= t_e; t++)
                {
                    foreach (Arc tt_arc in transaction_arc_list_arrival)
                    {
                        if (tt_arc.T == t)
                        {
                            volume++;
                        }
                    }
                }
                if (volume > GateCap1)
                {
                    Is_feasible = false;
                    goto end_label;
                }
            }
            foreach (Arc t_arc in transaction_arc_list_departure)
            {
                int t_e = t_arc.T;
                int t_s = t_e - (departure_transaction_link.travle_time - 1);
                int volume = 0;
                for (int t = t_s; t <= t_e; t++)
                {
                    foreach (Arc tt_arc in transaction_arc_list_departure)
                    {
                        if (tt_arc.T == t)
                        {
                            volume++;
                        }
                    }
                }
                if (volume > GateCap2)
                {
                    Is_feasible = false;
                    goto end_label;
                }
            }

            #endregion

            #region coupling check

            List<Arc> truck_handling_arc_list = new List<Arc>();
            foreach (Truck truck in truck_list)
            {
                foreach (Arc t_arc in truck.arclist_ADMM_path)
                {
                    if (t_arc.I == truck.ContainerBayID)
                    {
                        truck_handling_arc_list.Add(t_arc);
                    }
                }
            }
            foreach (Arc t_handling_arc in truck_handling_arc_list)
            {
                int t_use_num = 0;
                int c_use_num = 0;
                foreach (Arc t_arc in truck_handling_arc_list)
                {
                    if (t_arc.I == t_handling_arc.I && t_arc.J == t_handling_arc.J && t_arc.T == t_handling_arc.T && t_arc.S == t_handling_arc.S)
                    {
                        t_use_num++;
                    }
                }
                foreach (Crane crane in crane_list)
                {
                    foreach (Arc c_arc in crane.arc_list_ADMM_path)
                    {
                        if (c_arc.I == t_handling_arc.I && c_arc.J == t_handling_arc.J && c_arc.T == t_handling_arc.T && c_arc.S == t_handling_arc.S)
                        {
                            c_use_num++;
                        }
                    }
                }
                if (t_use_num != c_use_num)
                {
                    Is_feasible = false;
                    goto end_label;
                }
            }

            #endregion

            #region non_crossing check

            foreach (Crane crane in crane_list)
            {
                crane.arc_moving_list.Clear();
                foreach (Arc c_arc in crane.arc_list_ADMM_path)
                {
                    if (c_arc.I != 0)
                    {
                        if (c_arc.I != c_arc.J)
                        {
                            if (truck_node_list[c_arc.I - 1].description == "crane node0" && truck_node_list[c_arc.J - 1].description == "crane node0")
                            {
                                crane.arc_moving_list.Add(c_arc);
                            }
                        }
                    }
                }
            }
            List<Crane> crane_list_without_c = new List<Crane>();
            foreach (Crane crane in crane_list)
            {
                crane_list_without_c.Add(crane);
            }
            foreach (Crane crane in crane_list)
            {
                crane_list_without_c.Remove(crane);
                foreach (Arc c_arc in crane.arc_list_ADMM_path)
                {
                    if (c_arc.I != 0)
                    {
                        if (c_arc.I == c_arc.J)
                        {
                            CraneNode c_node = Get_crane_node(truck_node_list[c_arc.I - 1].name_code, crane_node_list);
                            List<Arc> incompatible_arc_list = Get_incompatible_arc_list(c_node, c_arc.T - 1);
                            foreach (Arc c_in_arc in incompatible_arc_list)
                            {
                                foreach (Crane c in crane_list_without_c)
                                {
                                    foreach (Arc c_moving_arc in c.arc_moving_list)
                                    {
                                        if (c_moving_arc.I == c_in_arc.I && c_moving_arc.J == c_in_arc.J && c_moving_arc.T == c_in_arc.T && c_moving_arc.S == c_in_arc.S)
                                        {
                                            Is_feasible = false;
                                            goto end_label;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            CraneLink c_link = Get_crane_link(c_arc.I, c_arc.J, crane_link_list);
                            List<Arc> incompatible_arc_list = Get_incompatible_arc_list(c_link, c_arc.T - 1);
                            foreach (Arc c_in_arc in incompatible_arc_list)
                            {
                                foreach (Crane c in crane_list_without_c)
                                {
                                    foreach (Arc c_moving_arc in c.arc_moving_list)
                                    {
                                        if (c_moving_arc.I == c_in_arc.I && c_moving_arc.J == c_in_arc.J && c_moving_arc.T == c_in_arc.T && c_moving_arc.S == c_in_arc.S)
                                        {
                                            Is_feasible = false;
                                            goto end_label;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                crane_list_without_c.Add(crane);
            }

            #endregion

            end_label:

            return Is_feasible;

        }

        public void update_pure_LR_multiplers()
        {
            foreach (TruckLink t_link in truck_link_list)
            {
                if (t_link.type == 3)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        Arc coupling_arc = new Arc(t_link.from_node_id, t_link.to_node_id, t + 1, (t + 1 + t_link.travle_time));
                        int sum_truck_use_link_at_t = 0;
                        foreach (Truck truck in truck_list)
                        {
                            foreach (Arc t_arc in truck.arclist_LR_path)
                            {
                                if (t_arc.T < coupling_arc.T)
                                {
                                    break;
                                }
                                if (t_arc.I == coupling_arc.I && t_arc.J == coupling_arc.J && t_arc.T == coupling_arc.T && t_arc.S == coupling_arc.S)
                                {
                                    sum_truck_use_link_at_t++;
                                    break;
                                }
                            }
                        }
                        int sum_crane_use_link_at_t = 0;
                        foreach (Crane crane in crane_list)
                        {
                            foreach (Arc c_arc in crane.arc_list_LR_path)
                            {
                                if (c_arc.T < coupling_arc.T)
                                {
                                    break;
                                }
                                if (c_arc.I == coupling_arc.I && c_arc.J == coupling_arc.J && c_arc.T == coupling_arc.T && c_arc.S == coupling_arc.S)
                                {
                                    sum_crane_use_link_at_t++;
                                    break;
                                }
                            }
                        }
                        int sub_grandient = sum_truck_use_link_at_t - sum_crane_use_link_at_t;
                        t_link.time_dependent_pure_LR_coupling_multiplier[t] = t_link.time_dependent_pure_LR_coupling_multiplier[t] + step_size_LR * sub_grandient;
                    }
                }
            }

            foreach (TruckLink t_link in truck_link_list)
            {
                if (t_link.type == 2)
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        int sum_truck_use_link_set_at_t = 0;
                        int t_min = Math.Max(1, t + 1 - (t_link.travle_time - 1));
                        for (int tt = t_min; tt <= t + 1; tt++)
                        {
                            Arc gate_transaction_arc = new Arc(t_link.from_node_id, t_link.to_node_id, tt, tt + t_link.travle_time);
                            foreach (Truck truck in truck_list)
                            {
                                if (truck.OriginT <= gate_transaction_arc.T)
                                {
                                    foreach (Arc t_arc in truck.arclist_LR_path)
                                    {
                                        if (t_arc.T < gate_transaction_arc.T)
                                        {
                                            break;
                                        }
                                        if (t_arc.I == gate_transaction_arc.I && t_arc.J == gate_transaction_arc.J && t_arc.T == gate_transaction_arc.T && t_arc.S == gate_transaction_arc.S)
                                        {
                                            sum_truck_use_link_set_at_t++;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        int cap = 0;
                        if (t_link.from_node.name_code == 10)
                        {
                            cap = GateCap1;
                        }
                        if (t_link.from_node.name_code == 20)
                        {
                            cap = GateCap2;
                        }
                        int sub_grandient = sum_truck_use_link_set_at_t - cap;
                        t_link.time_dependent_pure_LR_trasaction_multiplier[t] = Math.Max(0, t_link.time_dependent_pure_LR_trasaction_multiplier[t] + step_size_LR * sub_grandient);
                    }
                }
            }

            foreach (CraneLink c_link in crane_link_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    Arc crane_arc = new Arc(c_link.from_node_id, c_link.to_node_id, t + 1, (t + 1 + c_link.travle_time));
                    List<Arc> incompatible_crane_arc_list = c_link.incompatible_arc_list_t[t];
                    for (int c = 0; c < crane_list.Count; c++)
                    {
                        int y_ijts_c = 0;
                        foreach (Arc c_arc in crane_list[c].arc_list_LR_path)
                        {
                            if (c_arc.T < crane_arc.T)
                            {
                                break;
                            }
                            if (c_arc.T == crane_arc.T)
                            {
                                if (c_arc.I == crane_arc.I && c_arc.J == crane_arc.J)
                                {
                                    y_ijts_c += 1;
                                    break;
                                }
                            }
                        }
                        int sum_crane_without_c_use_arc = 0;
                        foreach (Arc incompatible_arc in incompatible_crane_arc_list)
                        {
                            foreach (Crane other_crane in crane_list)
                            {
                                if (other_crane.id != crane_list[c].id)
                                {
                                    foreach (Arc c_arc in other_crane.arc_list_LR_path)
                                    {
                                        if (c_arc.T < incompatible_arc.T)
                                        {
                                            break;
                                        }
                                        if (c_arc.T == incompatible_arc.T)
                                        {
                                            if (c_arc.I == incompatible_arc.I && c_arc.J == incompatible_arc.J)
                                            {
                                                sum_crane_without_c_use_arc += 1;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        int sub_grandient_c = y_ijts_c + sum_crane_without_c_use_arc - 1;
                        c_link.time_dependent_pure_LR_no_crossing_multiplier_c[c][t] = Math.Max(0, c_link.time_dependent_pure_LR_no_crossing_multiplier_c[c][t] + step_size_LR * sub_grandient_c);
                    }
                }
            }
            foreach (CraneNode c_node in crane_node_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    Arc crane_arc = new Arc(c_node.id, c_node.id, t + 1, t + 2);
                    List<Arc> incompatible_crane_arc_list = c_node.incompatible_arc_list_t[t];
                    for (int c = 0; c < crane_list.Count; c++)
                    {
                        int y_ijts_c = 0;
                        foreach (Arc c_arc in crane_list[c].arc_list_LR_path)
                        {
                            if (c_arc.T < crane_arc.T)
                            {
                                break;
                            }
                            if (c_arc.T == crane_arc.T)
                            {
                                if (c_arc.I == crane_arc.I && c_arc.J == crane_arc.J)
                                {
                                    y_ijts_c += 1;
                                    break;
                                }
                            }
                        }
                        int sum_crane_without_c_use_arc = 0;
                        foreach (Arc incompatible_arc in incompatible_crane_arc_list)
                        {
                            foreach (Crane other_crane in crane_list)
                            {
                                if (other_crane.id != crane_list[c].id)
                                {
                                    foreach (Arc c_arc in other_crane.arc_list_LR_path)
                                    {
                                        if (c_arc.T < incompatible_arc.T)
                                        {
                                            break;
                                        }
                                        if (c_arc.T == incompatible_arc.T)
                                        {
                                            if (c_arc.I == incompatible_arc.I && c_arc.J == incompatible_arc.J)
                                            {
                                                sum_crane_without_c_use_arc += 1;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        int sub_grandient_c = y_ijts_c + sum_crane_without_c_use_arc - 1;
                        c_node.time_dependent_pure_LR_no_crossing_multiplier_c[c][t] = Math.Max(0, c_node.time_dependent_pure_LR_no_crossing_multiplier_c[c][t] + step_size_LR * sub_grandient_c);
                    }
                }
            }
        }

        public TruckNode Get_truck_node_by_id(int node_id, List<TruckNode> truck_node_list)
        {
            TruckNode this_node = new TruckNode();
            foreach (TruckNode t_node in truck_node_list)
            {
                if (t_node.id == node_id)
                {
                    this_node = t_node;
                }
            }
            return this_node;
        }
        public int Get_name_node(int node_id, List<TruckNode> NodeList)
        {
            int name_code = 0;
            foreach (TruckNode node in NodeList)
            {
                if (node.id == node_id)
                {
                    name_code = node.name_code;
                    break;
                }
            }
            return name_code;
        }
        public CraneNode Get_crane_node(int name_code, List<CraneNode> NodeList)
        {
            CraneNode cranenode = new CraneNode();
            foreach (CraneNode node in NodeList)
            {
                if (node.name_code == name_code)
                {
                    cranenode = node;
                    break;
                }
            }
            return cranenode;
        }
        public TruckLink Get_truck_link(int from_node_id, int to_node_id, List<TruckLink> link_list)
        {
            TruckLink trucklink = new TruckLink();
            foreach (TruckLink t_link in link_list)
            {
                if (t_link.from_node_id == from_node_id && t_link.to_node_id == to_node_id)
                {
                    trucklink = t_link;
                    break;
                }
            }
            return trucklink;
        }
        public CraneLink Get_crane_link(int from_node_id, int to_node_id, List<CraneLink> link_list)
        {
            CraneLink cranelink = new CraneLink();
            foreach (CraneLink c_link in link_list)
            {
                if (c_link.from_node_id == from_node_id && c_link.to_node_id == to_node_id)
                {
                    cranelink = c_link;
                    break;
                }
            }
            return cranelink;
        }
        public int Get_index_in_crane_availabel_node_list(int node_id, Crane crane)
        {
            int node_index = 0;
            for (int i = 0; i < crane.available_node_list.Count; i++)
            {
                if (node_id == crane.available_node_list[i].id)
                {
                    node_index = i;
                    break;
                }
            }
            return node_index;
        }
        public List<Arc> Get_incompatible_arc_list(CraneLink crane_link, int t)
        {
            List<Arc> incompatible_arc_list = new List<Arc>();
            int physical_crane_name_code = crane_link.from_node.name_code / 10 * 10;
            CraneNode physical_crane_node = Get_crane_node(physical_crane_name_code, crane_node_list);
            int moving_link_travel_time = 0;
            foreach (Link ll in crane_link_list)
            {
                if (ll.type == 1)
                {
                    moving_link_travel_time = ll.travle_time;
                    break;
                }
            }

            if (crane_link.type == 1)
            {
                CraneNode physical_crane_node_orgin = crane_link.from_node;
                CraneNode physical_crane_node_destination = crane_link.to_node;
                for (int tt = 0; tt <= crane_link.travle_time; tt++)
                {
                    int from_t = (t + 1) + tt - moving_link_travel_time;
                    int to_t = (t + 1) + tt;
                    Arc arc_d_to_o = new Arc(physical_crane_node_destination.id, physical_crane_node_orgin.id, from_t, to_t);
                    incompatible_arc_list.Add(arc_d_to_o);
                }
                Arc arc_o_to_d = new Arc(physical_crane_node_orgin.id, physical_crane_node_destination.id, (t + 1), (t + 1 + crane_link.travle_time));
                incompatible_arc_list.Add(arc_o_to_d);
                if (physical_crane_node_orgin.name_code < physical_crane_node_destination.name_code)
                {
                    CraneNode physical_crane_node_down = Get_crane_node((physical_crane_node_orgin.name_code - 10), crane_node_list);
                    CraneNode physical_crane_node_up = Get_crane_node((physical_crane_node_destination.name_code + 10), crane_node_list);
                    Arc arc_down_origin = new Arc(physical_crane_node_down.id, physical_crane_node_orgin.id, (t + 1 - moving_link_travel_time), (t + 1));
                    Arc arc_up_destination = new Arc(physical_crane_node_up.id, physical_crane_node_destination.id, (t + 1), (t + 1 + moving_link_travel_time));
                    incompatible_arc_list.Add(arc_down_origin);
                    incompatible_arc_list.Add(arc_up_destination);
                }
                if (physical_crane_node_orgin.name_code > physical_crane_node_destination.name_code)
                {
                    CraneNode physical_crane_node_down = Get_crane_node((physical_crane_node_destination.name_code - 10), crane_node_list);
                    CraneNode physical_crane_node_up = Get_crane_node((physical_crane_node_orgin.name_code + 10), crane_node_list);
                    Arc arc_up_origin = new Arc(physical_crane_node_up.id, physical_crane_node_orgin.id, (t + 1 - moving_link_travel_time), (t + 1));
                    Arc arc_down_destination = new Arc(physical_crane_node_down.id, physical_crane_node_destination.id, (t + 1), (t + 1 + moving_link_travel_time));
                    incompatible_arc_list.Add(arc_up_origin);
                    incompatible_arc_list.Add(arc_down_destination);
                }
            }
            if (crane_link.type == 2 || crane_link.type == 3)
            {
                CraneNode physical_crane_node_down = Get_crane_node((physical_crane_name_code - 10), crane_node_list);
                CraneNode physical_crane_node_up = Get_crane_node((physical_crane_name_code + 10), crane_node_list);
                for (int tt = 0; tt <= crane_link.travle_time; tt++)
                {
                    int from_t = t + 1 + tt - moving_link_travel_time;
                    int to_t = t + 1 + tt;
                    Arc arc_down = new Arc(physical_crane_node_down.id, physical_crane_node.id, from_t, to_t);
                    incompatible_arc_list.Add(arc_down);
                    Arc arc_up = new Arc(physical_crane_node_up.id, physical_crane_node.id, from_t, to_t);
                    incompatible_arc_list.Add(arc_up);
                }
            }

            return incompatible_arc_list;
        }
        public List<Arc> Get_incompatible_arc_list(CraneNode crane_node, int t)
        {
            List<Arc> incompatible_arc_list = new List<Arc>();
            int physical_crane_name_code = crane_node.name_code / 10 * 10;
            CraneNode physical_crane_node = Get_crane_node(physical_crane_name_code, crane_node_list);
            int moving_link_travel_time = 0;
            foreach (Link ll in crane_link_list)
            {
                if (ll.type == 1)
                {
                    moving_link_travel_time = ll.travle_time;
                    break;
                }
            }
            CraneNode physical_crane_node_down = Get_crane_node((physical_crane_name_code - 10), crane_node_list);
            CraneNode physical_crane_node_up = Get_crane_node((physical_crane_name_code + 10), crane_node_list);
            for (int tt = 0; tt < 2; tt++)
            {
                int from_t = t + 1 + tt - moving_link_travel_time;
                int to_t = t + 1 + tt;
                Arc arc_down = new Arc(physical_crane_node_down.id, physical_crane_node.id, from_t, to_t);
                Arc arc_up = new Arc(physical_crane_node_up.id, physical_crane_node.id, from_t, to_t);
                incompatible_arc_list.Add(arc_down);
                incompatible_arc_list.Add(arc_up);
            }
            return incompatible_arc_list;
        }
        public List<Crane> Get_crane_list_without_c(Crane crane)
        {
            List<Crane> crane_list_without_c = new List<Crane>();
            foreach (Crane c in crane_list)
            {
                crane_list_without_c.Add(c);
            }
            crane_list_without_c.Remove(crane);
            return crane_list_without_c;
        }
        public bool Juge_cranelinklist_contain_link(Link this_link, List<CraneLink> link_list)
        {
            bool Is_contain = false;
            foreach (Link link in link_list)
            {
                if (link.id == this_link.id)
                {
                    Is_contain = true;
                    break;
                }
            }
            return Is_contain;
        }
        public void get_imcompatible_arc_set_for_crane_moving_arc()
        {
            foreach (CraneNode crane_node in crane_node_list)
            {
                if (crane_node.description == "crane node0")
                {
                    for (int t = 0; t < g_number_of_time_intervals; t++)
                    {
                        List<Arc> imcompatible_moving_arc_list = Get_incompatible_arc_list(crane_node, t);
                        foreach (Arc moving_arc in imcompatible_moving_arc_list)
                        {
                            if (moving_arc.T > 0)
                            {
                                CraneLink c_link = Get_crane_link(moving_arc.I, moving_arc.J, crane_link_list);
                                if (Juge_cranelinklist_contain_link(c_link, crane_link_list))
                                {
                                    c_link.CCA_list[moving_arc.T - 1].crane_node_conflict_list.Add(crane_node);
                                    c_link.CCA_list[moving_arc.T - 1].crane_node_conflict_time_index_list.Add(t);
                                }
                            }
                        }
                    }
                }
            }
            foreach (CraneLink crane_link in crane_link_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    List<Arc> imcompatible_moving_arc_list = Get_incompatible_arc_list(crane_link, t);
                    foreach (Arc moving_arc in imcompatible_moving_arc_list)
                    {
                        if (moving_arc.T > 0)
                        {
                            CraneLink c_link = Get_crane_link(moving_arc.I, moving_arc.J, crane_link_list);
                            if (Juge_cranelinklist_contain_link(c_link, crane_link_list))
                            {
                                c_link.CCA_list[moving_arc.T - 1].crane_link_conflict_list.Add(crane_link);
                                c_link.CCA_list[moving_arc.T - 1].crane_link_conflict_time_index_list.Add(t);
                            }
                        }
                    }
                }
            }
        }

        private void ADMM_OPT_Btn_Click(object sender, EventArgs e)
        {
            #region Initialization

            read_input_files();
            in_out_going_links_for_node();
            get_imcompatible_arc_set_for_crane_moving_arc();
            //initialize the multiplier and sub_grandients of trucks
            foreach (TruckLink t_link in truck_link_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    if (t_link.type == 2)
                    {
                        //ADMM
                        t_link.time_dependent_ADMM_LR_transaction_multiplier.Add(0);
                        //pure lagragian
                        t_link.time_dependent_pure_LR_trasaction_multiplier.Add(0);
                    }

                    if (t_link.type == 3)
                    {
                        //ADMM
                        t_link.time_dependent_ADMM_LR_coupling_multiplier.Add(0);
                        t_link.LR_at_t_same_num.Add(0);
                        //pure lagragian
                        t_link.time_dependent_pure_LR_coupling_multiplier.Add(0);
                    }

                    //sub_grandients
                    t_link.time_dependent_ADMM_transaction_sub_grandient.Add(0);
                    t_link.time_dependent_ADMM_transaction_last_sub_grandient.Add(0);
                    t_link.time_dependent_ADMM_coupling_sub_grandient.Add(0);
                    t_link.time_dependent_ADMM_coupling_last_sub_grandient.Add(0);
                }
            }
            //initialize the multiplier and sub_grandients of cranes
            foreach (CraneLink c_link in crane_link_list)
            {
                for (int c = 0; c < crane_list.Count; c++)
                {
                    //ADMM
                    List<double> ADMM_multiplier_miu_c = new List<double>();
                    c_link.time_dependent_ADMM_LR_non_crossing_multiplier_each_c.Add(ADMM_multiplier_miu_c);
                    //pure LR
                    List<double> pure_LR_no_crossing_multiplier_c = new List<double>();
                    c_link.time_dependent_pure_LR_no_crossing_multiplier_c.Add(pure_LR_no_crossing_multiplier_c);
                    //sub_grandient
                    List<double> sub_gradient_c = new List<double>();
                    c_link.time_dependent_ADMM_sub_grandient.Add(sub_gradient_c);
                    List<double> last_sub_gradient_c = new List<double>();
                    c_link.time_dependent_ADMM_last_sub_grandient.Add(last_sub_gradient_c);

                }
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    if (c_link.type == 2)
                    {
                        //ADMM
                        c_link.time_denpendent_ADMM_LR_coupling_multiplier.Add(0);
                        //pure_LR
                        c_link.time_dependent_pure_LR_coupling_multiplier.Add(0);
                    }
                    for (int c = 0; c < crane_list.Count; c++)
                    {
                        //ADMM
                        c_link.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[c].Add(2);
                        c_link.time_dependent_ADMM_sub_grandient[c].Add(0);
                        c_link.time_dependent_ADMM_last_sub_grandient[c].Add(0);
                        //pure_LR
                        c_link.time_dependent_pure_LR_no_crossing_multiplier_c[c].Add(0);
                    }
                }
            }
            foreach (CraneNode c_node in crane_node_list)
            {
                for (int c = 0; c < crane_list.Count; c++)
                {
                    //ADMM
                    List<double> ADMM_multiplier_miu_c = new List<double>();
                    c_node.time_dependent_ADMM_LR_non_crossing_multiplier_each_c.Add(ADMM_multiplier_miu_c);
                    //pure LR
                    List<double> pure_LR_no_crossing_multiplier_c = new List<double>();
                    c_node.time_dependent_pure_LR_no_crossing_multiplier_c.Add(pure_LR_no_crossing_multiplier_c);
                    //sub_grandient
                    List<double> sub_gradient_c = new List<double>();
                    c_node.time_dependent_ADMM_sub_grandient.Add(sub_gradient_c);
                    List<double> last_sub_gradient_c = new List<double>();
                    c_node.time_dependent_ADMM_last_sub_grandient.Add(last_sub_gradient_c);

                }
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    for (int c = 0; c < crane_list.Count; c++)
                    {
                        //ADMM
                        c_node.time_dependent_ADMM_LR_non_crossing_multiplier_each_c[c].Add(2);
                        c_node.time_dependent_ADMM_sub_grandient[c].Add(0);
                        c_node.time_dependent_ADMM_last_sub_grandient[c].Add(0);
                        //pure_LR
                        c_node.time_dependent_pure_LR_no_crossing_multiplier_c[c].Add(0);
                    }
                }
            }
            //initialize the crane non-crossing arc set
            foreach (CraneNode c_node in crane_node_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    c_node.incompatible_arc_list_t.Add(Get_incompatible_arc_list(c_node, t));
                }
            }
            foreach (CraneLink c_link in crane_link_list)
            {
                for (int t = 0; t < g_number_of_time_intervals; t++)
                {
                    c_link.incompatible_arc_list_t.Add(Get_incompatible_arc_list(c_link, t));
                }
            }

            FileStream fs = new FileStream("ADMM_results.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            List<double> upper_bound_result_list = new List<double>();
            List<double> lower_bound_result_list = new List<double>();
            List<double> best_upper_bound_result_list = new List<double>();
            List<double> best_lower_bound_result_list = new List<double>();
            List<string> feasible_juge_result_list = new List<string>();
            List<double> rou_list = new List<double>();
            List<double> sigma_list = new List<double>();

            #endregion

            #region ADMM procedure

            for (int k = 1; k <= MAX_Iteration; k++)
            {
                dynamic_programing_for_trucks();
                dynamic_programing_for_cranes();

                //calculate upper bound for feasible solutions
                bool feasible_state = feasible_juge();
                if (feasible_state == true)
                {
                    feasible_juge_result_list.Add("Yes");
                    upper_bound_result_list.Add(calculate_uper_bound());
                }
                else
                {
                    feasible_juge_result_list.Add("No");
                    upper_bound_result_list.Add(MAX_LABEL_COST);
                }
                //copy space-time paths of trucks
                List<Truck> truck_upper_bound_copy_list = new List<Truck>();
                foreach (Truck truck in truck_list)
                {
                    Truck truck_new = new Truck();
                    truck_new.id = truck.id;
                    foreach (Arc t_arc in truck.arclist_ADMM_path)
                    {
                        Arc t_arc_new = new Arc(0, 0, 0, 0);
                        t_arc_new.I = Get_name_node(t_arc.I, truck_node_list);
                        t_arc_new.J = Get_name_node(t_arc.J, truck_node_list);
                        t_arc_new.T = t_arc.T;
                        t_arc_new.S = t_arc.S;
                        truck_new.ArcList.Add(t_arc_new);
                    }
                    truck_upper_bound_copy_list.Add(truck_new);
                }
                StaticData.truck_upper_list.Add(truck_upper_bound_copy_list);
                //copy space-time paths of cranes
                List<Crane> crane_upper_bound_copy_list = new List<Crane>();
                int crane_id = 0;
                foreach (Crane crane in crane_list)
                {
                    crane_id++;
                    Crane crane_new = new Crane();
                    crane_new.id = crane_id;
                    foreach (Arc c_arc in crane.arc_list_ADMM_path)
                    {
                        Arc c_arc_new = new Arc(0, 0, 0, 0);
                        c_arc_new.I = Get_name_node(c_arc.I, truck_node_list);
                        c_arc_new.J = Get_name_node(c_arc.J, truck_node_list);
                        c_arc_new.T = c_arc.T;
                        c_arc_new.S = c_arc.S;
                        crane_new.ArcList.Add(c_arc_new);
                    }
                    crane_upper_bound_copy_list.Add(crane_new);
                }
                StaticData.crane_upper_list.Add(crane_upper_bound_copy_list);

                //calculate lower bound
                lower_bound_result_list.Add(calculate_lower_bound_using_pure_lagrangian());
                update_pure_LR_multiplers();
                step_size_LR = step_size_LR / (k + 1);

                //update ADMM multiplier and parameter
                update_ADMM_coupling_multiplier_for_trucks_and_cranes(k);
                update_ADMM_multipliers_for_trucks(k);
                update_ADMM_multipliers_for_cranes(k);

                //update the best upper and lower bound
                if (k == 1)
                {
                    best_upper_bound_result_list.Add(upper_bound_result_list[k - 1]);
                    best_lower_bound_result_list.Add(lower_bound_result_list[k - 1]);
                }
                else
                {
                    if (upper_bound_result_list[k - 1] < best_upper_bound_result_list[k - 2])
                    {
                        best_upper_bound_result_list.Add(upper_bound_result_list[k - 1]);
                    }
                    else
                    {
                        best_upper_bound_result_list.Add(best_upper_bound_result_list[k - 2]);
                    }
                    //lower bound
                    if (lower_bound_result_list[k - 1] > best_lower_bound_result_list[k - 2])
                    {
                        best_lower_bound_result_list.Add(lower_bound_result_list[k - 1]);
                    }
                    else
                    {
                        best_lower_bound_result_list.Add(best_lower_bound_result_list[k - 2]);
                    }
                }

                //record results
                string admm_result_record_string = k.ToString() + ": " +
                                     "UB: " + best_upper_bound_result_list[k - 1].ToString() +
                                     " Fesible_state: " + feasible_juge_result_list[k - 1] +
                                     " LB: " + best_lower_bound_result_list[k - 1].ToString();
                sw.WriteLine(admm_result_record_string);
                sw.Flush();
                richTextBox1.Text += admm_result_record_string + '\n';
            }

            sw.Close();
            fs.Close();

            #endregion

            MessageBox.Show("Finished!");
        }
    }
}
