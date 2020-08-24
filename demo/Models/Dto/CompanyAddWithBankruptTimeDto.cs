using System;
using System.ComponentModel.DataAnnotations;

namespace MyDemo.Models.Dto
{
    //继承自 CompanyAddDto，增加 BankruptTime 列

    /// <summary>
    /// Create Company 时使用的 Dto，增加了 BankruptTime 属性（P44）
    /// </summary>
    public class CompanyAddWithBankruptTimeDto : CompanyAddDto
    {
        [Display(Name = "破产时间")]
        public DateTime? BankruptTime { get; set; }
    }
}
