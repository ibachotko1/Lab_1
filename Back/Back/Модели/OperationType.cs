using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back.Модели
{
    /// <summary>
    /// Типы операций на складе
    /// </summary>
    public enum OperationType
    {
        Receiving,    // Приемка
        WriteOff,     // Списание
        Inventory     // Инвентаризация
    }
}
