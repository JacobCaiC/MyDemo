using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDemo.Services
{
    public interface IPropertyMappingService
    {
        /// <summary>
        /// 如果TSource 与 TDestination 存在映射关系，返回映射关系字典
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns></returns>
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();

        /// <summary>
        /// 客户端提交的 Uri query string 中的 orderby 是否合法（P38）
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="fields"></param>
        /// <returns></returns>
        bool ValidMappingExistsFor<TSource, TDestination>(string fields);
    }
}
