using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyDemo.Entities
{
    public class Movie
    {
        public int Id { get; set; }

        [Display(Name = "标题")]
        [StringLength(60, MinimumLength = 3)]
        [Required]
        public string Title { get; set; }

        /// <summary>
        ///  Display特性指定要显示的字段名称的内容
        ///  DataType属性指定数据的类型（日期）
        ///  DisplayFormat特性用于显式指定日期格式
        ///  ApplyFormatInEditMode设置指定在文本框中显示值以进行编辑时也应用格式
        /// </summary>
        [Display(Name = "发行日期"), DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "类型"),Required,StringLength(30)]
        //[RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        public string Genre { get; set; }

        [Display(Name = "价格"),Range(1, 100), DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Display(Name = "分级"),RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$"),StringLength(5),Required]
        public string Rating { get; set; }
    }
}
