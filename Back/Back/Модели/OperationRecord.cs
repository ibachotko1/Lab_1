using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back.Модели
{
    /// <summary>
    /// Запись о операции для журналирования
    /// </summary>
    public class OperationRecord
    {
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Description { get; set; }
        public DateTime OperationDate { get; set; }
        public OperationType Type { get; set; }
    }
}
