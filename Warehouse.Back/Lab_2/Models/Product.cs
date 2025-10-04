using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2.Models
{
    public class Product
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public string Supplier { get; set; }
        public DateTime LastDeliveryDate { get; set; }
    }
}
