using System.Runtime.Serialization;

namespace BookingService
{
    [DataContract]
    public class BookingResponse
    {
        [DataMember]
        public string bookingUrl;

        [DataMember]
        public decimal totalPrice;
    }
}
