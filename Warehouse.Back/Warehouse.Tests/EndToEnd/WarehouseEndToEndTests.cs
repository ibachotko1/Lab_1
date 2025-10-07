using Xunit;
using Lab_2.Repository;
using Lab_2.Service;
using System;
using System.Linq;
using Lab_2.Models;

namespace Warehouse.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end тесты для проверки полного рабочего процесса системы управления складом
    /// </summary>
    public class WarehouseEndToEndTests
    {
        /// <summary>
        /// Проверяет полный цикл работы системы: приемка, списание и корректировка товара
        /// </summary>
        [Fact]
        public void CompleteWorkflow_AllOperations_Success()
        {
            // Arrange
            var repository = new InMemoryWarehouseRepository();
            var service = new WarehouseService(repository);

            var sku = "E2E001";
            var deliveryDate = DateTime.Now.AddDays(-10);
            var writeOffDate = DateTime.Now.AddDays(-5);
            var adjustmentDate = DateTime.Now.AddDays(-1);

            // Act - Complete workflow
            service.ReceiveGoods(sku, "E2E Product", 100, 25.50m, "E2E Supplier", deliveryDate);
            service.WriteOffGoods(sku, 10, "Quality issues", writeOffDate);
            service.InventoryAdjustment(sku, 95, "Cycle count adjustment", adjustmentDate);

            var product = service.GetAllProducts().First(p => p.SKU == sku);
            var operations = service.GetAllOperations().Where(o => o.SKU == sku).ToList();
            var totalValue = service.GetTotalInventoryValue();

            // Assert
            Assert.Equal(95, product.Quantity);
            Assert.Equal(3, operations.Count);
            Assert.Equal(95 * 25.50m, totalValue);

            // Verify operation types
            Assert.Contains(operations, o => o.Type == OperationType.Receive);
            Assert.Contains(operations, o => o.Type == OperationType.WriteOff);
            Assert.Contains(operations, o => o.Type == OperationType.InventoryAdjustment);
        }

        /// <summary>
        /// Проверяет корректность расчета общей стоимости при пустом складе
        /// </summary>
        [Fact]
        public void GetTotalInventoryValue_EmptyWarehouse_ReturnsZero()
        {
            // Arrange
            var repository = new InMemoryWarehouseRepository();
            var service = new WarehouseService(repository);

            // Act
            var totalValue = service.GetTotalInventoryValue();

            // Assert
            Assert.Equal(0m, totalValue);
        }

        /// <summary>
        /// Проверяет работу системы с несколькими товарами и корректность агрегированных данных
        /// </summary>
        [Fact]
        public void MultipleProducts_OperationsAndInventoryValue_CorrectAggregation()
        {
            // Arrange
            var repository = new InMemoryWarehouseRepository();
            var service = new WarehouseService(repository);

            // Act - Add multiple products
            service.ReceiveGoods("PROD1", "Product 1", 50, 10.0m, "Supplier A", DateTime.Now.AddDays(-3));
            service.ReceiveGoods("PROD2", "Product 2", 30, 15.5m, "Supplier B", DateTime.Now.AddDays(-2));
            service.WriteOffGoods("PROD1", 5, "Damaged", DateTime.Now.AddDays(-1));

            var products = service.GetAllProducts().ToList();
            var operations = service.GetAllOperations().ToList();
            var totalValue = service.GetTotalInventoryValue();

            // Assert
            Assert.Equal(2, products.Count);
            Assert.Equal(3, operations.Count);

            // (50-5)*10.0 + 30*15.5 = 45*10 + 465 = 450 + 465 = 915
            Assert.Equal(915m, totalValue);
        }

        /// <summary>
        /// Проверяет, что операции правильно фильтруются по SKU при запросе истории операций
        /// </summary>
        [Fact]
        public void GetOperationsBySku_MultipleProducts_ReturnsCorrectFilteredOperations()
        {
            // Arrange
            var repository = new InMemoryWarehouseRepository();
            var service = new WarehouseService(repository);

            // Act
            service.ReceiveGoods("FILTER1", "Filter Product 1", 100, 10m, "Supplier", DateTime.Now.AddDays(-3));
            service.ReceiveGoods("FILTER2", "Filter Product 2", 50, 20m, "Supplier", DateTime.Now.AddDays(-2));
            service.WriteOffGoods("FILTER1", 10, "Reason", DateTime.Now.AddDays(-1));

            // Use repository directly for filtered operations
            var filter1Operations = repository.GetOperationsBySku("FILTER1").ToList();
            var filter2Operations = repository.GetOperationsBySku("FILTER2").ToList();

            // Assert
            Assert.Equal(2, filter1Operations.Count); // Receive and WriteOff
            Assert.Equal(1, filter2Operations.Count); // Only Receive
            Assert.All(filter1Operations, o => Assert.Equal("FILTER1", o.SKU));
            Assert.All(filter2Operations, o => Assert.Equal("FILTER2", o.SKU));
        }

        /// <summary>
        /// Проверяет сценарий с корректировкой инвентаря, приводящей к увеличению количества товара
        /// </summary>
        [Fact]
        public void InventoryAdjustment_IncreaseQuantity_UpdatesCorrectly()
        {
            // Arrange
            var repository = new InMemoryWarehouseRepository();
            var service = new WarehouseService(repository);

            // Act
            service.ReceiveGoods("ADJUST1", "Adjust Product", 50, 10m, "Supplier", DateTime.Now.AddDays(-2));
            service.InventoryAdjustment("ADJUST1", 60, "Found extra items", DateTime.Now.AddDays(-1));

            var product = service.GetAllProducts().First(p => p.SKU == "ADJUST1");
            var operations = repository.GetOperationsBySku("ADJUST1").ToList();

            // Assert
            Assert.Equal(60, product.Quantity);
            Assert.Equal(2, operations.Count);
            Assert.Contains(operations, o => o.Type == OperationType.InventoryAdjustment && o.Quantity == 60);
        }

        /// <summary>
        /// Проверяет сценарий с корректировкой инвентаря, приводящей к уменьшению количества товара
        /// </summary>
        [Fact]
        public void InventoryAdjustment_DecreaseQuantity_UpdatesCorrectly()
        {
            // Arrange
            var repository = new InMemoryWarehouseRepository();
            var service = new WarehouseService(repository);

            // Act
            service.ReceiveGoods("ADJUST2", "Adjust Product", 50, 10m, "Supplier", DateTime.Now.AddDays(-2));
            service.InventoryAdjustment("ADJUST2", 45, "Missing items", DateTime.Now.AddDays(-1));

            var product = service.GetAllProducts().First(p => p.SKU == "ADJUST2");
            var operations = repository.GetOperationsBySku("ADJUST2").ToList();

            // Assert
            Assert.Equal(45, product.Quantity);
            Assert.Equal(2, operations.Count);
            Assert.Contains(operations, o => o.Type == OperationType.InventoryAdjustment && o.Quantity == 45);
        }

        /// <summary>
        /// Проверяет сценарий полного списания товара и последующее состояние системы
        /// </summary>
        [Fact]
        public void WriteOffGoods_CompleteWriteOff_ProductRemainsWithZeroQuantity()
        {
            // Arrange
            var repository = new InMemoryWarehouseRepository();
            var service = new WarehouseService(repository);

            // Act
            service.ReceiveGoods("WRITEOFF1", "WriteOff Product", 25, 8.5m, "Supplier", DateTime.Now.AddDays(-2));
            service.WriteOffGoods("WRITEOFF1", 25, "Complete disposal", DateTime.Now.AddDays(-1));

            var product = service.GetAllProducts().First(p => p.SKU == "WRITEOFF1");
            var operations = repository.GetOperationsBySku("WRITEOFF1").ToList();

            // Assert
            Assert.Equal(0, product.Quantity);
            Assert.Equal(2, operations.Count);
            Assert.Contains(operations, o => o.Type == OperationType.WriteOff && o.Quantity == 25);
        }
    }
}