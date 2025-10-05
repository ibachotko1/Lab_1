using Lab_2.Interface;
using Lab_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2.Repository
{
    public class InMemoryWarehouseRepository : IWarehouseRepository
    {
        private readonly List<Product> _products = new List<Product>();
        private readonly List<OperationRecord> _operations = new List<OperationRecord>();    
        private readonly object _lock = new object();

        public void AddProduct(Product product)
        {
            lock (_lock)
            {
                if (_products.Any(p => p.SKU == product.SKU))
                    throw new ArgumentException($"Product with SKU {product.SKU} already exists");

                _products.Add(product);
            }
        }

        public Product GetProduct(string sku)
        {
            lock (_lock)
            {
                return _products.FirstOrDefault(p => p.SKU == sku);
            }
        }

        public void UpdateProduct(Product product)
        {
            lock (_lock)
            {
                var existingProduct = GetProduct(product.SKU);
                if (existingProduct != null)
                {
                    existingProduct.Quantity = product.Quantity;
                    existingProduct.PricePerUnit = product.PricePerUnit;
                    existingProduct.Name = product.Name;
                    existingProduct.Supplier = product.Supplier;
                    existingProduct.LastDeliveryDate = product.LastDeliveryDate;
                }
            }
        }

        public IEnumerable<Product> GetAllProducts()
        {
            lock (_lock)
            {
                return _products.ToList();
            }
        }

        public void AddOperation(OperationRecord operation)
        {
            lock (_lock)
            {
                _operations.Add(operation);
            }
        }

        public IEnumerable<OperationRecord> GetOperations()
        {
            lock (_lock)
            {
                return _operations.ToList();
            }
        }

        public IEnumerable<OperationRecord> GetOperationsBySku(string sku)
        {
            lock (_lock)
            {
                return _operations.Where(o => o.SKU == sku).ToList();
            }
        }
    }

}
