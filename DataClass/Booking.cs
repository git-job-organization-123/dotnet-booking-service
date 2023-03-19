using System.Runtime.Serialization;
using System.Collections.Generic;

namespace BookingService
{
    [DataContract]
    public class Booking
    {
        [DataMember]
        public List<Product> products;

        [DataMember]
        public Customer customer;

        // [DataMember]
        // public List<Customer> passengers;
    }
}