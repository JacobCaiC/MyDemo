using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyDemo.Models;

namespace MyDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NLogController : ControllerBase
    {
        private readonly ILogger _logger;

        public NLogController(ILogger<NLogController> logger)
        {
            _logger = logger;
        }

        [HttpGet("info/{id}")]
        public void Info(string id)
        {
            _logger.LogInformation(new EventId(LoggingEvents.GetItem, LoggingEvents.GetItemString),
                "Getting item {ID}", id);
        }


        [HttpGet("error/{id}")]
        public void Error(string id)
        {
            //_logger.LogError(LoggingEvents.GenerateItems, "Generate items {ID}", id);
            _logger.LogError(new EventId(LoggingEvents.GenerateItems, LoggingEvents.GenerateItemsString),
                "错误测试");
        }

        [HttpGet("Warn/{id}")]
        public void Warn()
        {
            _logger.LogWarning(new EventId(LoggingEvents.GetItemNotFound, LoggingEvents.GetItemNotFoundString),
                "警告信息！因程序出现故障或其他不会导致程序停止的流程异常或意外事件。");
        }

        [HttpGet("Critical/{id}")]
        public void Critical()
        {
            //_logger.LogTrace("开发阶段调试，可能包含敏感程序数据", 1);
            //_logger.LogDebug("开发阶段短期内比较有用，对调试有益。");
            //_logger.LogInformation("你访问了首页。跟踪程序的一般流程。");
            //_logger.LogWarning("警告信息！因程序出现故障或其他不会导致程序停止的流程异常或意外事件。");
            //_logger.LogError("错误信息。因某些故障停止工作");
            _logger.LogCritical(new EventId(LoggingEvents.DeleteItem, LoggingEvents.DeleteItemString),
                "程序或系统崩溃、遇到灾难性故障！！！");
        }
    }
}