using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDemo.Services
{
    public class OrderService:IOrderService
    {
        private OrderServiceOptions _options;

        public OrderService(OrderServiceOptions options)
        {
            this._options = options;
        }

        public int ShowMaxOrderCount()
        {
            return _options.MaxOrderCount;
        }
    }

    public class OrderServiceOptions
    {
        public int MaxOrderCount { get; set; } = 100;
    }

}
