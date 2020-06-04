using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDemo.Models.Response
{
    public class ApiResponse
    {
        public string dataJson { get; set; } = string.Empty;
        public object data { get; set; }
        public MessageInfo message { get; set; } = new MessageInfo();

        public string messageinfo { get; set; } = string.Empty;
        public string result { get; set; } = string.Empty;
        public bool success { get; set; } = false;
    }

    public class MessageInfo
    {
        public int code { get; set; } = 0;
        public string message { get; set; } = string.Empty;
    }
}

