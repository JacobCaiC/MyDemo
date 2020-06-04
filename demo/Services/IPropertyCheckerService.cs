namespace MyDemo.Services
{
    public interface IPropertyCheckerService
    {
        /// <summary>
        /// fields 字符串是否合法（P39）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <returns></returns>
        bool TypeHasProperties<T>(string fields);
    }
}