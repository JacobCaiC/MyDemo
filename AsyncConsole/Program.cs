using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncConsole
{
    /// <summary>
    /// https://www.jb51.net/article/158544.htm
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(ThreadID() + "=> Hello World!");
            Console.WriteLine(ThreadID() + "=>" + $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：开始");
            // 调用同步方法
            //SyncTestMethod();
            // 调用异步步方法
            AsyncTestMethod().Wait();
            Console.WriteLine(ThreadID() + "=>" + $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：结束");

            // Console.WriteLine($"sync{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：开始");
            // var a = Method1();
            // var b = Method200ms();
            // var c = Method500ms(a);
            // var d = Method1000ms();
            // var result = a + b + c + d;
            // Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：最后得到的结果{result}");
            // Console.WriteLine($"sync{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：结束");

            Console.WriteLine(ThreadID() + "=>" + $"async{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：开始");
            var m1 = AsyncMethod1();
            var m2 = AsyncMethod200ms();
            var m4 = AsyncMethod1000ms();
            m1.Wait();
            var m3 = AsyncMethod500ms(m1.Result);
            m2.Wait();
            m3.Wait();
            m4.Wait();
            var result = m1.Result + m2.Result + m3.Result + m4.Result;
            Console.WriteLine(ThreadID() + "=>" + $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：最后得到的结果{result}");
            Console.WriteLine(ThreadID() + "=>" + $"async{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：结束");

            Console.ReadKey();
        }

        static string ThreadID()
        {
            return Thread.CurrentThread.ManagedThreadId.ToString();
        }

        /// <summary>
        /// 同步方法
        /// </summary>
        static void SyncTestMethod()
        {
            for (int i = 0; i < 10; i++)
            {
                var str = ThreadID() + "=>" + $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}:SyncTestMethod{i}";
                Console.WriteLine(str);
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 异步方法
        /// </summary>
        /// <returns></returns>
        static async Task AsyncTestMethod()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine(ThreadID() + "=>AsyncTestMethod");
                    Thread.Sleep(10);
                }
            });
        }

        static int Method1()
        {
            Thread.Sleep(200);
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：我计算了一个值耗费200ms");
            return 1;
        }

        static int Method200ms()
        {
            Thread.Sleep(200);
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：我做了一件耗费200ms的事情");
            return 200;
        }

        static int Method500ms(int index)
        {
            Thread.Sleep(500);
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：我做了一件耗费500ms的事情");
            return ++index;
        }

        static int Method1000ms()
        {
            Thread.Sleep(1000);
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：我做了一件耗费1000ms的事情");
            return 1000;
        }

        static async Task<int> AsyncMethod1()
        {
            await Task.Run(() =>
            {
                Thread.Sleep(200);
                Console.WriteLine(ThreadID() + "=>" +
                                  $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：我计算了一个值耗费200ms");
            });
            return 1;
        }

        static async Task<int> AsyncMethod200ms()
        {
            await Task.Run(() =>
            {
                Thread.Sleep(200);
                Console.WriteLine(ThreadID() + "=>" +
                                  $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：我做了一件耗费200ms的事情");
            });
            return 200;
        }

        static async Task<int> AsyncMethod500ms(int index)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(500);
                Console.WriteLine(ThreadID() + "=>" +
                                  $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：我做了一件耗费500ms的事情");
            });
            return ++index;
        }

        static async Task<int> AsyncMethod1000ms()
        {
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                Console.WriteLine(ThreadID() + "=>" +
                                  $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ms")}：我做了一件耗费1000ms的事情");
            });
            return 1000;
        }
    }
}
