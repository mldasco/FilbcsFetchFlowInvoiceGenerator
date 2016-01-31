
using System.Runtime.Serialization;

namespace FilbcsFetchFlowXmlGenerator
{
    [DataContract]
    public class DocItem
    {
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public decimal ItemPrice { get; set; }
        [DataMember]
        public int ItemQuantity { get; set; }
        [DataMember]
        public string ItemId { get { return string.Empty; }
            set { value = value; }
        }
        [DataMember]
        public int? TaxId1 { get { return null; }
            set { value = value; }
        }
        [DataMember]
        public int? TaxId2 { get { return null; }
            set { value = value; }
        }
        [DataMember]
        public int DiscountType { get { return 0; }
            set { value = value; }
        }
        [DataMember]
        public int DiscountValue { get { return 0; }
            set { value = value; }
        }


    }
}
