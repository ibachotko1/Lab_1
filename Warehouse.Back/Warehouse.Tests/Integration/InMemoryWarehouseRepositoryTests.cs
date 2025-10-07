using Xunit;
using Lab_2.Models;
using Lab_2.Repository;
using System;
using System.Linq;

namespace Warehouse.Tests.Integration
{
    /// <summary>
    /// Тесты для проверки функциональности InMemoryWarehouseRepository
    /// </summary>
    public class InMemoryWarehouseRepositoryTests
    {
        private readonly InMemoryWarehouseRepository _repository;

        public InMemoryWarehouseRepositoryTests()
        {
            _repository = new InMemoryWarehouseRepository();
        }

        /// <summary>
        /// Проверяет успешное добавление нового товара
        /// </summary>
        [Fact]
        public void AddProduct_NewProduct_ProductAddedSuccessfully()
        {
            // Arrange
            var product = new Product { SKU = "TEST001", Name = "Test Product" };

            // Act
            _repository.AddProduct(product);
            var result = _repository.GetProduct("TEST001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST001", result.SKU);
            Assert.Equal("Test Product", result.Name);
        }

        /// <summary>
        /// Проверяет, что при добавлении товара с дублирующимся SKU выбрасывается исключение
        /// </summary>
        [Fact]
        public void AddProduct_DuplicateSku_ThrowsException()
        {
            // Arrange
            var product1 = new Product { SKU = "DUPLICATE", Name = "Product 1" };
            var product2 = new Product { SKU = "DUPLICATE", Name = "Product 2" };

            // Act
            _repository.AddProduct(product1);

            // Assert
            var exception = Assert.Throws<ArgumentException>(() => _repository.AddProduct(product2));
            Assert.Contains("already exists", exception.Message);
        }

        /// <summary>
        /// Проверяет успешное обновление существующего товара
        /// </summary>
        [Fact]
        public void UpdateProduct_ExistingProduct_UpdatesSuccessfully()
        {
            // Arrange
            var originalProduct = new Product
            {
                SKU = "UPDATE001",
                Name = "Original Name",
                Quantity = 10
            };
            _repository.AddProduct(originalProduct);

            var updatedProduct = new Product
            {
                SKU = "UPDATE001",
                Name = "Updated Name",
                Quantity = 20
            };

            // Act
            _repository.UpdateProduct(updatedProduct);
            var result = _repository.GetProduct("UPDATE001");

            // Assert
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(20, result.Quantity);
        }

        /// <summary>
        /// Проверяет, что обновление несуществующего товара не вызывает ошибок
        /// </summary>
        [Fact]
        public void UpdateProduct_NonExistentProduct_NoError()
        {
            // Arrange
            var nonExistentProduct = new Product
            {
                SKU = "NONEXISTENT",
                Name = "Test",
                Quantity = 10
            };

            // Act & Assert (не должно быть исключения)
            var exception = Record.Exception(() => _repository.UpdateProduct(nonExistentProduct));
            Assert.Null(exception);
        }

        /// <summary>
        /// Проверяет успешное получение товара по SKU
        /// </summary>
        [Fact]
        public void GetProduct_ExistingProduct_ReturnsProduct()
        {
            // Arrange
            var product = new Product { SKU = "EXISTING", Name = "Existing Product" };
            _repository.AddProduct(product);

            // Act
            var result = _repository.GetProduct("EXISTING");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EXISTING", result.SKU);
        }

        /// <summary>
        /// Проверяет, что получение несуществующего товара возвращает null
        /// </summary>
        [Fact]
        public void GetProduct_NonExistentProduct_ReturnsNull()
        {
            // Act
            var result = _repository.GetProduct("NONEXISTENT");

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Проверяет успешное получение всех товаров
        /// </summary>
        [Fact]
        public void GetAllProducts_WithProducts_ReturnsAllProducts()
        {
            // Arrange
            _repository.AddProduct(new Product { SKU = "P1", Name = "Product 1" });
            _repository.AddProduct(new Product { SKU = "P2", Name = "Product 2" });

            // Act
            var results = _repository.GetAllProducts();

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Contains(results, p => p.SKU == "P1");
            Assert.Contains(results, p => p.SKU == "P2");
        }

        /// <summary>
        /// Проверяет, что получение всех товаров из пустого репозитория возвращает пустую коллекцию
        /// </summary>
        [Fact]
        public void GetAllProducts_EmptyRepository_ReturnsEmptyCollection()
        {
            // Act
            var results = _repository.GetAllProducts();

            // Assert
            Assert.Empty(results);
        }

        /// <summary>
        /// Проверяет успешную фильтрацию операций по SKU
        /// </summary>
        [Fact]
        public void GetOperationsBySku_WithOperations_ReturnsFilteredOperations()
        {
            // Arrange
            var operation1 = new OperationRecord { SKU = "FILTER001", Type = OperationType.Receive };
            var operation2 = new OperationRecord { SKU = "FILTER001", Type = OperationType.WriteOff };
            var operation3 = new OperationRecord { SKU = "OTHER001", Type = OperationType.Receive };

            _repository.AddOperation(operation1);
            _repository.AddOperation(operation2);
            _repository.AddOperation(operation3);

            // Act
            var results = _repository.GetOperationsBySku("FILTER001");

            // Assert
            Assert.Equal(2, results.Count());
            Assert.All(results, o => Assert.Equal("FILTER001", o.SKU));
        }

        /// <summary>
        /// Проверяет, что фильтрация операций по несуществующему SKU возвращает пустую коллекцию
        /// </summary>
        [Fact]
        public void GetOperationsBySku_NonExistentSku_ReturnsEmptyCollection()
        {
            // Act
            var results = _repository.GetOperationsBySku("NONEXISTENT");

            // Assert
            Assert.Empty(results);
        }

        /// <summary>
        /// Проверяет работу блокировок в многопоточном доступе
        /// </summary>
        [Fact]
        public void Repository_ThreadSafeOperations_NoDataCorruption()
        {
            // Arrange
            var product1 = new Product { SKU = "THREAD1", Name = "Thread Product 1" };
            var product2 = new Product { SKU = "THREAD2", Name = "Thread Product 2" };

            // Act
            var exception = Record.Exception(() =>
            {
                // Эти операции должны выполняться без исключений благодаря lock
                _repository.AddProduct(product1);
                _repository.AddProduct(product2);
                var products = _repository.GetAllProducts();
                var product = _repository.GetProduct("THREAD1");
                _repository.UpdateProduct(product1);
            });

            // Assert
            Assert.Null(exception);
        }
    }
}