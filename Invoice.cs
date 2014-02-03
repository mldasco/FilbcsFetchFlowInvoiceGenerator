using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilbcsFetchFlowXmlGenerator
{
    public class Invoice
    {
        public string ClientId { get; set; }
        public int Due { get; set; }
        public List<DocItem> DocItems { get; set; }
        public string Send { get; set; }
    }
}
    


