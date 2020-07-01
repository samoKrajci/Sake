using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace protocolLibrary
{
    public static class Utils
    {
        public static List<int> DeserializeToInt(string s)
        {
            string[] cells = s.Split(' ');
            List<int> cellsInt = new List<int>();
            foreach (string cell in cells)
            {
                if (cell == "")
                    continue;
                try {
                    cellsInt.Add(Convert.ToInt32(cell));
                }
                catch (FormatException e)
                {
                    Debug.WriteLine(String.Format("Wrong response format. Exception: {0}", e));
                }
            }
            return cellsInt;
        }
    }
}
