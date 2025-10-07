using Moq;
using Xunit;
using Lab_2.Interface;
using Lab_2.Models;
using Lab_2.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Warehouse.Tests.Unit
{
    /// <summary>
    /// Тесты для проверки функциональности WarehouseService
    /// </summary>
    public class WarehouseServiceTests
    {
        private readonly Mock<IWarehouseRepository> _mockRepository;
        private readonly WarehouseService _warehouseService;

        public WarehouseServiceTests()
        {
            _mockRepository = new Mock<IWarehouseRepository>();
            _warehouseService = new WarehouseService(_mockRepository.Object);
        }

        /// <summary>
        /// Проверяет успешное добавление товара при корректных параметрах
        /// </summary>
        [Fact]
        public void ReceiveGoods_ValidParameters_AddsProductAndOperation()
        {
            // Arrange
            var sku = "TEST001";
            var name = "Test Product";
            var quantity = 10;
            var price = 15.99m;
            var supplier = "Test Supplier";
            var deliveryDate = DateTime.Now.AddDays(-1);

            _mockRepository.Setup(r => r.GetProduct(sku)).Returns((Product)null);

            // Act
            _warehouseService.ReceiveGoods(sku, name, quantity, price, supplier, deliveryDate);

            // Assert
            _mockRepository.Verify(r => r.AddProduct(It.Is<Product>(p =>
                p.SKU == sku &&
                p.Name == name &&
                p.Quantity == quantity &&
                p.PricePerUnit == price &&
                p.Supplier == supplier &&
                p.LastDeliveryDate == deliveryDate
            )), Times.Once);

            _mockRepository.Verify(r => r.AddOperation(It.Is<OperationRecord>(o =>
                o.SKU == sku &&
                o.Type == OperationType.Receive &&
                o.Quantity == quantity &&
                o.PricePerUnit == price &&
                o.OperationDate == deliveryDate
            )), Times.Once);
        }

        /// <summary>
        /// Проверяет, что при нулевом или отрицательном количестве выбрасывается исключение
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void ReceiveGoods_InvalidQuantity_ThrowsException(int invalidQuantity)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                _warehouseService.ReceiveGoods("TEST", "Product", invalidQuantity, 10m, "Supplier", DateTime.Now.AddDays(-1))
            );

            Assert.Equal("quantity", exception.ParamName);
        }

        /// <summary>
        /// Проверяет, что при нулевой или отрицательной цене выбрасывается исключение
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-5.5)]
        public void ReceiveGoods_InvalidPrice_ThrowsException(decimal invalidPrice)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                _warehouseService.ReceiveGoods("TEST", "Product", 10, invalidPrice, "Supplier", DateTime.Now.AddDays(-1))
            );

            Assert.Equal("pricePerUnit", exception.ParamName);
        }

        /// <summary>
        /// Проверяет, что при пустом поставщике выбрасывается исключение
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ReceiveGoods_InvalidSupplier_ThrowsException(string invalidSupplier)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _warehouseService.ReceiveGoods("TEST", "Product", 10, 10m, invalidSupplier, DateTime.Now.AddDays(-1))
            );

            Assert.Equal("supplier", exception.ParamName);
        }

        /// <summary>
        /// Проверяет, что при дате поставки в будущем выбрасывается исключение
        /// </summary>
        [Fact]
        public void ReceiveGoods_FutureDeliveryDate_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _warehouseService.ReceiveGoods("TEST", "Product", 10, 10m, "Supplier", DateTime.Now.AddDays(1))
            );
        }

        /// <summary>
        /// Проверяет, что при добавлении товара с существующим SKU выбрасывается исключение
        /// </summary>
        [Fact]
        public void ReceiveGoods_DuplicateSku_ThrowsException()
        {
            // Arrange
            var existingProduct = new Product { SKU = "EXIST001" };
            _mockRepository.Setup(r => r.GetProduct("EXIST001")).Returns(existingProduct);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _warehouseService.ReceiveGoods("EXIST001", "Product", 10, 10m, "Supplier", DateTime.Now.AddDays(-1))
            );

            Assert.Contains("already exists", exception.Message);
        }

        /// <summary>
        /// Проверяет успешное списание товара при корректных параметрах
        /// </summary>
        [Fact]
        public void WriteOffGoods_ValidParameters_UpdatesProductAndCreatesOperation()
        {
            // Arrange
            var sku = "TEST001";
            var existingProduct = new Product
            {
                SKU = sku,
                Quantity = 20,
                PricePerUnit = 10m
            };

            _mockRepository.Setup(r => r.GetProduct(sku)).Returns(existingProduct);

            // Act
            _warehouseService.WriteOffGoods(sku, 5, "Damaged", DateTime.Now.AddDays(-1));

            // Assert
            _mockRepository.Verify(r => r.UpdateProduct(It.Is<Product>(p =>
                p.SKU == sku && p.Quantity == 15
            )), Times.Once);

            _mockRepository.Verify(r => r.AddOperation(It.Is<OperationRecord>(o =>
                o.Type == OperationType.WriteOff &&
                o.Quantity == 5 &&
                o.Reason == "Damaged"
            )), Times.Once);
        }

        /// <summary>
        /// Проверяет, что при списании нулевого или отрицательного количества выбрасывается исключение
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void WriteOffGoods_InvalidQuantity_ThrowsException(int invalidQuantity)
        {
            // Arrange
            var sku = "TEST001";
            var existingProduct = new Product { SKU = sku, Quantity = 10 };
            _mockRepository.Setup(r => r.GetProduct(sku)).Returns(existingProduct);

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                _warehouseService.WriteOffGoods(sku, invalidQuantity, "Damaged", DateTime.Now.AddDays(-1))
            );

            Assert.Equal("quantity", exception.ParamName);
        }

        /// <summary>
        /// Проверяет, что при списании с пустой причиной выбрасывается исключение
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WriteOffGoods_InvalidReason_ThrowsException(string invalidReason)
        {
            // Arrange
            var sku = "TEST001";
            var existingProduct = new Product { SKU = sku, Quantity = 10 };
            _mockRepository.Setup(r => r.GetProduct(sku)).Returns(existingProduct);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _warehouseService.WriteOffGoods(sku, 5, invalidReason, DateTime.Now.AddDays(-1))
            );

            Assert.Equal("reason", exception.ParamName);
        }

        /// <summary>
        /// Проверяет, что при списании с датой в будущем выбрасывается исключение
        /// </summary>
        [Fact]
        public void WriteOffGoods_FutureWriteOffDate_ThrowsException()
        {
            // Arrange
            var sku = "TEST001";
            var existingProduct = new Product { SKU = sku, Quantity = 10 };
            _mockRepository.Setup(r => r.GetProduct(sku)).Returns(existingProduct);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _warehouseService.WriteOffGoods(sku, 5, "Damaged", DateTime.Now.AddDays(1))
            );
        }

        /// <summary>
        /// Проверяет, что при списании несуществующего товара выбрасывается исключение
        /// </summary>
        [Fact]
        public void WriteOffGoods_NonExistentProduct_ThrowsException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetProduct("NONEXISTENT")).Returns((Product)null);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _warehouseService.WriteOffGoods("NONEXISTENT", 5, "Damaged", DateTime.Now.AddDays(-1))
            );

            Assert.Contains("not found", exception.Message);
        }

        /// <summary>
        /// Проверяет, что при списании большего количества чем есть выбрасывается исключение
        /// </summary>
        [Fact]
        public void WriteOffGoods_InsufficientQuantity_ThrowsException()
        {
            // Arrange
            var sku = "TEST001";
            var existingProduct = new Product { SKU = sku, Quantity = 5 };
            _mockRepository.Setup(r => r.GetProduct(sku)).Returns(existingProduct);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _warehouseService.WriteOffGoods(sku, 10, "Damaged", DateTime.Now.AddDays(-1))
            );

            Assert.Contains("Insufficient quantity", exception.Message);
        }

        /// <summary>
        /// Проверяет успешную корректировку инвентаря при корректных параметрах
        /// </summary>
        [Fact]
        public void InventoryAdjustment_ValidParameters_UpdatesQuantityAndCreatesOperation()
        {
            // Arrange
            var sku = "TEST001";
            var existingProduct = new Product { SKU = sku, Quantity = 10 };
            _mockRepository.Setup(r => r.GetProduct(sku)).Returns(existingProduct);

            // Act
            _warehouseService.InventoryAdjustment(sku, 15, "Stock count correction", DateTime.Now.AddDays(-1));

            // Assert
            _mockRepository.Verify(r => r.UpdateProduct(It.Is<Product>(p =>
                p.Quantity == 15
            )), Times.Once);

            _mockRepository.Verify(r => r.AddOperation(It.Is<OperationRecord>(o =>
                o.Type == OperationType.InventoryAdjustment &&
                o.Quantity == 15 &&
                o.Reason == "Stock count correction"
            )), Times.Once);
        }

        /// <summary>
        /// Проверяет, что при корректировке на отрицательное количество выбрасывается исключение
        /// </summary>
        [Fact]
        public void InventoryAdjustment_NegativeQuantity_ThrowsException()
        {
            // Arrange
            var sku = "TEST001";
            var existingProduct = new Product { SKU = sku, Quantity = 10 };
            _mockRepository.Setup(r => r.GetProduct(sku)).Returns(existingProduct);

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                _warehouseService.InventoryAdjustment(sku, -5, "Stock count correction", DateTime.Now.AddDays(-1))
            );

            Assert.Equal("actualQuantity", exception.ParamName);
        }

        /// <summary>
        /// Проверяет, что при корректировке с пустой причиной выбрасывается исключение
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void InventoryAdjustment_InvalidReason_ThrowsException(string invalidReason)
        {
            // Arrange
            var sku = "TEST001";
            var existingProduct = new Product { SKU = sku, Quantity = 10 };
            _mockRepository.Setup(r => r.GetProduct(sku)).Returns(existingProduct);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _warehouseService.InventoryAdjustment(sku, 15, invalidReason, DateTime.Now.AddDays(-1))
            );

            Assert.Equal("reason", exception.ParamName);
        }

        /// <summary>
        /// Проверяет, что при корректировке с датой в будущем выбрасывается исключение
        /// </summary>
        [Fact]
        public void InventoryAdjustment_FutureAdjustmentDate_ThrowsException()
        {
            // Arrange
            var sku = "TEST001";
            var existingProduct = new Product { SKU = sku, Quantity = 10 };
            _mockRepository.Setup(r => r.GetProduct(sku)).Returns(existingProduct);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _warehouseService.InventoryAdjustment(sku, 15, "Stock count correction", DateTime.Now.AddDays(1))
            );
        }

        /// <summary>
        /// Проверяет, что при корректировке несуществующего товара выбрасывается исключение
        /// </summary>
        [Fact]
        public void InventoryAdjustment_NonExistentProduct_ThrowsException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetProduct("NONEXISTENT")).Returns((Product)null);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _warehouseService.InventoryAdjustment("NONEXISTENT", 15, "Stock count correction", DateTime.Now.AddDays(-1))
            );

            Assert.Contains("not found", exception.Message);
        }

        /// <summary>
        /// Проверяет корректный расчет общей стоимости инвентаря
        /// </summary>
        [Fact]
        public void GetTotalInventoryValue_WithProducts_ReturnsCorrectValue()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { SKU = "P1", Quantity = 2, PricePerUnit = 10m },
                new Product { SKU = "P2", Quantity = 3, PricePerUnit = 5m }
            };

            _mockRepository.Setup(r => r.GetAllProducts()).Returns(products);

            // Act
            var result = _warehouseService.GetTotalInventoryValue();

            // Assert
            Assert.Equal(35m, result); // (2 * 10) + (3 * 5) = 35
        }

        /// <summary>
        /// Проверяет, что расчет стоимости возвращает 0 при пустом инвентаре
        /// </summary>
        [Fact]
        public void GetTotalInventoryValue_EmptyInventory_ReturnsZero()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllProducts()).Returns(new List<Product>());

            // Act
            var result = _warehouseService.GetTotalInventoryValue();

            // Assert
            Assert.Equal(0m, result);
        }
    }
}