using System;
using System.Linq;
using System.Net;
using System.Text;
using FilbcsFetchFlowXmlGenerator;
using LinqToExcel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


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
                int dueInDays; 
                Console.Write("Enter number of days from now before invoices are due:");

                while (!int.TryParse(Console.ReadLine(), out dueInDays))
                {
                    Console.Write("Enter number of days from now before invoices are due:");
                }    

                var excel = new ExcelQueryFactory("FilBCS Monthly Invoicing.xlsx");
                var invoiceRows = from row in excel.Worksheet(sheetName)
                                  select new InvoiceRow
                                  {
                                      ClientId = row["ClientId"].Cast<string>(),
                                      ClientName = row["First Name"].Cast<string>() + " " + row["Surname"].Cast<string>(),
                                      ClientEmail = row["Invoice Email"].Cast<string>(),
                                      Due = row["Due"].Cast<int>(),
                                      DocItem = new DocItem
                                      {
                                          ItemDescription = row["ItemDescription"].Cast<string>(),
                                          ItemQuantity = row["ItemQuantity"].Cast<int>(),
                                          ItemPrice = row["ItemPrice"].Cast<decimal>()
                                      }
                                  };

                
                var invoices = from rows in invoiceRows.ToList()
                               group rows.DocItem by new { rows.ClientId, rows.ClientEmail } 
                                into g
                                where g.Key != null 
                                select new Invoice
                                {
                                    ClientId = g.Key.ClientId,
                                    Email = g.Key.ClientEmail,
                                    Due = dueInDays, 
                                    DocItems = g.ToList(),
                                };

                var baseUrl = "https://www.BILLIVING.com/API2";
                var invoiceUrl = baseUrl + "/v1/invoices";
                var apiKey = "8e715d34-3c41-4c45-8ee7-a440ee48525f";
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(apiKey + ":" + password));


                foreach (var invoice in invoices.Where(i=>i.ClientId!=null))
                {
                    string clientName = invoiceRows.First(ir => ir.ClientId == invoice.ClientId).ClientName;

                    var invoiceJson = JsonConvert.SerializeObject(invoice);

                    string result;
                    using (var client = new WebClient())
                    {
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                        client.Headers.Add(HttpRequestHeader.Accept, "*/*");
                        client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + credentials);
                        byte[] bytes = Encoding.UTF8.GetBytes(invoiceJson);
                        
                        result = client.UploadString(invoiceUrl, Encoding.ASCII.GetString(bytes).Replace("?"," "));
                        Console.WriteLine("Invoice for {0} - created successfully", clientName);
                    }

                    dynamic invoiceResponse = JObject.Parse(result);
              
                    using (var client = new WebClient())
                    {
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                        client.Headers.Add(HttpRequestHeader.Accept, "*/*");
                        client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + credentials);
                        var emailTo = new EmailTo() {To = invoice.Email};
                        var emailJson = JsonConvert.SerializeObject(emailTo);
                        byte[] bytes = Encoding.Default.GetBytes(emailJson);
                        client.UploadString(baseUrl+ invoiceResponse.Uri+"/send", Encoding.UTF8.GetString(bytes));
                        Console.WriteLine("Invoice for {0} - emailed successfully", clientName);
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
