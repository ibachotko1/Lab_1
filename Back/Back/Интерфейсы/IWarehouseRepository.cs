using Back.Модели;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagement.Core.Модели;

namespace Back.Интерфейсы
{
    public interface IWarehouseRepository
    {
        void AddProduct(Product product);
        Product GetProduct(string sku);
        void UpdateProduct(Product product);
        IEnumerable<Product> GetAllProducts();
        void AddOperationRecord(OperationRecord record);
        IEnumerable<OperationRecord> GetOperationHistory();
    }
}
