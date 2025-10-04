using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2.Models
{
    public class OperationRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SKU { get; set; }
        public OperationType Type { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public string Reason { get; set; }
        public DateTime OperationDate { get; set; }
        public DateTime RecordDate { get; set; } = DateTime.Now;
    }
}
