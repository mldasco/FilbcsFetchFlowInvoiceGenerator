using System.Collections.Generic;
using System.Runtime.Serialization;


namespace FilbcsFetchFlowXmlGenerator
{
    [DataContract]
    public class Invoice
    {
        [DataMember]
        public string ClientId { get; set; }

        [DataMember]
        public string ReferenceNumber
        {
            get { return null; }
            set { value = value; }
        }

        [DataMember]
        public int Discount
        {
            get { return 0; }
            set { value = value; }
        }

        [DataMember]
        public int Due { get; set; }

        [DataMember]
        public string ClientNotes {
            get { return string.Empty; }
            set { value = value; }
        }

        [DataMember]
        public string InternalNotes
        {
            get { return string.Empty; }
            set { value = value; }
        }

        [DataMember]
        public List<DocItem> DocItems { get; set; }

        [DataMember]
        public string Uri { get; set; }

        [DataMember]
        public string Email { get; set; }
    }
}
    


