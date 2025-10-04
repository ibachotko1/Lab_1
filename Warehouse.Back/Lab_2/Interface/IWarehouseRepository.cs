using Lab_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2.Interface
{
    public interface IWarehouseRepository
    {
        void AddProduct(Product product);
        Product GetProduct(string sku);
        void UpdateProduct(Product product);
        IEnumerable<Product> GetAllProducts();
        void AddOperation(OperationRecord operation);
        IEnumerable<OperationRecord> GetOperations();
        IEnumerable<OperationRecord> GetOperationsBySku(string sku);
    }
}
