using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDemo.Helpers
{
    /// <summary>
    /// 需要生成的翻页 URI 是上一页还是下一页（P35）
    /// </summary>
    public enum ResourceUriType
    {
        PreviousPage,
        NextPage,
        CurrentPage
    }
}
