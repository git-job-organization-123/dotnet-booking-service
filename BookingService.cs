using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Net;
// using System.Data;
// using System.Data.SqlClient;

namespace BookingService
{
    [ServiceContract]
    public interface IBookingService
    {
        [OperationContract]
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*")]
        void HandleHttpOptionsRequest();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "api/accommodations", ResponseFormat = WebMessageFormat.Json)]
        List<Product> GetAccommodations();

        [OperationContract]
        [WebInvoke(UriTemplate = "api/booking", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        BookingResponse CreateBooking(Booking booking);
    }

    // [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, Name = "BookingService")]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, Name = "BookingService")] // Allow constructor calling
    public class BookingService : IBookingService
    {
        private static List<Product> _products_db;

        // Database simulation
        public BookingService()
        {
            Random random = new Random();

            // Services
            _products_db = new List<Product>()
            {
                new Product { name = "Spa Treatment", type = "Service", price = 50, code = GenerateProductCode() },
                new Product { name = "Room Service", type = "Service", price = 20, code = GenerateProductCode() },
                new Product { name = "Airport Transfer", type = "Service", price = 30, code = GenerateProductCode() },
                new Product { name = "Personal Shopper", type = "Service", price = 60, code = GenerateProductCode() },
                new Product { name = "Laundry and Dry Cleaning", type = "Service", price = 25, code = GenerateProductCode() },
                new Product { name = "Fitness Center Access", type = "Service", price = 10, code = GenerateProductCode() },
                new Product { name = "Sightseeing Tour", type = "Service", price = 40, code = GenerateProductCode() },
                new Product { name = "Golf Course Access", type = "Service", price = 70, code = GenerateProductCode() },
                new Product { name = "Pet Care", type = "Service", price = 15, code = GenerateProductCode() },
                new Product { name = "Baby-sitting Service", type = "Service", price = 35, code = GenerateProductCode() }
            };

            // Accommodations
            for (int i = 0; i < 11; i++)
            {
                _products_db.Add(new Product()
                {
                    imageSrc = "https://via.placeholder.com/150x150",
                    name = "Name " + i,
                    type = "Accommodation",
                    description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                    price = Math.Floor((decimal)(random.NextDouble() * 100)) + 1,
                    code = GenerateProductCode(),
                    services = GetServices()
                });
            }
        }

        private static int index = 0;

        private static string GenerateProductCode()
        {
            ++index;
            return "P" + index.ToString("D4");
        }

        // OPTIONS
        public void HandleHttpOptionsRequest() {}

        private List<Product> GetServices()
        {
            return _products_db.Where(p => p.type == "Service").ToList();
        }

        public List<Product> GetAccommodations()
        {
            return _products_db.Where(p => p.type == "Accommodation").ToList();
        }

        public BookingResponse CreateBooking(Booking booking)
        {
            ValidateCustomer(booking.customer);
 
            return new BookingResponse()
            {
                bookingUrl = "bookingUrl",
                totalPrice = getTotalPrice(booking.products)
            };
        }

        private static void ValidateCustomer(Customer customer)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(customer.firstName))
            {
                errors.Add("firstName", "Invalid customer first name");
            }

            if (string.IsNullOrWhiteSpace(customer.lastName))
            {
                errors.Add("lastName", "Invalid customer last name");
            }

            if (string.IsNullOrWhiteSpace(customer.email))
            {
                errors.Add("email", "Invalid customer email");
            }
            else if (!Regex.IsMatch(customer.email, @"\S+@\S+\.\S+"))
            {
                errors.Add("email", "Invalid customer email");
            }

            if (string.IsNullOrWhiteSpace(customer.phone))
            {
                errors.Add("phone", "Invalid customer phone number");
            }
            else if (!Regex.IsMatch(customer.phone, @"^\d{10}$"))
            {
                errors.Add("phone", "Invalid customer phone number");
            }

            if (errors.Count > 0)
            {
                // May add error logging here
                
                string errorMessage = string.Join("\n", errors.Values);
                throw new WebFaultException<string>(errorMessage, HttpStatusCode.Forbidden);
            }
        }

        // Get DB products with same code as products and sum their price
        private static decimal getTotalPrice(List<Product> products)
        {
            return products
                   .Join(_products_db, p => p.code, dbp => dbp.code, (p, dbp) => dbp.price)
                   .Sum();
        }

        // public List<Product> SQLTest()
        // {
        //     string connectionString = ConfigurationManager.ConnectionStrings["Database"].ConnectionString;
        //     using (SqlConnection connection = new SqlConnection(connectionString))
        //     {
        //         connection.Open();
        //         SqlCommand command = new SqlCommand("SELECT * FROM Table", connection);
        //         SqlDataReader reader = command.ExecuteReader();

        //         List<Product> products = reader.Cast<IDataRecord>()
        //             .Select(x => new Product
        //             {
        //                 // ID = (int)x["ID"],
        //                 name = (string)x["Name"],
        //                 code = (string)x["Code"],
        //                 type = (string)x["Type"],
        //                 price = (decimal)x["Price"],
        //             })
        //             .ToList();

        //         reader.Close();

        //         return products;
        //     }
        // }
    }

    public class CorsMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (WebOperationContext.Current != null)
            {
                // This is an incoming request. Add the CORS headers to the response.
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            // No action needed here.
        }
    }

    public class CorsBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // No action needed here.
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // No action needed here.
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CorsMessageInspector());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // No action needed here.
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string address = "http://localhost:8080/BookingService";

            BookingService BookingService = new BookingService();
            ServiceHost host = new ServiceHost(BookingService, new Uri(address));
            // ServiceHost host = new ServiceHost(typeof(BookingService), new Uri(address));

            // Add the CorsMessageInspector to the service host.
            host.Description.Endpoints[0].EndpointBehaviors.Add(new CorsBehavior());

            host.Open();
            Console.WriteLine("Service is running at: " + address);
            Console.ReadLine();
            host.Close();
        }
    }
}
