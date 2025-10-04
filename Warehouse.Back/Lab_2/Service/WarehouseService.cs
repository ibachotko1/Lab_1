using Lab_2.Interface;
using Lab_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2.Service
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _repository;

        public WarehouseService(IWarehouseRepository repository)
        {
            _repository = repository;
        }

        public void ReceiveGoods(string sku, string name, int quantity, decimal pricePerUnit, string supplier, DateTime deliveryDate)
        {
            // Pre-conditions
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");

            if (pricePerUnit <= 0)
                throw new ArgumentOutOfRangeException(nameof(pricePerUnit), "Price must be greater than 0");

            if (string.IsNullOrEmpty(supplier))
                throw new ArgumentException("Supplier cannot be null or empty");

            if (deliveryDate > DateTime.Now)
                throw new InvalidOperationException("Delivery date cannot be in the future");

            var existingProduct = _repository.GetProduct(sku);
            if (existingProduct != null)
                throw new ArgumentException($"Product with SKU {sku} already exists");

            // Create new product
            var product = new Product
            {
                SKU = sku,
                Name = name,
                Quantity = quantity,
                PricePerUnit = pricePerUnit,
                Supplier = supplier,
                LastDeliveryDate = deliveryDate
            };

            // Post-conditions
            _repository.AddProduct(product);

            var operation = new OperationRecord
            {
                SKU = sku,
                Type = OperationType.Receive,
                Quantity = quantity,
                PricePerUnit = pricePerUnit,
                OperationDate = deliveryDate
            };

            _repository.AddOperation(operation);
        }

        public void WriteOffGoods(string sku, int quantity, string reason, DateTime writeOffDate)
        {
            // Pre-conditions
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");

            if (string.IsNullOrEmpty(reason))
                throw new ArgumentException("Reason cannot be null or empty");

            if (writeOffDate > DateTime.Now)
                throw new InvalidOperationException("Write-off date cannot be in the future");

            var product = _repository.GetProduct(sku);
            if (product == null)
                throw new ArgumentException($"Product with SKU {sku} not found");

            if (product.Quantity < quantity)
                throw new InvalidOperationException($"Insufficient quantity. Available: {product.Quantity}, Requested: {quantity}");

            // Update product
            product.Quantity -= quantity;
            _repository.UpdateProduct(product);

            // Post-conditions
            var operation = new OperationRecord
            {
                SKU = sku,
                Type = OperationType.WriteOff,
                Quantity = quantity,
                Reason = reason,
                OperationDate = writeOffDate
            };

            _repository.AddOperation(operation);
        }

        public void InventoryAdjustment(string sku, int actualQuantity, string reason, DateTime adjustmentDate)
        {
            // Pre-conditions
            if (actualQuantity < 0)
                throw new ArgumentOutOfRangeException(nameof(actualQuantity), "Quantity cannot be negative");

            if (string.IsNullOrEmpty(reason))
                throw new ArgumentException("Reason cannot be null or empty");

            if (adjustmentDate > DateTime.Now)
                throw new InvalidOperationException("Adjustment date cannot be in the future");

            var product = _repository.GetProduct(sku);
            if (product == null)
                throw new ArgumentException($"Product with SKU {sku} not found");

            var difference = actualQuantity - product.Quantity;

            // Update product
            product.Quantity = actualQuantity;
            _repository.UpdateProduct(product);

            // Post-conditions
            var operation = new OperationRecord
            {
                SKU = sku,
                Type = OperationType.InventoryAdjustment,
                Quantity = actualQuantity,
                Reason = reason,
                OperationDate = adjustmentDate
            };

            _repository.AddOperation(operation);
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _repository.GetAllProducts();
        }

        public IEnumerable<OperationRecord> GetAllOperations()
        {
            return _repository.GetOperations();
        }

        public decimal GetTotalInventoryValue()
        {
            return _repository.GetAllProducts().Sum(p => p.Quantity * p.PricePerUnit);
        }
    }
}
