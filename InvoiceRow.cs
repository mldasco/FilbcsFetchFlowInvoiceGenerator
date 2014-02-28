
using FilbcsFetchFlowXmlGenerator;

namespace FilbcsFetchFlowInvoiceGenerator
{
    public class InvoiceRow
    {
        public string ClientId { get; set; }
        public int Due { get; set; }
        public DocItem DocItem { get; set; }
        public string ClientName { get; set; }


    }
}
