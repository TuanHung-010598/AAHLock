using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAH_Call_Api.Models
{
    public class InitialValues
    {
        public InitialValues()
        {

            // Values you need to provide based on your environment

            VisionlineIP = "https://127.0.0.1"; //Example : https://127.0.0.1
            EndpointId = "JILLY44"; //Example : JILLY44
            KeyExpireTime = DateTime.Now.AddDays(1).ToString("yyyyMMddTHHmm"); //Default 1 day (24h) valid key, modify if required

            // Static values, should not be modified
            ContentType = "application/json; charset=utf-8";
            ApiSession = "/api/v1/sessions";
            ApiCard = "/api/v1/cards?action=mobileAccess&override=true";
        }

        public string VisionlineIP { get; set; }
        public string ApiSession { get; set; }
        public string ApiCard { get; set; }
        public string ContentType { get; set; }
        public string KeyExpireTime { get; set; }
        public string EndpointId { get; set; }
    }
}
