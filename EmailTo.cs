using System.Collections.Generic;
using System.Runtime.Serialization;


namespace FilbcsFetchFlowXmlGenerator
{
    [DataContract]
    public class EmailTo
    {
        [DataMember]
        public string To { get; set; }
    }
}
    


