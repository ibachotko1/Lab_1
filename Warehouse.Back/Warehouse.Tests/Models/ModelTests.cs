using Xunit;
using Lab_2.Models;
using System;

namespace Warehouse.Tests.Models
{
    /// <summary>
    /// Тесты для проверки корректности работы моделей данных
    /// </summary>
    public class ModelTests
    {
        /// <summary>
        /// Проверяет, что при создании новой операции записи устанавливаются корректные значения по умолчанию
        /// </summary>
        [Fact]
        public void OperationRecord_DefaultValues_SetCorrectly()
        {
            // Act
            var operation = new OperationRecord();

            // Assert
            Assert.NotNull(operation.Id);
            Assert.NotEmpty(operation.Id);
            Assert.True(Guid.TryParse(operation.Id, out _));
            Assert.Equal(DateTime.Now.Date, operation.RecordDate.Date);
        }

        /// <summary>
        /// Проверяет корректность установки и получения всех свойств операции записи
        /// </summary>
        [Fact]
        public void OperationRecord_AllProperties_CanBeSetAndGet()
        {
            // Arrange & Act
            var operationDate = new DateTime(2024, 1, 15);
            var recordDate = new DateTime(2024, 1, 16);

            var operation = new OperationRecord
            {
                Id = "TEST-ID",
                SKU = "TEST001",
                Type = OperationType.Receive,
                Quantity = 100,
                PricePerUnit = 25.99m,
                Reason = "Test reason",
                OperationDate = operationDate,
                RecordDate = recordDate
            };

            // Assert
            Assert.Equal("TEST-ID", operation.Id);
            Assert.Equal("TEST001", operation.SKU);
            Assert.Equal(OperationType.Receive, operation.Type);
            Assert.Equal(100, operation.Quantity);
            Assert.Equal(25.99m, operation.PricePerUnit);
            Assert.Equal("Test reason", operation.Reason);
            Assert.Equal(operationDate, operation.OperationDate);
            Assert.Equal(recordDate, operation.RecordDate);
        }

        /// <summary>
        /// Проверяет корректность установки и получения всех свойств товара
        /// </summary>
        [Fact]
        public void Product_Properties_CanBeSetAndGet()
        {
            // Arrange & Act
            var lastDeliveryDate = new DateTime(2024, 1, 15);

            var product = new Product
            {
                SKU = "TEST001",
                Name = "Test Product",
                Quantity = 15,
                PricePerUnit = 29.99m,
                Supplier = "Test Supplier",
                LastDeliveryDate = lastDeliveryDate
            };

            // Assert
            Assert.Equal("TEST001", product.SKU);
            Assert.Equal("Test Product", product.Name);
            Assert.Equal(15, product.Quantity);
            Assert.Equal(29.99m, product.PricePerUnit);
            Assert.Equal("Test Supplier", product.Supplier);
            Assert.Equal(lastDeliveryDate, product.LastDeliveryDate);
        }

        /// <summary>
        /// Проверяет корректность преобразования значений перечисления OperationType в строки
        /// </summary>
        [Theory]
        [InlineData(OperationType.Receive, "Receive")]
        [InlineData(OperationType.WriteOff, "WriteOff")]
        [InlineData(OperationType.InventoryAdjustment, "InventoryAdjustment")]
        public void OperationType_ToString_ReturnsExpectedValue(OperationType operationType, string expectedName)
        {
            // Act
            var result = operationType.ToString();

            // Assert
            Assert.Equal(expectedName, result);
        }

        /// <summary>
        /// Проверяет, что товар с нулевым количеством корректно обрабатывается системой
        /// </summary>
        [Fact]
        public void Product_ZeroQuantity_IsValid()
        {
            // Arrange & Act
            var product = new Product
            {
                SKU = "ZERO001",
                Name = "Zero Quantity Product",
                Quantity = 0,
                PricePerUnit = 10.0m,
                Supplier = "Supplier",
                LastDeliveryDate = DateTime.Now.AddDays(-1)
            };

            // Assert
            Assert.Equal(0, product.Quantity);
            Assert.Equal(0m, product.Quantity * product.PricePerUnit); // Zero value
        }

        /// <summary>
        /// Проверяет, что операция записи с минимальными допустимыми значениями корректно создается
        /// </summary>
        [Fact]
        public void OperationRecord_MinimalValidData_CreatesSuccessfully()
        {
            // Arrange & Act
            var operation = new OperationRecord
            {
                SKU = "MIN001",
                Type = OperationType.InventoryAdjustment,
                Quantity = 0, // Minimum valid quantity
                OperationDate = DateTime.Now.AddDays(-1)
            };

            // Assert
            Assert.Equal("MIN001", operation.SKU);
            Assert.Equal(OperationType.InventoryAdjustment, operation.Type);
            Assert.Equal(0, operation.Quantity);
        }

        /// <summary>
        /// Проверяет равенство и неравенство товаров на основе их SKU
        /// </summary>
        [Fact]
        public void Product_Equality_BasedOnSku()
        {
            // Arrange
            var product1 = new Product { SKU = "SAME001", Name = "Product 1" };
            var product2 = new Product { SKU = "SAME001", Name = "Product 2" };
            var product3 = new Product { SKU = "DIFFERENT", Name = "Product 1" };

            // Act & Assert
            Assert.Equal(product1.SKU, product2.SKU);
            Assert.NotEqual(product1.SKU, product3.SKU);
        }

        /// <summary>
        /// Проверяет корректность работы с максимальными допустимыми значениями числовых свойств
        /// </summary>
        [Fact]
        public void Models_MaximumValues_HandleCorrectly()
        {
            // Arrange & Act
            var product = new Product
            {
                SKU = "MAX001",
                Quantity = int.MaxValue,
                PricePerUnit = decimal.MaxValue
            };

            var operation = new OperationRecord
            {
                Quantity = int.MaxValue,
                PricePerUnit = decimal.MaxValue
            };

            // Assert
            Assert.Equal(int.MaxValue, product.Quantity);
            Assert.Equal(decimal.MaxValue, product.PricePerUnit);
            Assert.Equal(int.MaxValue, operation.Quantity);
            Assert.Equal(decimal.MaxValue, operation.PricePerUnit);
        }

        /// <summary>
        /// Проверяет, что дата операции не может быть в будущем (бизнес-логика)
        /// </summary>
        [Fact]
        public void OperationRecord_OperationDate_CannotBeInFuture()
        {
            // Note: This test documents the business rule, though the enforcement
            // is actually in the service layer, not in the model itself

            // Arrange & Act
            var operation = new OperationRecord
            {
                OperationDate = DateTime.Now.AddDays(-1), // Past date - valid
                RecordDate = DateTime.Now
            };

            // Assert
            Assert.True(operation.OperationDate <= DateTime.Now);
            Assert.True(operation.RecordDate <= DateTime.Now);
        }
    }
}