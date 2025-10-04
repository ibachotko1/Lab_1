using Back.Интерфейсы;
using Back.Модели;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System;

namespace Back.Репозитории
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly Dictionary<string, Product> _products = new Dictionary<string, Product>();
        private readonly List<OperationRecord> _operations = new List<OperationRecord>();

        private const string ProductsFile = "products.json";
        private const string OperationsFile = "operations.json";

        public WarehouseRepository()
        {
            LoadData();
        }

        public void AddProduct(Product product)
        {
            if (_products.ContainsKey(product.Артикул))
                throw new ArgumentException($"Товар с артикулом {product.Артикул} уже существует в системе");

            _products[product.Артикул] = product;
            SaveProducts();
        }

        public Product GetProduct(string артикул)
        {
            _products.TryGetValue(артикул, out Product product);
            return product;
        }

        public List<Product> GetAllProducts()
        {
            return _products.Values.ToList();
        }

        public void UpdateProduct(Product product)
        {
            if (!_products.ContainsKey(product.Артикул))
                throw new ArgumentException($"Товар с артикулом {product.Артикул} не найден");

            _products[product.Артикул] = product;
            SaveProducts();
        }

        public void AddOperationRecord(OperationRecord record)
        {
            record.ДатаОперации = DateTime.Now;
            _operations.Add(record);
            SaveOperations();
        }

        public List<OperationRecord> GetOperationHistory()
        {
            return _operations.ToList();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка товаров
                if (File.Exists(ProductsFile))
                {
                    var json = File.ReadAllText(ProductsFile);
                    var products = JsonSerializer.Deserialize<List<Product>>(json);
                    _products.Clear();
                    foreach (var product in products)
                    {
                        _products[product.Артикул] = product;
                    }
                }

                // Загрузка операций
                if (File.Exists(OperationsFile))
                {
                    var json = File.ReadAllText(OperationsFile);
                    var operations = JsonSerializer.Deserialize<List<OperationRecord>>(json);
                    _operations.Clear();
                    _operations.AddRange(operations);
                }
            }
            catch (Exception ex)
            {
                // Если файлы повреждены, создаем новые
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void SaveProducts()
        {
            try
            {
                var json = JsonSerializer.Serialize(_products.Values.ToList(), new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ProductsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения товаров: {ex.Message}");
            }
        }

        private void SaveOperations()
        {
            try
            {
                var json = JsonSerializer.Serialize(_operations, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(OperationsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения операций: {ex.Message}");
            }
        }
    }
}