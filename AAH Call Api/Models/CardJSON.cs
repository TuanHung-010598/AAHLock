using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAH_Call_Api.Models
{
    public class CardJSON
    {
        public CardJSON()
        {
            var doorOp = new CardJSONDoorOperations();
            doorOp.doors = new List<string> { "101" };
            doorOp.operation = "guest";

            doorOperations = new List<CardJSONDoorOperations> { doorOp };
            expireTime = DateTime.Now.AddDays(1).ToString("yyyyMMddTHHmm");
            format = "rfid48";
            label = "%ROOMRANGE%:%UUID%:%CARDNUM%";
            description = "Hotel California";
        }

        public string expireTime { get; set; }
        public string format { get; set; }
        public string endPointID { get; set; }
        public string label { get; set; }
        public string description { get; set; }

        public IList<CardJSONDoorOperations> doorOperations { get; set; }
    }
}
