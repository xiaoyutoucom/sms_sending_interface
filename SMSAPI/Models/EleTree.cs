using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMSAPI
{
    public class EleTree
    {
        public string id { get; set; }

        public string label { get; set; }

        public List<EleTree> children { get; set; }
    }
}