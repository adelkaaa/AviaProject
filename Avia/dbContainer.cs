using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;

namespace Avia
{
    internal static class dbContainer
    {
        public static string getAdminString()
        {
            return ConfigurationManager.ConnectionStrings["adminConnectionString"].ConnectionString; ;
        }
    }
}
