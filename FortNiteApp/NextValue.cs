using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortNiteApp
{
    class NextValue
    {
        public string Next(string str, int ind)
        {
            int e_indx;
            string disVal = "\"displayValue\":\"";
            string newstr;


            ind = str.IndexOf(disVal, ind) + disVal.Length;
            e_indx = str.IndexOf("\"", ind);
            newstr = str.Substring(ind, e_indx - ind);

            return newstr;
        }
    }
}
