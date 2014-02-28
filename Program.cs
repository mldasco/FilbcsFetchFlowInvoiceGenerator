using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;
using FilbcsFetchFlowXmlGenerator;
using LinqToExcel;


namespace FilbcsFetchFlowInvoiceGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Enter the sheet name:");
                var sheetName = Console.ReadLine();
                Console.Write("Enter the fetch flow account password:");
                var password = Console.ReadLine();
                var sendInvoicesViaEmail = PromptToProceed("Send invoices via email?");
                
                var excel = new ExcelQueryFactory("FilBCS Monthly Invoicing.xlsx");
                var invoiceRows = from row in excel.Worksheet(sheetName)
                                  select new InvoiceRow
                                  {
                                      ClientId = row["ClientId"].Cast<string>(),
                                      ClientName = row["First Name"].Cast<string>() + " " + row["Surname"].Cast<string>(),
                                      Due = row["Due"].Cast<int>(),
                                      DocItem = new DocItem
                                      {
                                          ItemDescription = row["ItemDescription"].Cast<string>(),
                                          ItemQuantity = row["ItemQuantity"].Cast<int>(),
                                          ItemPrice = row["ItemPrice"].Cast<decimal>()
                                      }
                                  };

                var invoices = from rows in invoiceRows.ToList()
                               group rows.DocItem by rows.ClientId
                                into g
                                where g.Key != null 
                                select new Invoice
                                {
                                    ClientId = g.Key, 
                                    Due = 15, 
                                    Send = sendInvoicesViaEmail ? "1":"0", 
                                    DocItems = g.ToList()
                                };
                               


                foreach (var invoice in invoices)
                {

                    string clientName = invoiceRows.First(ir => ir.ClientId == invoice.ClientId).ClientName;
                    var stringwriter = new System.IO.StringWriter();
                    var serializer = new XmlSerializer(invoice.GetType());
                    serializer.Serialize(stringwriter, invoice);
                    var invoiceXml = stringwriter.ToString();
                    XDocument invoiceFullXml = XDocument.Parse(invoiceXml);
                    string invoiceInnerXml = invoiceFullXml.FirstNode.ToString();
                    invoiceInnerXml = invoiceInnerXml.Replace("</Invoice>", "");
                    invoiceInnerXml =
                        invoiceInnerXml.Replace(
                            "<Invoice xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">",
                            "");

                    string xmlRequest =
                        @"<?xml version=""1.0"" encoding=""utf-8"" ?>" +
                        String.Format(
                            "    <Request authtoken=\"8e715d34-3c41-4c45-8ee7-a440ee48525f\" password=\"{0}\">",
                            password) +
                        @"    <Action type=""invoice"" method=""create"">" +
                        invoiceInnerXml +
                        @"    </Action>" +
                        @"</Request>";

                    var requestXml = XDocument.Parse(xmlRequest);



                    // build XML request 
                    var httpRequest =
                        HttpWebRequest.Create("https://www.fetchflow.com/API/XMLRequest/ ");
                    httpRequest.Method = "POST";
                    httpRequest.ContentType = "text/xml";

                    // set appropriate headers

                    using (var requestStream = httpRequest.GetRequestStream())
                    {
                        requestXml.Save(requestStream);
                    }

                    using (var response = (HttpWebResponse)httpRequest.GetResponse())
                    using (var responseStream = response.GetResponseStream())
                    {
                        // may want to check response.StatusCode to
                        // see if the request was successful

                        var responseDoc = XDocument.Load(responseStream);
                        string status = responseDoc.Root
                            .Elements()
                            .First(node => node.Name.LocalName == "Action")
                            .Attribute("status")
                            .Value;
                        if (status != "success")
                        {
                            Console.WriteLine("Invoice for {0} - failed with status: {1}", clientName, status);
                            var proceed = PromptToProceed("Do you want to proceed generating other invoices?");

                            if (!proceed)
                            {
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invoice for {0} - created successfully", clientName);
                        }
                    }
                }

                Console.WriteLine("Invoice generation complete...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Invoice generation failed with general exception {0}", e);
                Console.WriteLine("Abandoning process...");
                Console.ReadLine();
            }
        }

        static bool PromptToProceed(string prompt)
        {
            Console.Write("{0} [y]es [n]o:", prompt);
            var proceed = Console.ReadLine();
            while (!proceed.Equals("y", StringComparison.OrdinalIgnoreCase) &&
                   !proceed.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write("{0} [y]es [n]o:", prompt);
                proceed = Console.ReadLine();
            }
            if (proceed.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (proceed.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return false;
        }
    }
}
