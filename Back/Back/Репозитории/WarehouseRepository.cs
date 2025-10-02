using Back.Интерфейсы;
using Back.Модели;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagement.Core.Модели;

namespace Back.Репозитории
{
    /// <summary>
    /// Реализация репозитория warehouse (склад)
    /// </summary>
    public class WarehouseRepository : IWarehouseRepository
    {
        // Хранилище товаров - словарь для быстрого поиска по SKU
        // SKU (Stock Keeping Unit) - уникальный артикул товара
        private readonly Dictionary<string, Product> _products = new Dictionary<string, Product>();

        // Журнал операций - список всех действий на складе
        private readonly List<OperationRecord> _operations = new List<OperationRecord>();

        /// <summary>
        /// Добавление нового товара в систему
        /// </summary>
        public void AddProduct(Product product)
        {
            // Проверка уникальности SKU
            if (_products.ContainsKey(product.SKU))
                throw new ArgumentException($"Товар с артикулом {product.SKU} уже существует в системе");

            // Добавление товара в словарь
            _products[product.SKU] = product;
        }

        /// <summary>
        /// Получение товара по артикулу (SKU)
        /// Возвращает null если товар не найден
        /// </summary>
        public Product GetProduct(string sku)
        {
            _products.TryGetValue(sku, out Product product);
            return product;
        }

        /// <summary>
        /// Обновление информации о товаре
        /// </summary>
        public void UpdateProduct(Product product)
        {
            if (!_products.ContainsKey(product.SKU))
                throw new ArgumentException($"Товар с артикулом {product.SKU} не найден");

            _products[product.SKU] = product;
        }

        /// <summary>
        /// Получение всех товаров на складе
        /// </summary>
        public IEnumerable<Product> GetAllProducts() => _products.Values;

        /// <summary>
        /// Добавление записи в журнал операций
        /// </summary>
        public void AddOperationRecord(OperationRecord record) => _operations.Add(record);

        /// <summary>
        /// Получение полной истории операций
        /// </summary>
        public IEnumerable<OperationRecord> GetOperationHistory() => _operations;
    }
}
