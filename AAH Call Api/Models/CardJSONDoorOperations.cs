﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAH_Call_Api.Models
{
    public class CardJSONDoorOperations
    {
        public string operation { get; set; }
        public IList<string> doors { get; set; }
    }
}
