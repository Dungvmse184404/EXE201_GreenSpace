using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.PayOS
{
    public class PayOSCallBackDto
    {
        public string? code { get; set; }
        public string? id { get; set; }
        public string? orderCode { get; set; }
        public string? status { get; set; }
    }
}
