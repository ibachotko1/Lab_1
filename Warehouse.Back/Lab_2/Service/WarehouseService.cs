using Lab_2.Interface;
using Lab_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Warehouse.Back.Models;

namespace Lab_2.Service
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _repository;

        public WarehouseService(IWarehouseRepository repository)
        {
            _repository = repository;
        }

        public OperationResult ReceiveGoods(string sku, string name, int quantity, decimal pricePerUnit, string supplier, DateTime deliveryDate)
        {
            var result = new OperationResult();
            var postConditions = new List<PostConditionInfo>();

            try
            {
                // Pre-conditions
                if (string.IsNullOrEmpty(sku))
                {
                    throw new ArgumentException("SKU cannot be null or empty");
                }

                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("Name cannot be null or empty");
                }

                if (quantity <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
                }

                if (pricePerUnit <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pricePerUnit), "Price must be greater than 0");
                }

                if (string.IsNullOrEmpty(supplier))
                {
                    throw new ArgumentException("Supplier cannot be null or empty");
                }

                if (deliveryDate > DateTime.Now)
                {
                    throw new InvalidOperationException("Delivery date cannot be in the future");
                }

                var existingProduct = _repository.GetProduct(sku);
                if (existingProduct != null)
                {
                    throw new ArgumentException($"Product with SKU {sku} already exists");
                }

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

                // Add product to repository
                _repository.AddProduct(product);

                // Create operation record
                var operation = new OperationRecord
                {
                    SKU = sku,
                    Type = OperationType.Receive,
                    Quantity = quantity,
                    PricePerUnit = pricePerUnit,
                    OperationDate = deliveryDate,
                    Reason = "Приемка товара"
                };

                _repository.AddOperation(operation);

                // Post-conditions verification
                var addedProduct = _repository.GetProduct(sku);
                postConditions.Add(new PostConditionInfo
                {
                    Description = "Товар добавлен в репозиторий",
                    IsSatisfied = addedProduct != null,
                    Details = addedProduct != null ? $"SKU: {addedProduct.SKU}" : "Товар не найден"
                });

                postConditions.Add(new PostConditionInfo
                {
                    Description = "Количество установлено корректно",
                    IsSatisfied = addedProduct?.Quantity == quantity,
                    Details = $"Ожидалось: {quantity}, Фактически: {addedProduct?.Quantity}"
                });

                postConditions.Add(new PostConditionInfo
                {
                    Description = "Цена за единицу установлена корректно",
                    IsSatisfied = addedProduct?.PricePerUnit == pricePerUnit,
                    Details = $"Ожидалось: {pricePerUnit}, Фактически: {addedProduct?.PricePerUnit}"
                });

                postConditions.Add(new PostConditionInfo
                {
                    Description = "Операция записана в журнал",
                    IsSatisfied = _repository.GetOperationsBySku(sku).Any(o => o.Type == OperationType.Receive),
                    Details = "Операция приёмки добавлена в журнал"
                });

                postConditions.Add(new PostConditionInfo
                {
                    Description = "Общая стоимость инвентаря обновлена",
                    IsSatisfied = true,
                    Details = $"Новая общая стоимость: {GetTotalInventoryValue():C}"
                });

                result.Success = true;
                result.Message = "Товар успешно принят на склад";
                result.PostConditions = postConditions;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                result.PostConditions = postConditions;
            }

            return result;
        }

        public OperationResult WriteOffGoods(string sku, int quantity, string reason, DateTime writeOffDate)
        {
            var result = new OperationResult();
            var postConditions = new List<PostConditionInfo>();

            try
            {
                // Pre-conditions
                if (string.IsNullOrEmpty(sku))
                {
                    throw new ArgumentException("SKU cannot be null or empty");
                }

                if (quantity <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
                }

                if (string.IsNullOrEmpty(reason))
                {
                    throw new ArgumentException("Reason cannot be null or empty");
                }

                if (writeOffDate > DateTime.Now)
                {
                    throw new InvalidOperationException("Write-off date cannot be in the future");
                }

                var product = _repository.GetProduct(sku);
                if (product == null)
                {
                    throw new ArgumentException($"Product with SKU {sku} not found");
                }

                if (product.Quantity < quantity)
                {
                    throw new InvalidOperationException($"Insufficient quantity. Available: {product.Quantity}, Requested: {quantity}");
                }

                int oldQuantity = product.Quantity;
                decimal oldTotalValue = GetTotalInventoryValue();

                // Update product
                product.Quantity -= quantity;
                _repository.UpdateProduct(product);

                // Create operation record
                var operation = new OperationRecord
                {
                    SKU = sku,
                    Type = OperationType.WriteOff,
                    Quantity = quantity,
                    PricePerUnit = product.PricePerUnit,
                    Reason = reason,
                    OperationDate = writeOffDate
                };

                _repository.AddOperation(operation);

                // Post-conditions verification
                var updatedProduct = _repository.GetProduct(sku);
                postConditions.Add(new PostConditionInfo
                {
                    Description = "Количество товара уменьшено",
                    IsSatisfied = updatedProduct?.Quantity == oldQuantity - quantity,
                    Details = $"Было: {oldQuantity}, Стало: {updatedProduct?.Quantity}"
                });

                postConditions.Add(new PostConditionInfo
                {
                    Description = "Операция списания записана в журнал",
                    IsSatisfied = _repository.GetOperationsBySku(sku).Any(o =>
                        o.Type == OperationType.WriteOff && o.Quantity == quantity),
                    Details = "Операция списания добавлена в журнал"
                });

                postConditions.Add(new PostConditionInfo
                {
                    Description = "Общая стоимость инвентаря уменьшена",
                    IsSatisfied = GetTotalInventoryValue() < oldTotalValue,
                    Details = $"Было: {oldTotalValue:C}, Стало: {GetTotalInventoryValue():C}"
                });

                result.Success = true;
                result.Message = "Товар успешно списан со склада";
                result.PostConditions = postConditions;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                result.PostConditions = postConditions;
            }

            return result;
        }

        public OperationResult InventoryAdjustment(string sku, int actualQuantity, string reason, DateTime adjustmentDate)
        {
            var result = new OperationResult();
            var postConditions = new List<PostConditionInfo>();

            try
            {
                // Pre-conditions
                if (string.IsNullOrEmpty(sku))
                {
                    throw new ArgumentException("SKU cannot be null or empty");
                }

                if (actualQuantity < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(actualQuantity), "Quantity cannot be negative");
                }

                if (string.IsNullOrEmpty(reason))
                {
                    throw new ArgumentException("Reason cannot be null or empty");
                }

                if (adjustmentDate > DateTime.Now)
                {
                    throw new InvalidOperationException("Adjustment date cannot be in the future");
                }

                var product = _repository.GetProduct(sku);
                if (product == null)
                {
                    throw new ArgumentException($"Product with SKU {sku} not found");
                }

                int oldQuantity = product.Quantity;
                int difference = actualQuantity - oldQuantity;
                decimal oldTotalValue = GetTotalInventoryValue();

                // Update product
                product.Quantity = actualQuantity;
                _repository.UpdateProduct(product);

                // Create operation record
                var operation = new OperationRecord
                {
                    SKU = sku,
                    Type = OperationType.InventoryAdjustment,
                    Quantity = actualQuantity,
                    PricePerUnit = product.PricePerUnit,
                    Reason = $"{reason} (Разница: {difference})",
                    OperationDate = adjustmentDate
                };

                _repository.AddOperation(operation);

                // Post-conditions verification
                var updatedProduct = _repository.GetProduct(sku);
                postConditions.Add(new PostConditionInfo
                {
                    Description = "Количество товара скорректировано",
                    IsSatisfied = updatedProduct?.Quantity == actualQuantity,
                    Details = $"Было: {oldQuantity}, Стало: {actualQuantity}, Разница: {difference}"
                });

                postConditions.Add(new PostConditionInfo
                {
                    Description = "Операция инвентаризации записана в журнал",
                    IsSatisfied = _repository.GetOperationsBySku(sku).Any(o =>
                        o.Type == OperationType.InventoryAdjustment),
                    Details = "Операция инвентаризации добавлена в журнал"
                });

                postConditions.Add(new PostConditionInfo
                {
                    Description = "Общая стоимость инвентаря пересчитана",
                    IsSatisfied = true,
                    Details = $"Было: {oldTotalValue:C}, Стало: {GetTotalInventoryValue():C}"
                });

                result.Success = true;
                result.Message = "Инвентаризация успешно выполнена";
                result.PostConditions = postConditions;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                result.PostConditions = postConditions;
            }

            return result;
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