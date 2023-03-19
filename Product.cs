using System.Runtime.Serialization;
using System.Collections.Generic;

namespace BookingService
{
    [DataContract]
    public class Product
    {
        [DataMember]
        public string imageSrc { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public string description { get; set; }

        [DataMember]
        public decimal price { get; set; }

        [DataMember]
        public string code { get; set; }

        [DataMember]
        public List<Product> services { get; set; }
    }
}
