using System;

namespace Back.Модели
{
    public class OperationRecord
    {
        public int Id { get; set; }
        public string Артикул { get; set; }
        public int Количество { get; set; }
        public decimal Цена { get; set; }
        public string Описание { get; set; }
        public DateTime ДатаОперации { get; set; }
        public OperationType ТипОперации { get; set; }
    }
}