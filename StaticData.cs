using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateInputdata
{
    public class StaticData
    {
        public static int iter = 0;
        public static List<List<Truck>> truck_upper_list = new List<List<Truck>>();
        public static List<List<Crane>> crane_upper_list = new List<List<Crane>>();
        public static List<double> prime_cost_list = new List<double>();

        public static List<Truck> truck_sim_list = new List<Truck>();
        public static List<Crane> crane_sim_list = new List<Crane>();
        //visualization control
        public static bool GAMS_visulization_open = false;
        public static bool ADMM_visulization_open = false;
        public static bool Sim_visulization_open = false;
    }
}
