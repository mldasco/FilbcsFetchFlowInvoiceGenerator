using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilbcsFetchFlowXmlGenerator
{
    public class DocItem
    {
        public string ItemDescription { get; set; }
        public decimal ItemPrice { get; set; }
        public int ItemQuantity { get; set; }
        public string ItemID { get { return string.Empty; } }
    }
}
