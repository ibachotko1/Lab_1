using Back.Интерфейсы;
using Back.Модели;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Back.Сервисы
{
    public class WarehouseService
    {
        private readonly IWarehouseRepository _repository;

        public decimal ОбщаяСтоимость => _repository.GetAllProducts().Sum(p => p.Количество * p.Цена);

        public WarehouseService(IWarehouseRepository repository)
        {
            _repository = repository;
        }

        public void ПринятьТовар(
            string артикул,
            string наименование,
            int количество,
            decimal цена,
            string поставщик,
            DateTime датаПоставки)
        {
            if (_repository.GetProduct(артикул) != null)
                throw new ArgumentException("Артикул уже существует в системе");

            if (количество <= 0)
                throw new ArgumentOutOfRangeException(nameof(количество), "Количество должно быть больше 0");

            if (цена <= 0)
                throw new ArgumentOutOfRangeException(nameof(цена), "Цена должна быть больше 0");

            if (string.IsNullOrWhiteSpace(поставщик))
                throw new ArgumentException("Поставщик не может быть пустым");

            var product = new Product
            {
                Артикул = артикул,
                Наименование = наименование,
                Количество = количество,
                Цена = цена,
                Поставщик = поставщик,
                ДатаПоставки = датаПоставки
                // Убрали Описание и ЕдиницаИзмерения
            };

            _repository.AddProduct(product);

            _repository.AddOperationRecord(new OperationRecord
            {
                Артикул = артикул,
                Количество = количество,
                Цена = цена,
                Описание = $"Приемка от поставщика: {поставщик}",
                ТипОперации = OperationType.Поступление
            });
        }

        public void СписатьТовар(
            string артикул,
            int количество,
            string причина,
            DateTime датаСписания)
        {
            var product = _repository.GetProduct(артикул);
            if (product == null)
                throw new ArgumentException("Товар не найден");

            if (количество <= 0)
                throw new ArgumentOutOfRangeException(nameof(количество), "Количество списания должно быть больше 0");

            if (product.Количество < количество)
                throw new InvalidOperationException("Недостаточный остаток для списания");

            if (string.IsNullOrWhiteSpace(причина))
                throw new ArgumentException("Причина списания не может быть пустой");

            product.Количество -= количество;
            _repository.UpdateProduct(product);

            _repository.AddOperationRecord(new OperationRecord
            {
                Артикул = артикул,
                Количество = количество,
                Цена = product.Цена,
                Описание = $"Списание: {причина}",
                ТипОперации = OperationType.Списание
            });
        }

        public List<Product> GetAllProducts() => _repository.GetAllProducts();

        public List<OperationRecord> GetOperationHistory() => _repository.GetOperationHistory();
    }
}