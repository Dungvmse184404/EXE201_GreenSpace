using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Domain.Constants
{
    public static class OrderStatus
    {
        public const string Pending = "Pending"; // khi mới tạo đơn hàng
        public const string Confirmed = "Confirmed";// sau khi thanh toán thành công
        public const string Shipping = "Shipping"; 
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Returned = "Returned";

    }
}
