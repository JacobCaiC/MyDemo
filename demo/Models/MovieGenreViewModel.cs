using Microsoft.AspNetCore.Mvc.Rendering;
using MyDemo.Entities;
using System.Collections.Generic;

namespace MyDemo.Models
{
    /// <summary>
    /// “电影流派”视图模型
    /// </summary>
    public class MovieGenreViewModel
    {
        public List<Movie> Movies { get; set; }
        /// <summary>
        /// 包含流派列表的 SelectList用户能够从列表中选择一种流派
        /// </summary>
        public SelectList Genres { get; set; }
        public string MovieGenre { get; set; }
        public string SearchString { get; set; }
    }
}
