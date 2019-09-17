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
            SolidBrush BrushPoint = new SolidBrush(Color.Black);
            Font F1 = new Font("Times New Roman", 14f, FontStyle.Bold);
            Font F2 = new Font("Times New Roman", 12f, FontStyle.Bold);
            Font F3 = new Font("Times New Roman", 12f, FontStyle.Bold);

            time_intervel_pix = 10;
            int total_time_length = time_intervel_pix * TTLength;

            int height = pictureBox1.Height;
            pictureBox1.Size = new Size(total_time_length + 200, height);

            Point point_o = new Point(100, 500);
            Point point_x = new Point(total_time_length, 500);
            Point point_y = new Point(100, 20);
            int s_length = point_o.Y - point_y.Y;
            int t_length = point_x.X - point_o.X;
            g.DrawLine(pen, point_o, point_x);
            g.DrawString("Time", F1, BrushPoint,(point_o.X+point_x.X)/2,point_o.Y+20);
            g.DrawLine(pen, point_o, point_y);
            g.DrawString("Space", F1, BrushPoint, point_o.X - 60, point_y.Y-15);

            List<Node> NodeList = new List<Node>();
            DataTable input_node_dt = new DataTable();
            List<Node> TruckNodeList = new List<Node>();
            List<Node> GateNodeArrvial0 = new List<Node>();
            List<Node> GateNodeArrvial1 = new List<Node>();
            List<Node> GateNodeDeparture0 = new List<Node>();
            List<Node> GateNodeDeparture1 = new List<Node>();
            List<Node> CraneNode0 = new List<Node>();
            List<Node> CraneNode1 = new List<Node>();
            List<Node> CraneNode2 = new List<Node>();
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
            int s_intervel = s_length / NodeList.Count;
            for (int i = 0; i < NodeList.Count; i++)
            {
                Point p_a = GetPoint(1, NodeList[i].id, TTLength, NodeList, point_o, t_length, s_length);
                Point p_b = new Point(point_x.X, p_a.Y);
                g.DrawString(NodeList[i].id.ToString(), F2, BrushPoint, p_a.X-40, p_a.Y-5);
                //g.DrawString(NodeList[i].id.ToString(), F2, BrushPoint, point_x.X +10, p_a.Y - 5);
                g.DrawLine(pen_dashed, p_a, p_b);
            }

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

        private void Visualization_Btn_Click(object sender, EventArgs e)
        {
            StaticData.ADMM_visulization_open = true;
            StaticData.GAMS_visulization_open = false;
            StaticData.Sim_visulization_open = false;
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
