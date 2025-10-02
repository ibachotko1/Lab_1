using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagement.Core.Модели
{
    /// <summary>
    /// Модель товара на складе
    /// </summary>
    public class Product
    {
        /// <summary>Уникальный артикул товара</summary>
        public string SKU { get; set; }

        /// <summary>Наименование товара</summary>
        public string Name { get; set; }

        /// <summary>Текущее количество на складе</summary>
        public int Quantity { get; set; }

        /// <summary>Цена за единицу товара</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Поставщик товара</summary>
        public string Supplier { get; set; }

        /// <summary>Дата последней поставки</summary>
        public DateTime LastDeliveryDate { get; set; }
    }
}
