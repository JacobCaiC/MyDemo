using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDemo.Services
{
    public interface IOrderService
    {
        int ShowMaxOrderCount();
    }

    public class DisposableOrderService : IOrderService, IDisposable
    {
        public void Dispose()
        { 
            Console.WriteLine($"DisposableOrderService Disposed:{this.GetHashCode()}");
        }

        public int ShowMaxOrderCount()
        {
            throw new NotImplementedException();
        }
    }
}   