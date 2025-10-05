using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab_2.Interface;
using Lab_2.Repository;
using Lab_2.Service;

namespace Warehouse.Front.Services
{
    public static class WarehouseConnector
    {
        private static readonly IWarehouseRepository _repository = new InMemoryWarehouseRepository();
        private static readonly IWarehouseService _service = new WarehouseService(_repository);

        public static IWarehouseService Service => _service;
    }
}
