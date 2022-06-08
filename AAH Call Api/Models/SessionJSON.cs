using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAH_Call_Api.Models
{
    public class SessionJSON
    {
        public SessionJSON()
        {
            username = "sym";
            password = "sym";
        }

        public string username { get; set; }
        public string password { get; set; }
    }
}
