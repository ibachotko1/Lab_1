using Lab_2.Models;
using System;
using System.Collections.Generic;
using Warehouse.Back.Models;

namespace Lab_2.Interface
{
    public interface IWarehouseService
    {
        OperationResult ReceiveGoods(string sku, string name, int quantity, decimal pricePerUnit, string supplier, DateTime deliveryDate);
        OperationResult WriteOffGoods(string sku, int quantity, string reason, DateTime writeOffDate);
        OperationResult InventoryAdjustment(string sku, int actualQuantity, string reason, DateTime adjustmentDate);
        IEnumerable<Product> GetAllProducts();
        IEnumerable<OperationRecord> GetAllOperations();
        decimal GetTotalInventoryValue();
    }
}