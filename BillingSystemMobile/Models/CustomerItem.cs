using System;
using System.Collections.Generic;
using System.Text;

namespace BillingSystemMobile.Models
{
    // Matches the JSON returned by GET /api/customer
    public class CustomerItem
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
