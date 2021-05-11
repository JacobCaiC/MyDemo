using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Magicodes.ExporterAndImporter.Attributes;
using MyDemo.Models.Dto;
using MyDemo.Models.DtoParamaters;

namespace MyDemo.Controllers
{
    [ApiController]
    [Route("api/MagiCodes")]
    public class MagiCodesController : ControllerBase
    {

        [HttpGet(Name = nameof(GetTask))]
        public async Task<IActionResult> GetTask()
        {

            // return Ok(new CompanyDto()
            // {
            //     CompanyName = "Jacob",
            //     Country = "China"
            // });
            return StatusCode(200, new CompanyDto()
            {
                CompanyName = "Jacob",
                Country = "China"
            });
        }

        /// <summary>
        /// export word
        /// </summary>
        /// <returns></returns>
        [HttpGet("Word")]
        [Magicodes(Type = typeof(ReceiptInfo), TemplatePath = ".//ExportTemplates//receipt.cshtml")]
        public ReceiptInfo Word()
        {
            return new ReceiptInfo
            {
                Amount = 22939.43M,
                Grade = "2021Summer",
                IdNo = "43062619890622xxxx",
                Name = "JacobCai",
                Payee = "科技有限公司",
                PaymentMethod = "微信支付",
                Profession = "运动训练",
                Remark = "学费",
                TradeStatus = "已完成",
                TradeTime = DateTime.Now,
                UppercaseAmount = "贰万贰仟玖佰叁拾玖圆肆角叁分",
                Code = "19071800001"
            };
        }


        /// <summary>
        /// export Html
        /// </summary>
        /// <returns></returns>
        [HttpGet("Html")]
        [Magicodes(Type = typeof(ReceiptInfo), TemplatePath = ".//ExportTemplates//receipt.cshtml")]
        public ReceiptInfo Html()
        {
            return new ReceiptInfo
            {
                Amount = 22939.43M,
                Grade = "2021Summer",
                IdNo = "43062619890622xxxx",
                Name = "JacobCai",
                Payee = "科技有限公司",
                PaymentMethod = "微信支付",
                Profession = "运动训练",
                Remark = "学费",
                TradeStatus = "已完成",
                TradeTime = DateTime.Now,
                UppercaseAmount = "贰万贰仟玖佰叁拾玖圆肆角叁分",
                Code = "19071800001"
            };
        }


        /// <summary>
        /// export pdf
        /// </summary>
        /// <returns></returns>
        [HttpGet("pdf")]
        [Magicodes(Type = typeof(ReceiptInfo), TemplatePath = ".//ExportTemplates//receipt.cshtml")]
        public ReceiptInfo Pdf()
        {
            return new ReceiptInfo
            {
                Amount = 22939.43M,
                Grade = "2021Summer",
                IdNo = "43062619890622xxxx",
                Name = "JacobCai",
                Payee = "科技有限公司",
                PaymentMethod = "微信支付",
                Profession = "运动训练",
                Remark = "学费",
                TradeStatus = "已完成",
                TradeTime = DateTime.Now,
                UppercaseAmount = "贰万贰仟玖佰叁拾玖圆肆角叁分",
                Code = "19071800001"
            };
        }
    }
}
