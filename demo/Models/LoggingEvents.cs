using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDemo.Models
{
    public class LoggingEvents
    {
        public const int GenerateItems = 1000;
        public const string GenerateItemsString = "生成";
        public const int ListItems = 1001;
        public const string ListItemsString = "列表";
        public const int GetItem = 1002;
        public const string GetItemString = "获取";
        public const int InsertItem = 1003;
        public const string InsertItemString = "插入";
        public const int UpdateItem = 1004;
        public const string UpdateItemString = "更新";
        public const int DeleteItem = 1005;
        public const string DeleteItemString = "删除";
        public const int GetItemNotFound = 4000;
        public const string GetItemNotFoundString = "获取未找到";
        public const int UpdateItemNotFound = 4001;
        public const string UpdateItemNotFoundString = "更新未找到";
    }
}