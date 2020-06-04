namespace MyDemo.Models.DtoParamaters
{
    public class CompanyDtoParameters
    {
        private const int MaxPageSize = 20; //翻页（P34-35）
        public string CompanyName { get; set; }
        public string SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1; //默认值为1
        public string OrderBy { get; set; } = "CompanyName"; //默认用公司名字排序P36-38
        public string Fields { get; set; } //数据塑形（P39）

        private int _pageSize = 5;

        // 设置页面最大条数
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize ? MaxPageSize : value);
        }
    }
}
