using Back.Модели;
using System.Collections.Generic;

namespace Back.Интерфейсы
{
    public interface IWarehouseRepository
    {
        void AddProduct(Product product);
        Product GetProduct(string артикул);
        List<Product> GetAllProducts();
        void UpdateProduct(Product product);
        void AddOperationRecord(OperationRecord record);
        List<OperationRecord> GetOperationHistory();
    }
}