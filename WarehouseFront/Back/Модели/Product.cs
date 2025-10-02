namespace Back.Модели
{
    public class Product
    {
        public string Артикул { get; set; }
        public string Наименование { get; set; }
        public string Описание { get; set; }
        public int Количество { get; set; }
        public decimal Цена { get; set; }
        public string ЕдиницаИзмерения { get; set; }
        public string Поставщик { get; set; }
        public System.DateTime ДатаПоставки { get; set; }
    }
}