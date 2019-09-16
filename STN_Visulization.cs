using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenerateInputdata
{
    public partial class Visualization : Form
    {
        List<Truck> TruckList = new List<Truck>();
        List<Truck> Truck_draw_list = new List<Truck>();
        List<Crane> CraneList = new List<Crane>();
        List<Crane> Crane_draw_list = new List<Crane>();

        public Visualization()
        {
            InitializeComponent();
        }

        private void STN_Visulization_Load(object sender, EventArgs e)
        {
            Arrival_Gate_Num_TBox.Text = "2";
            Departure_Gate_Num_TBox.Text = "2";
            Container_Bay_Num_TBox.Text = "8";
            Crane_Num_TBox.Text = "4";
            TimeHorizion_TBox.Text = "100";
        }


        int TTLength = 100;//default value
        int time_intervel_pix = 0;
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;  //improving quality
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            Pen pen = new Pen(Color.Black, 1);//solid line
            Pen pen_dashed = new Pen(Color.Black, 1);//dashed line
            Pen pen_truck = new Pen(Color.Blue, 2);//truck line
            Pen pen_crane = new Pen(Color.Red, 3);//truck line
            List<Color> ColorList = new List<Color>();
            ColorList.Add(Color.Orange);
            ColorList.Add(Color.Purple);
            ColorList.Add(Color.Red);
            ColorList.Add(Color.Olive);
            ColorList.Add(Color.Pink);

            pen_dashed.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };
            SolidBrush BrushPoint = new SolidBrush(Color.Black);//车站节点内部填充颜色：白色
            Font F1 = new Font("Times New Roman", 14f, FontStyle.Bold);//space time lable
            Font F2 = new Font("Times New Roman", 12f, FontStyle.Bold);//space-time dashed line
            Font F3 = new Font("Times New Roman", 12f, FontStyle.Bold);//truck id

            // paint space and time axis
            time_intervel_pix = 10;
            int total_time_length = time_intervel_pix * TTLength;

            //picturebox size
            int height = pictureBox1.Height;
            pictureBox1.Size = new Size(total_time_length + 200, height);

            // paint space and time axis
            Point point_o = new Point(100, 500);
            Point point_x = new Point(total_time_length, 500);
            Point point_y = new Point(100, 20);
            int s_length = point_o.Y - point_y.Y;
            int t_length = point_x.X - point_o.X;
            g.DrawLine(pen, point_o, point_x);
            g.DrawString("Time", F1, BrushPoint,(point_o.X+point_x.X)/2,point_o.Y+20);
            g.DrawLine(pen, point_o, point_y);
            g.DrawString("Space", F1, BrushPoint, point_o.X - 60, point_y.Y-15);
            //Read Data

            List<Node> NodeList = new List<Node>();
            DataTable input_node_dt = new DataTable();
            List<Node> TruckNodeList = new List<Node>();
            List<Node> GateNodeArrvial0 = new List<Node>();
            List<Node> GateNodeArrvial1 = new List<Node>();
            List<Node> GateNodeDeparture0 = new List<Node>();
            List<Node> GateNodeDeparture1 = new List<Node>();
            List<Node> CraneNode0 = new List<Node>();
            List<Node> CraneNode1 = new List<Node>();//loading node
            List<Node> CraneNode2 = new List<Node>();//unloading node
            input_node_dt = CSVFileHelper.OpenCSV("input nodes.csv");
            for (int i = 0; i < input_node_dt.Rows.Count; i++)
            {
                Node node = new Node();
                node.description = (string)input_node_dt.Rows[i][0];
                node.id = Convert.ToInt32(input_node_dt.Rows[i][1]);
                switch (node.description)
                {
                    case "truck node":
                        TruckNodeList.Add(node);
                        break;
                    case "gate node arrvial0":
                        GateNodeArrvial0.Add(node);
                        break;
                    case "gate node arrvial1":
                        GateNodeArrvial1.Add(node);
                        break;
                    case "gate node departure0":
                        GateNodeDeparture0.Add(node);
                        break;
                    case "gate node departure1":
                        GateNodeDeparture1.Add(node);
                        break;
                    case "crane node0":
                        CraneNode0.Add(node);
                        break;
                    case "crane node1":
                        CraneNode1.Add(node);
                        break;
                    case "crane node2":
                        CraneNode2.Add(node);
                        break;
                }
            }
            NodeList.Add(TruckNodeList[0]);
            NodeList.AddRange(GateNodeArrvial0);
            NodeList.AddRange(GateNodeArrvial1);
            NodeList.Add(TruckNodeList[1]);
            for (int i = 0; i < CraneNode0.Count; i++)
            {
                NodeList.Add(CraneNode1[i]);
                NodeList.Add(CraneNode0[i]);
                NodeList.Add(CraneNode2[i]);
            }
            NodeList.AddRange(GateNodeDeparture0);
            NodeList.AddRange(GateNodeDeparture1);
            NodeList.Add(TruckNodeList[2]);

            //time dashedline
            for (int i = 0; i < TTLength; i++)
            {
                Point p_a = GetPoint((i + 1), TruckNodeList[0].id, TTLength, NodeList, point_o, t_length, s_length);
                Point p_b = new Point(p_a.X, point_y.Y);
                if ((i==0)||(i + 1) % 10 == 0)
                {
                    g.DrawLine(pen_dashed, p_a, p_b);
                    g.DrawString((i + 1).ToString(), F2, BrushPoint, p_a.X - 5, p_a.Y + 6);
                    //g.DrawString((i + 1).ToString(), F2, BrushPoint, p_a.X - 5, p_b.Y - 10);
                }
            }
            //space dashedline
            int s_intervel = s_length / NodeList.Count;
            for (int i = 0; i < NodeList.Count; i++)
            {
                Point p_a = GetPoint(1, NodeList[i].id, TTLength, NodeList, point_o, t_length, s_length);
                Point p_b = new Point(point_x.X, p_a.Y);
                g.DrawString(NodeList[i].id.ToString(), F2, BrushPoint, p_a.X-40, p_a.Y-5);
                //g.DrawString(NodeList[i].id.ToString(), F2, BrushPoint, point_x.X +10, p_a.Y - 5);
                g.DrawLine(pen_dashed, p_a, p_b);
            }

            //draw gams scheduling results
            if (StaticData.GAMS_visulization_open == true)
            {
                //draw truck arcs
                foreach (Truck truck in Truck_draw_list)
                {
                    foreach (Arc t_arc in truck.ArcList)
                    {
                        Point p_a = GetPoint(t_arc.T, t_arc.I, TTLength, NodeList, point_o, t_length, s_length);
                        Point p_b = GetPoint(t_arc.S, t_arc.J, TTLength, NodeList, point_o, t_length, s_length);
                        g.DrawLine(pen_truck, p_a, p_b);
                        g.DrawString(truck.id.ToString(), F3, BrushPoint, (p_a.X + p_b.X) / 2, (p_a.Y + p_b.Y) / 2);
                    }
                }
                //draw crane arcs
                foreach (Crane crane in Crane_draw_list)
                {
                    int colorindex = (crane.id - 1) % 5;
                    //pen_crane.Color = ColorList[colorindex];
                    foreach (Arc c_arc in crane.ArcList)
                    {
                        Point p_a = GetPoint(c_arc.T, c_arc.I, TTLength, NodeList, point_o, t_length, s_length);
                        Point p_b = GetPoint(c_arc.S, c_arc.J, TTLength, NodeList, point_o, t_length, s_length);
                        g.DrawLine(pen_crane, p_a, p_b);
                    }
                }
            }

            //draw ADMM output_arcs for each iteration
            if (StaticData.ADMM_visulization_open == true)
            {
                //draw truck_upper_arcs
                if (StaticData.truck_upper_list.Count != 0)
                {
                    List<Truck> truck_upper_list = StaticData.truck_upper_list[StaticData.iter - 1];
                    foreach (Truck truck in truck_upper_list)
                    {
                        foreach (Arc t_arc in truck.ArcList)
                        {
                            if (t_arc.S <= TTLength)
                            {
                                Point p_a = GetPoint(t_arc.T, t_arc.I, TTLength, NodeList, point_o, t_length, s_length);
                                Point p_b = GetPoint(t_arc.S, t_arc.J, TTLength, NodeList, point_o, t_length, s_length);
                                g.DrawLine(pen_truck, p_a, p_b);
                                //only label the truck number on important arcs
                                //
                                if ((t_arc.I>=110&&t_arc.I<=180&&t_arc.J>=110&&t_arc.J<=190))
                                {
                                    g.DrawString(truck.id.ToString(), F3, BrushPoint, (p_a.X + p_b.X) / 2, (p_a.Y + p_b.Y) / 2+5);
                                }
                            }
                        }
                    }
                }
                //draw crane_upper_arcs
                if (StaticData.crane_upper_list.Count != 0)
                {
                    List<Crane> crane_upper_list = StaticData.crane_upper_list[StaticData.iter - 1];
                    foreach (Crane crane in crane_upper_list)
                    {
                        int colorindex = (crane.id - 1) % 5;
                        //pen_crane.Color = ColorList[colorindex];
                        foreach (Arc c_arc in crane.ArcList)
                        {
                            if (c_arc.S <= TTLength)
                            {
                                Point p_a = GetPoint(c_arc.T, c_arc.I, TTLength, NodeList, point_o, t_length, s_length);
                                Point p_b = GetPoint(c_arc.S, c_arc.J, TTLength, NodeList, point_o, t_length, s_length);
                                g.DrawLine(pen_crane, p_a, p_b);
                            }
                        }
                    }
                }
            }
            
            //draw simulation output_arcs
            if(StaticData.Sim_visulization_open==true)
            {
                //draw truck_sim_output_arcs
                foreach (Truck truck in StaticData.truck_sim_list)
                {
                    foreach (Arc t_arc in truck.sim_arc_list_for_drawing)
                    {
                        Point p_a = GetPoint(t_arc.T, t_arc.I, TTLength, NodeList, point_o, t_length, s_length);
                        Point p_b = GetPoint(t_arc.S, t_arc.J, TTLength, NodeList, point_o, t_length, s_length);
                        g.DrawLine(pen_truck, p_a, p_b);
                    }
                }
                //draw crane_sim_output_arcs
                foreach(Crane crane in StaticData.crane_sim_list)
                {
                    int colorindex = (crane.id - 1) % 5;
                    //pen_crane.Color = ColorList[colorindex];
                    foreach(Arc c_arc in crane.sim_arc_list_for_drawing)
                    {
                        Point p_a = GetPoint(c_arc.T, c_arc.I, TTLength, NodeList, point_o, t_length, s_length);
                        Point p_b = GetPoint(c_arc.S, c_arc.J, TTLength, NodeList, point_o, t_length, s_length);
                        g.DrawLine(pen_crane, p_a, p_b);
                    }
                }
            }
        }

        public Point GetPoint(int T, int NodeID,int TT,List<Node> NodeList,Point point_o, int t_length, int s_length)
        {
            Point point = new Point();
            int t_intervel = t_length / TT;
            int s_intervel = s_length / NodeList.Count;

            point.X = point_o.X + (T - 1) * t_intervel;
            int y_intervel_num = 0;
            for (int i = 0; i < NodeList.Count; i++)
            {
                if (NodeList[i].id == NodeID)
                {
                    y_intervel_num = i + 1;
                }
            }
            point.Y = point_o.Y - (y_intervel_num - 1) * s_intervel;
            return point;
        }

        public int GetID(int name_code, DataTable node_dt)
        {
            int ID = 0;
            for (int i = 0; i < node_dt.Rows.Count; i++)
            {
                DataRow row = node_dt.Rows[i];
                if (Convert.ToInt32(row[1]) == name_code)
                {
                    ID = Convert.ToInt32(row[0]);
                    break;
                }
            }
            return ID;
        }

        private void Iter_Cbox_TextChanged(object sender, EventArgs e)
        {
            StaticData.iter = Convert.ToInt32(Iter_Cbox.Text);
            this.Invalidate(true);
        }

        private void ADMM_Opt_Btn_Click(object sender, EventArgs e)
        {
            ADMM_OPT_Frm admm_opt_frm = new ADMM_OPT_Frm();
            admm_opt_frm.Show();
        }

        private void Generate_Input_File_Btn_Click(object sender, EventArgs e)
        {

            int arrival_gate_num = Convert.ToInt32(Arrival_Gate_Num_TBox.Text);
            int departure_gate_num = Convert.ToInt32(Departure_Gate_Num_TBox.Text);
            int container_bay_num = Convert.ToInt32(Container_Bay_Num_TBox.Text);
            int crane_num = Convert.ToInt32(Crane_Num_TBox.Text);
            int TT = Convert.ToInt32(TimeHorizion_TBox.Text);
            int Max_Capacity = 1000000000;

            List<Truck> TruckList = new List<Truck>();
            //8 bay: SAME WITH RCPSP
            TruckList.Add(new Truck(1, 0, 1, 1000, TT, 120, 121, 5));
            TruckList.Add(new Truck(2, 0, 5, 1000, TT, 160, 161, 5));
            TruckList.Add(new Truck(3, 0, 6, 1000, TT, 140, 142, 4));
            TruckList.Add(new Truck(4, 0, 8, 1000, TT, 170, 172, 4));
            TruckList.Add(new Truck(5, 0, 10, 1000, TT, 140, 141, 5));
            TruckList.Add(new Truck(6, 0, 10, 1000, TT, 160, 161, 5));
            TruckList.Add(new Truck(7, 0, 12, 1000, TT, 130, 132, 4));
            TruckList.Add(new Truck(8, 0, 15, 1000, TT, 170, 172, 4));
            TruckList.Add(new Truck(9, 0, 18, 1000, TT, 150, 151, 5));
            TruckList.Add(new Truck(10, 0, 18, 1000, TT, 120, 122, 4));
            TruckList.Add(new Truck(11, 0, 19, 1000, TT, 130, 132, 4));
            TruckList.Add(new Truck(12, 0, 20, 1000, TT, 180, 181, 5));
            TruckList.Add(new Truck(13, 0, 21, 1000, TT, 170, 172, 4));
            TruckList.Add(new Truck(14, 0, 22, 1000, TT, 130, 132, 4));
            TruckList.Add(new Truck(15, 0, 22, 1000, TT, 140, 141, 5));
            TruckList.Add(new Truck(16, 0, 22, 1000, TT, 160, 162, 4));
            TruckList.Add(new Truck(17, 0, 23, 1000, TT, 170, 171, 5));
            TruckList.Add(new Truck(18, 0, 23, 1000, TT, 110, 112, 4));
            TruckList.Add(new Truck(19, 0, 25, 1000, TT, 150, 152, 4));
            TruckList.Add(new Truck(20, 0, 26, 1000, TT, 130, 131, 5));
            TruckList.Add(new Truck(21, 0, 26, 1000, TT, 170, 172, 4));
            TruckList.Add(new Truck(22, 0, 28, 1000, TT, 160, 161, 5));
            TruckList.Add(new Truck(23, 0, 28, 1000, TT, 120, 122, 4));
            TruckList.Add(new Truck(24, 0, 29, 1000, TT, 160, 162, 4));
            TruckList.Add(new Truck(25, 0, 31, 1000, TT, 120, 121, 5));

            List<Crane> CraneList = new List<Crane>();
            ////2 cranes
            //CraneList.Add(new Crane(1, 110, 1, 110, TT));
            //CraneList.Add(new Crane(2, 150, 1, 150, TT));
            ////3 cranes
            //CraneList.Add(new Crane(1, 110, 1, 110, TT));
            //CraneList.Add(new Crane(2, 140, 1, 140, TT));
            //CraneList.Add(new Crane(3, 160, 1, 160, TT));
            //4 cranes
            CraneList.Add(new Crane(1, 110, 1, 110, TT));
            CraneList.Add(new Crane(2, 130, 1, 130, TT));
            CraneList.Add(new Crane(3, 150, 1, 150, TT));
            CraneList.Add(new Crane(4, 170, 1, 170, TT));

            #region build input nodes.csv input_truck_node.csv and input_crane_node.csv

            #region generate input_node.csv

            DataTable InputNode_gams_dt = new DataTable();
            DataColumn dc_name = new DataColumn("node_name");
            DataColumn dc_id = new DataColumn("node_id");
            //DataColumn dc_id = new DataColumn("");
            InputNode_gams_dt.Columns.Add(dc_name);
            InputNode_gams_dt.Columns.Add(dc_id);

            //Truck nodes
            DataRow row_truck0 = InputNode_gams_dt.NewRow();
            row_truck0[0] = "truck node";
            row_truck0[1] = "0";
            InputNode_gams_dt.Rows.Add(row_truck0);

            DataRow row_arrival0 = InputNode_gams_dt.NewRow();
            row_arrival0[0] = "gate node arrvial0";
            row_arrival0[1] = 10;
            InputNode_gams_dt.Rows.Add(row_arrival0);

            DataRow row_arrival1 = InputNode_gams_dt.NewRow();
            row_arrival1[0] = "gate node arrvial1";
            row_arrival1[1] = 11;
            InputNode_gams_dt.Rows.Add(row_arrival1);

            DataRow row_truck100 = InputNode_gams_dt.NewRow();
            row_truck100[0] = "truck node";
            row_truck100[1] = "100";
            InputNode_gams_dt.Rows.Add(row_truck100);

            DataRow row_departure0 = InputNode_gams_dt.NewRow();
            row_departure0[0] = "gate node departure0";
            row_departure0[1] = 20;
            InputNode_gams_dt.Rows.Add(row_departure0);

            DataRow row_departure1 = InputNode_gams_dt.NewRow();
            row_departure1[0] = "gate node departure1";
            row_departure1[1] = 21;
            InputNode_gams_dt.Rows.Add(row_departure1);

            DataRow row_truck1000 = InputNode_gams_dt.NewRow();
            row_truck1000[0] = "truck node";
            row_truck1000[1] = "1000";
            InputNode_gams_dt.Rows.Add(row_truck1000);

            //Crane nodes
            for (int i = 0; i < container_bay_num; i++)
            {
                DataRow row = InputNode_gams_dt.NewRow();
                row[0] = "crane node0";
                row[1] = 100 + 10 * (i + 1);
                InputNode_gams_dt.Rows.Add(row);
            }
            for (int i = 0; i < container_bay_num; i++)
            {
                DataRow row = InputNode_gams_dt.NewRow();
                row[0] = "crane node1";
                row[1] = 100 + 10 * (i + 1) + 1;
                InputNode_gams_dt.Rows.Add(row);
            }
            for (int i = 0; i < container_bay_num; i++)
            {
                DataRow row = InputNode_gams_dt.NewRow();
                row[0] = "crane node2";
                row[1] = 100 + 10 * (i + 1) + 2;
                InputNode_gams_dt.Rows.Add(row);
            }
            CSVFileHelper.SaveCSV(InputNode_gams_dt, "input nodes.csv");

            #endregion

            #region input_truck_node.csv

            DataTable InputTruckNode_dt = new DataTable();
            DataColumn dc_truck_id = new DataColumn("node_id");
            DataColumn dc_truck_namecode = new DataColumn("node_name_code");
            DataColumn dc_truck_description = new DataColumn("node_description");
            DataColumn dc_truck_waiting_cost = new DataColumn("waiting_cost ");
            InputTruckNode_dt.Columns.Add(dc_truck_id);
            InputTruckNode_dt.Columns.Add(dc_truck_namecode);
            InputTruckNode_dt.Columns.Add(dc_truck_description);
            InputTruckNode_dt.Columns.Add(dc_truck_waiting_cost);
            int t_node_id = 0;
            foreach (DataRow row in InputNode_gams_dt.Rows)
            {
                t_node_id++;
                DataRow r = InputTruckNode_dt.NewRow();
                r[0] = t_node_id; r[1] = row[1]; r[2] = row[0];
                //waiting cost
                int name_code = Convert.ToInt32(r[1]);
                if (name_code == 10 || name_code == 20 || name_code == 100)
                {
                    if (name_code == 100)
                    {
                        r[3] = 1;
                    }
                    else
                    {
                        r[3] = 1.1;
                    }
                }
                else
                {
                    r[3] = 0;
                }
                if (Convert.ToString(r[2]) == "crane node0")
                {
                    r[3] = 1;
                }
                InputTruckNode_dt.Rows.Add(r);
            }
            CSVFileHelper.SaveCSV(InputTruckNode_dt, "input_truck_node.csv");

            #endregion

            #region input_crane_node.csv

            DataTable InputCraneNode_dt = new DataTable();
            DataColumn dc_crane_id = new DataColumn("node_id");
            DataColumn dc_crane_namecode = new DataColumn("node_name_code");
            DataColumn dc_crane_description = new DataColumn("node_description");
            DataColumn dc_crane_waiting_cost = new DataColumn("waiting_cost ");

            InputCraneNode_dt.Columns.Add(dc_crane_id);
            InputCraneNode_dt.Columns.Add(dc_crane_namecode);
            InputCraneNode_dt.Columns.Add(dc_crane_description);
            InputCraneNode_dt.Columns.Add(dc_crane_waiting_cost);
            int c_node_id = 0;
            foreach (DataRow row in InputNode_gams_dt.Rows)
            {
                if (row[0].ToString() == "crane node0" || row[0].ToString() == "crane node1" || row[0].ToString() == "crane node2")
                {
                    c_node_id = GetID(Convert.ToInt32(row[1]), InputTruckNode_dt);
                    DataRow r = InputCraneNode_dt.NewRow();
                    r[0] = c_node_id; r[1] = row[1]; r[2] = row[0]; r[3] = 0;
                    InputCraneNode_dt.Rows.Add(r);
                }
            }
            CSVFileHelper.SaveCSV(InputCraneNode_dt, "input_crane_node.csv");
            #endregion

            #endregion

            #region read node.csv and generate NodeList 
            List<Node> NodeList = new List<Node>();
            List<Node> TruckNodeList = new List<Node>();
            List<Node> GateNodeArrvial0 = new List<Node>();
            List<Node> GateNodeArrvial1 = new List<Node>();
            List<Node> GateNodeDeparture0 = new List<Node>();
            List<Node> GateNodeDeparture1 = new List<Node>();
            List<Node> CraneNode0 = new List<Node>();
            List<Node> CraneNode1 = new List<Node>();//loading node
            List<Node> CraneNode2 = new List<Node>();//unloading node

            DataTable input_node_dt = new DataTable();
            input_node_dt = CSVFileHelper.OpenCSV("input nodes.csv");
            for (int i = 0; i < input_node_dt.Rows.Count; i++)
            {
                Node node = new Node();
                node.description = (string)input_node_dt.Rows[i][0];
                node.id = Convert.ToInt32(input_node_dt.Rows[i][1]);
                NodeList.Add(node);

                switch (node.description)
                {
                    case "truck node":
                        TruckNodeList.Add(node);
                        break;
                    case "gate node arrvial0":
                        GateNodeArrvial0.Add(node);
                        break;
                    case "gate node arrvial1":
                        GateNodeArrvial1.Add(node);
                        break;
                    case "gate node departure0":
                        GateNodeDeparture0.Add(node);
                        break;
                    case "gate node departure1":
                        GateNodeDeparture1.Add(node);
                        break;
                    case "crane node0":
                        CraneNode0.Add(node);
                        break;
                    case "crane node1":
                        CraneNode1.Add(node);
                        break;
                    case "crane node2":
                        CraneNode2.Add(node);
                        break;
                }
            }
            #endregion

            #region build input_truck_link.csv and input_crane_link.csv

            int link_id = 0;

            #region 1_Build input_truck_link
            List<Link> Truck_LinkList = new List<Link>();
            //stage 1: truck origin node to gate arrival node
            for (int i = 0; i < GateNodeArrvial0.Count; i++)
            {
                link_id++;
                Link link = new Link();
                link.name = "moving";
                link.type = 1;
                link.id = link_id;
                link.from_node_id = GetID(TruckNodeList[0].id, InputTruckNode_dt);
                link.to_node_id = GetID(GateNodeArrvial0[i].id, InputTruckNode_dt);
                link.travle_time = 0;
                link.cost = link.travle_time;
                link.capacity = Max_Capacity;
                Truck_LinkList.Add(link);
            }
            //stage 2: arrival gate transaction
            for (int i = 0; i < GateNodeArrvial0.Count; i++)
            {
                link_id++;
                Link link = new Link();
                link.name = "gate transaction";
                link.type = 2;
                link.id = link_id;
                link.from_node_id = GetID(GateNodeArrvial0[i].id, InputTruckNode_dt);
                link.to_node_id = GetID(GateNodeArrvial1[i].id, InputTruckNode_dt);
                link.travle_time = 3;
                link.cost = link.travle_time;
                link.capacity = arrival_gate_num;
                Truck_LinkList.Add(link);
            }
            //stage 3: gate transaction finish node to yard parking node
            for (int i = 0; i < GateNodeArrvial1.Count; i++)
            {
                link_id++;
                Link link = new Link();
                link.name = "moving";
                link.type = 1;
                link.id = link_id;
                link.from_node_id = GetID(GateNodeArrvial1[i].id, InputTruckNode_dt);
                link.to_node_id = GetID(TruckNodeList[1].id, InputTruckNode_dt);
                link.travle_time = 3;
                link.cost = link.travle_time;
                link.capacity = Max_Capacity;
                Truck_LinkList.Add(link);
            }
            //stage 4: yard parking node to crane nodes 3+i*1
            for (int i = 0; i < CraneNode0.Count; i++)
            {
                link_id++;
                Link link = new Link();
                link.name = "moving";
                link.type = 1;
                link.id = link_id;
                link.from_node_id = GetID(TruckNodeList[1].id, InputTruckNode_dt);
                link.to_node_id = GetID(CraneNode0[i].id, InputTruckNode_dt);
                link.travle_time = 3 + i;
                link.cost = link.travle_time;
                link.capacity = Max_Capacity;
                Truck_LinkList.Add(link);
            }
            //stage 5: crane handling
            for (int i = 0; i < CraneNode0.Count; i++)
            {
                //loading link
                link_id++;
                Link link1 = new Link();
                link1.name = "handling";
                link1.type = 3;
                link1.id = link_id;
                link1.from_node_id = GetID(CraneNode0[i].id, InputTruckNode_dt);
                link1.to_node_id = GetID(CraneNode1[i].id, InputTruckNode_dt);
                link1.travle_time = 5;
                link1.cost = link1.travle_time;
                link1.capacity = 1;
                Truck_LinkList.Add(link1);

                //unloading link
                link_id++;
                Link link2 = new Link();
                link2.name = "handling";
                link2.type = 3;
                link2.id = link_id;
                link2.from_node_id = GetID(CraneNode0[i].id, InputTruckNode_dt);
                link2.to_node_id = GetID(CraneNode2[i].id, InputTruckNode_dt);
                link2.travle_time = 4;
                link2.cost = link2.travle_time;
                link2.capacity = 1;
                Truck_LinkList.Add(link2);
            }
            //stage 6: crane to departure gate link //travel_time=3+(cranenum-1-i)
            for (int i = 0; i < CraneNode0.Count; i++)
            {
                //crane1-departure gate
                link_id++;
                Link link1 = new Link();
                link1.name = "moving";
                link1.type = 1;
                link1.id = link_id;
                link1.from_node_id = GetID(CraneNode1[i].id, InputTruckNode_dt);
                link1.to_node_id = GetID(GateNodeDeparture0[0].id, InputTruckNode_dt);
                link1.travle_time = 3 + (CraneNode0.Count() - 1 - i);
                link1.cost = link1.travle_time;
                link1.capacity = Max_Capacity;
                Truck_LinkList.Add(link1);

                //crane2-departure gate
                link_id++;
                Link link2 = new Link();
                link2.name = "moving";
                link2.type = 1;
                link2.id = link_id;
                link2.from_node_id = GetID(CraneNode2[i].id, InputTruckNode_dt);
                link2.to_node_id = GetID(GateNodeDeparture0[0].id, InputTruckNode_dt);
                link2.travle_time = 3 + (CraneNode0.Count() - 1 - i);
                link2.cost = link2.travle_time;
                link2.capacity = Max_Capacity;
                Truck_LinkList.Add(link2);
            }
            //stage 7: departue gate transaction
            for (int i = 0; i < GateNodeDeparture0.Count; i++)
            {
                link_id++;
                Link link = new Link();
                link.name = "gate transaction";
                link.type = 2;
                link.id = link_id;
                link.from_node_id = GetID(GateNodeDeparture0[i].id, InputTruckNode_dt);
                link.to_node_id = GetID(GateNodeDeparture1[i].id, InputTruckNode_dt);
                link.travle_time = 3;
                link.cost = link.travle_time;
                link.capacity = departure_gate_num;
                Truck_LinkList.Add(link);
            }
            //stage 8: truck departure link
            for (int i = 0; i < GateNodeDeparture1.Count; i++)
            {
                link_id++;
                Link link = new Link();
                link.name = "moving";
                link.type = 1;
                link.id = link_id;
                link.from_node_id = GetID(GateNodeDeparture1[0].id, InputTruckNode_dt);
                link.to_node_id = GetID(TruckNodeList[2].id, InputTruckNode_dt);
                link.travle_time = 0;
                link.cost = link.travle_time;
                link.capacity = Max_Capacity;
                Truck_LinkList.Add(link);
            }
            #endregion

            #region 2_build input_crane_link

            link_id = 0;
            List<Link> Crane_LinkList = new List<Link>();
            //stage 1:moving link
            for (int i = 0; i < CraneNode0.Count - 1; i++)
            {
                //down to up
                link_id++;
                Link link1 = new Link();
                link1.name = "moving";
                link1.type = 1;
                link1.id = link_id;
                link1.from_node_id = GetID(CraneNode0[i].id, InputCraneNode_dt);
                link1.to_node_id = GetID(CraneNode0[i + 1].id, InputCraneNode_dt);
                link1.travle_time = 1;
                link1.cost = 0.5;
                link1.capacity = 1;
                Crane_LinkList.Add(link1);

                //up to down
                link_id++;
                Link link2 = new Link();
                link2.name = "moving";
                link2.type = 1;
                link2.id = link_id;
                link2.from_node_id = GetID(CraneNode0[i + 1].id, InputCraneNode_dt);
                link2.to_node_id = GetID(CraneNode0[i].id, InputCraneNode_dt);
                link2.travle_time = 1;
                link2.cost = 0.5;
                link2.capacity = 1;
                Crane_LinkList.Add(link2);
            }
            //stage 2:handling
            for (int i = 0; i < CraneNode0.Count; i++)
            {
                //loading link
                link_id++;
                Link link1 = new Link();
                link1.name = "handling";
                link1.type = 2;
                link1.id = link_id;
                link1.from_node_id = GetID(CraneNode0[i].id, InputCraneNode_dt);
                link1.to_node_id = GetID(CraneNode1[i].id, InputCraneNode_dt);
                link1.travle_time = 5;
                link1.cost = link1.travle_time;
                link1.capacity = 1;
                Crane_LinkList.Add(link1);

                //unloading link
                link_id++;
                Link link2 = new Link();
                link2.name = "handling";
                link2.type = 2;
                link2.id = link_id;
                link2.from_node_id = GetID(CraneNode0[i].id, InputCraneNode_dt);
                link2.to_node_id = GetID(CraneNode2[i].id, InputCraneNode_dt);
                link2.travle_time = 4;
                link2.cost = link2.travle_time;
                link2.capacity = 1;
                Crane_LinkList.Add(link2);
            }
            //stage 3: recovery
            for (int i = 0; i < CraneNode0.Count; i++)
            {
                link_id++;
                Link link1 = new Link();
                link1.name = "recovery";
                link1.type = 3;
                link1.id = link_id;
                link1.from_node_id = GetID(CraneNode1[i].id, InputCraneNode_dt);
                link1.to_node_id = GetID(CraneNode0[i].id, InputCraneNode_dt);
                link1.travle_time = 1;
                link1.cost = 0;
                link1.capacity = 1;
                Crane_LinkList.Add(link1);

                link_id++;
                Link link2 = new Link();
                link2.name = "recovery";
                link2.type = 3;
                link2.id = link_id;
                link2.from_node_id = GetID(CraneNode2[i].id, InputCraneNode_dt);
                link2.to_node_id = GetID(CraneNode0[i].id, InputCraneNode_dt);
                link2.travle_time = 1;
                link2.cost = 0;
                link2.capacity = 1;
                Crane_LinkList.Add(link2);
            }

            #endregion

            //Link Datatable
            DataTable input_link_dt = new DataTable();
            DataColumn dc1 = new DataColumn("id");
            DataColumn dc2 = new DataColumn("name");
            DataColumn dc3 = new DataColumn("type");
            DataColumn dc4 = new DataColumn("from_node_id");
            DataColumn dc5 = new DataColumn("to_node_id");
            DataColumn dc6 = new DataColumn("travel time");
            DataColumn dc7 = new DataColumn("cost");
            DataColumn dc8 = new DataColumn("capacity");
            input_link_dt.Columns.Add(dc1);
            input_link_dt.Columns.Add(dc2);
            input_link_dt.Columns.Add(dc3);
            input_link_dt.Columns.Add(dc4);
            input_link_dt.Columns.Add(dc5);
            input_link_dt.Columns.Add(dc6);
            input_link_dt.Columns.Add(dc7);
            input_link_dt.Columns.Add(dc8);

            //generate input_truck_link.csv
            foreach (Link link in Truck_LinkList)
            {
                DataRow row = input_link_dt.NewRow();
                row[0] = link.id;
                row[1] = link.name;
                row[2] = link.type;
                row[3] = link.from_node_id;
                row[4] = link.to_node_id;
                row[5] = link.travle_time;
                row[6] = link.cost;
                row[7] = link.capacity;
                input_link_dt.Rows.Add(row);
            }
            CSVFileHelper.SaveCSV(input_link_dt, "input_truck_link.csv");
            //generate input_crane_link.csv
            input_link_dt.Rows.Clear();
            foreach (Link link in Crane_LinkList)
            {
                DataRow row = input_link_dt.NewRow();
                row[0] = link.id;
                row[1] = link.name;
                row[2] = link.type;
                row[3] = link.from_node_id;
                row[4] = link.to_node_id;
                row[5] = link.travle_time;
                row[6] = link.cost;
                row[7] = link.capacity;
                input_link_dt.Rows.Add(row);
            }
            CSVFileHelper.SaveCSV(input_link_dt, "input_crane_link.csv");

            #endregion

            #region build input_truck.csv and input_crane.csv

            #region input_truck.csv

            DataTable input_truck_dt = new DataTable();
            DataColumn t_dc0 = new DataColumn("truck_id");
            DataColumn t_dc1 = new DataColumn("type");
            DataColumn t_dc2 = new DataColumn("from_node");
            DataColumn t_dc3 = new DataColumn("to_node");
            DataColumn t_dc4 = new DataColumn("container_bay_node");
            DataColumn t_dc5 = new DataColumn("ContainerBay_stateID");
            DataColumn t_dc6 = new DataColumn("arrival_time");
            DataColumn t_dc7 = new DataColumn("departure_time");
            DataColumn t_dc8 = new DataColumn("available_node_list");
            input_truck_dt.Columns.Add(t_dc0);
            input_truck_dt.Columns.Add(t_dc1);
            input_truck_dt.Columns.Add(t_dc2);
            input_truck_dt.Columns.Add(t_dc3);
            input_truck_dt.Columns.Add(t_dc4);
            input_truck_dt.Columns.Add(t_dc5);
            input_truck_dt.Columns.Add(t_dc6);
            input_truck_dt.Columns.Add(t_dc7);
            input_truck_dt.Columns.Add(t_dc8);

            foreach (Truck truck in TruckList)
            {
                DataRow row = input_truck_dt.NewRow();
                row[0] = truck.id;
                row[1] = truck.Type;
                row[2] = GetID(truck.OriginI, InputTruckNode_dt);
                row[3] = GetID(truck.DestinationI, InputTruckNode_dt);
                row[4] = GetID(truck.ContainerBayID, InputTruckNode_dt);
                row[5] = GetID(truck.ContainerBay_stateID, InputTruckNode_dt);
                row[6] = truck.OriginT;
                row[7] = truck.DestinationT;
                List<int> available_node_list = new List<int>();
                available_node_list.Add(GetID(0, InputTruckNode_dt));
                available_node_list.Add(GetID(10, InputTruckNode_dt));
                available_node_list.Add(GetID(11, InputTruckNode_dt));
                available_node_list.Add(GetID(100, InputTruckNode_dt));
                available_node_list.Add(GetID(truck.ContainerBayID, InputTruckNode_dt));
                available_node_list.Add(GetID(truck.ContainerBay_stateID, InputTruckNode_dt));
                available_node_list.Add(GetID(20, InputTruckNode_dt));
                available_node_list.Add(GetID(21, InputTruckNode_dt));
                available_node_list.Add(GetID(1000, InputTruckNode_dt));
                string available_node_list_str = available_node_list[0].ToString();
                for (int i = 1; i < available_node_list.Count; i++)
                {
                    available_node_list_str += ";" + available_node_list[i];
                }
                row[8] = available_node_list_str;
                input_truck_dt.Rows.Add(row);
            }
            CSVFileHelper.SaveCSV(input_truck_dt, "input_truck.csv");

            #endregion

            //input_crane.csv
            DataTable input_crane_dt = new DataTable();
            DataColumn c_dc0 = new DataColumn("crane_id");
            DataColumn c_dc1 = new DataColumn("from_node");
            DataColumn c_dc2 = new DataColumn("to_node");
            DataColumn c_dc3 = new DataColumn("begin_time");
            DataColumn c_dc4 = new DataColumn("end_time");
            DataColumn c_dc5 = new DataColumn("available_node_list");
            input_crane_dt.Columns.Add(c_dc0);
            input_crane_dt.Columns.Add(c_dc1);
            input_crane_dt.Columns.Add(c_dc2);
            input_crane_dt.Columns.Add(c_dc3);
            input_crane_dt.Columns.Add(c_dc4);
            input_crane_dt.Columns.Add(c_dc5);

            foreach (Crane crane in CraneList)
            {
                DataRow row = input_crane_dt.NewRow();
                row[0] = crane.id;
                row[1] = GetID(crane.OriginI, InputCraneNode_dt);
                row[2] = GetID(crane.DestinationI, InputCraneNode_dt);
                row[3] = crane.OriginT;
                row[4] = crane.DestinationT;
                List<int> available_node_list = new List<int>();

                //Total flexible crane working zone
                int available_crane_bay_index_begin = crane.id - 1;
                int available_crane_bay_index_end = container_bay_num - (crane_num - crane.id) - 1;

                //8 bay
                ////No-crossing fixed working zone
                ////2 crane
                //int available_crane_bay_index_begin = 0;
                //int available_crane_bay_index_end = 0;
                //if (crane.id == 1)
                //{
                //    available_crane_bay_index_begin = 0;
                //    available_crane_bay_index_end = 3;
                //}
                //if (crane.id == 2)
                //{
                //    available_crane_bay_index_begin = 4;
                //    available_crane_bay_index_end = 7;
                //}

                ////3 crane
                //int available_crane_bay_index_begin = 0;
                //int available_crane_bay_index_end = 0;
                //if (crane.id == 1)
                //{
                //    available_crane_bay_index_begin = 0;
                //    available_crane_bay_index_end = 2;
                //}
                //if (crane.id == 2)
                //{
                //    available_crane_bay_index_begin = 3;
                //    available_crane_bay_index_end = 4;
                //}
                //if (crane.id == 3)
                //{
                //    available_crane_bay_index_begin = 5;
                //    available_crane_bay_index_end = 7;
                //}

                ////4 crane
                //int available_crane_bay_index_begin = 0;
                //int available_crane_bay_index_end = 0;
                //if (crane.id == 1)
                //{
                //    available_crane_bay_index_begin = 0;
                //    available_crane_bay_index_end = 1;
                //}
                //if (crane.id == 2)
                //{
                //    available_crane_bay_index_begin = 2;
                //    available_crane_bay_index_end = 3;
                //}
                //if (crane.id == 3)
                //{
                //    available_crane_bay_index_begin = 4;
                //    available_crane_bay_index_end = 5;
                //}
                //if (crane.id == 4)
                //{
                //    available_crane_bay_index_begin = 6;
                //    available_crane_bay_index_end = 7;
                //}

                for (int i = available_crane_bay_index_begin; i <= available_crane_bay_index_end; i++)
                {
                    available_node_list.Add(GetID(CraneNode0[i].id, InputCraneNode_dt));
                    available_node_list.Add(GetID(CraneNode1[i].id, InputCraneNode_dt));
                    available_node_list.Add(GetID(CraneNode2[i].id, InputCraneNode_dt));
                }

                string available_node_list_str = available_node_list[0].ToString();
                for (int i = 1; i < available_node_list.Count; i++)
                {
                    available_node_list_str += ";" + available_node_list[i];
                }
                row[5] = available_node_list_str;
                input_crane_dt.Rows.Add(row);
            }
            CSVFileHelper.SaveCSV(input_crane_dt, "input_crane.csv");

            #endregion

            MessageBox.Show("Input Files have been generated!");
        }

        private void Visualization_Btn_Click(object sender, EventArgs e)
        {
            StaticData.ADMM_visulization_open = true;
            StaticData.GAMS_visulization_open = false;
            StaticData.Sim_visulization_open = false;
            TTLength = Convert.ToInt32(TimeHorizion_TBox.Text);
            for (int i = 0; i < StaticData.truck_upper_list.Count; i++)
            {
                Iter_Cbox.Items.Add((i + 1));
            }
            Iter_Cbox.Text = StaticData.truck_upper_list.Count.ToString();
            StaticData.iter = 1;
            this.Invalidate(true);
        }
    }
}
