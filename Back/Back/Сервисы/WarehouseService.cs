using Back.Интерфейсы;
using Back.Модели;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManagement.Core.Модели;

namespace Back.Сервисы
{
    /// <summary>
    /// Сервис склада - содержит основную бизнес-логику
    /// Реализует контракты операций согласно ТЗ
    /// </summary>
    public class WarehouseService
    {
        private readonly IWarehouseRepository _repository;

        /// <summary>
        /// Общая стоимость инвентаря (вычисляемое свойство)
        /// </summary>
        public decimal TotalInventoryValue => _repository.GetAllProducts().Sum(p => p.Quantity * p.UnitPrice);

        public WarehouseService(IWarehouseRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// ОПЕРАЦИЯ 1: Приемка товара на склад
        /// </summary>
        public void ReceiveGoods(
            string sku,
            string name,
            int quantity,
            decimal unitPrice,
            string supplier,
            DateTime deliveryDate)
        {
            // ПРОВЕРКА ПРЕДУСЛОВИЙ (Pre-conditions)
            if (_repository.GetProduct(sku) != null)
                throw new ArgumentException("Артикул уже существует в системе");

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Количество должно быть больше 0");

            if (unitPrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(unitPrice), "Цена должна быть больше 0");

            if (string.IsNullOrWhiteSpace(supplier))
                throw new ArgumentException("Поставщик не может быть пустым");

            if (deliveryDate > DateTime.Now)
                throw new InvalidOperationException("Дата поставки не может быть в будущем");

            // ОСНОВНАЯ ЛОГИКА
            var product = new Product
            {
                SKU = sku,
                Name = name,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Supplier = supplier,
                LastDeliveryDate = deliveryDate
            };

            _repository.AddProduct(product);

            // ЗАПИСЬ В ЖУРНАЛ ОПЕРАЦИЙ (Постусловие)
            _repository.AddOperationRecord(new OperationRecord
            {
                SKU = sku,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Description = $"Приемка от поставщика: {supplier}",
                OperationDate = DateTime.Now,
                Type = OperationType.Receiving
            });
        }

        /// <summary>
        /// ОПЕРАЦИЯ 2: Списание товара со склада
        /// </summary>
        public void WriteOffGoods(
            string sku,
            int quantity,
            string reason,
            DateTime writeOffDate)
        {
            // ПРОВЕРКА ПРЕДУСЛОВИЙ
            var product = _repository.GetProduct(sku);
            if (product == null)
                throw new ArgumentException("Товар не найден");

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Количество списания должно быть больше 0");

            if (product.Quantity < quantity)
                throw new InvalidOperationException("Недостаточный остаток для списания");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Причина списания не может быть пустой");

            if (writeOffDate > DateTime.Now)
                throw new InvalidOperationException("Дата списания не может быть в будущем");

            // ОСНОВНАЯ ЛОГИКА
            product.Quantity -= quantity;
            _repository.UpdateProduct(product);

            // ЗАПИСЬ В ЖУРНАЛ
            _repository.AddOperationRecord(new OperationRecord
            {
                SKU = sku,
                Quantity = quantity,
                UnitPrice = product.UnitPrice,
                Description = $"Списание: {reason}",
                OperationDate = DateTime.Now,
                Type = OperationType.WriteOff
            });
        }

        /// <summary>
        /// ОПЕРАЦИЯ 3: Корректировка при инвентаризации
        /// </summary>
        public void InventoryAdjustment(
            string sku,
            int actualQuantity,
            string reason,
            DateTime adjustmentDate)
        {
            // ПРОВЕРКА ПРЕДУСЛОВИЙ
            var product = _repository.GetProduct(sku);
            if (product == null)
                throw new ArgumentException("Товар не найден");

            if (actualQuantity < 0)
                throw new ArgumentOutOfRangeException(nameof(actualQuantity), "Фактическое количество не может быть отрицательным");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Причина расхождения не может быть пустой");

            if (adjustmentDate > DateTime.Now)
                throw new InvalidOperationException("Дата инвентаризации не может быть в будущем");

            // ОСНОВНАЯ ЛОГИКА
            var difference = actualQuantity - product.Quantity;
            product.Quantity = actualQuantity;
            _repository.UpdateProduct(product);

            // ЗАПИСЬ В ЖУРНАЛ
            _repository.AddOperationRecord(new OperationRecord
            {
                SKU = sku,
                Quantity = difference,
                UnitPrice = product.UnitPrice,
                Description = $"Инвентаризация: {reason} (было: {product.Quantity - difference}, стало: {actualQuantity})",
                OperationDate = DateTime.Now,
                Type = OperationType.Inventory
            });
        }
    }
}
