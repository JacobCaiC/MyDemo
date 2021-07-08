using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;


namespace ConsoleApp
{
    /// <summary>
    /// https://zhuanlan.zhihu.com/p/343235838
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            
            Console.WriteLine(ThreadID() + " =》主线程启动");
            MethodAsync(); //调用异步方法
            Console.WriteLine(ThreadID() + " =》主线程继续执行");
            Console.WriteLine(ThreadID() + " =》主线程结束 END");

            Console.Read();
        }

        static async void MethodAsync()
        {
            Console.WriteLine(ThreadID() + " =》MethodAsync方法开始执行");
            string str = await DoSomething(); //等待GetString执行完成 
            Console.WriteLine(ThreadID() + " =》MethodAsync方法执行结束");
        }

        static Task<string> DoSomething()
        {
            Console.WriteLine(ThreadID() + " =》DoSomething方法开始执行");
            return Task<string>.Run(() =>
            {
                Thread.Sleep(4000); //导步线程  处理耗时任务 
                Console.WriteLine(ThreadID() + " =》DoSomething方法结束执行");
                return "GetString的返回值";
            });
        }

        static string ThreadID()
        {
            return Thread.CurrentThread.ManagedThreadId.ToString();
        }


    }
}