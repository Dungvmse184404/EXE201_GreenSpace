using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Domain.Constants
{
    public static class OrderStatus
    {
        public const string Pending = "Pending"; // khi moi tao don
        public const string Confirmed = "Confirmed";// sau khi thanh toans
        public const string Shipping = "Shipping"; 
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Returned = "Returned";

    }
}
