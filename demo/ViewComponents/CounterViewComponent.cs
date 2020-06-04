using System;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace MyDemo.ViewComponents
{

    public class CounterViewComponent : ViewComponent
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public CounterViewComponent(IConnectionMultiplexer redis)
        {
            //_redis = redis;
            _db = redis.GetDatabase();
        }

        /// <summary>
        /// 计数
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var controller = this.RouteData.Values["controller"] as string;
            var action = RouteData.Values["action"] as string;
            if (!string.IsNullOrWhiteSpace(controller) && !string.IsNullOrWhiteSpace(action))
            {
                var pageId = $"{controller}-{action}";
                await _db.StringIncrementAsync(pageId);
                var count = await _db.StringGetAsync(pageId);
                return View("Default",pageId+":"+count);
            }
            throw new Exception("cannot get pageid");

        }

    }
}
