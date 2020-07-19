﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyDemo.Models.Dto;

namespace MyDemo.Controllers
{
    //根目录（P42）
    [Route("api")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = nameof(GetRoot))]
        public IActionResult GetRoot()
        {
            var links = new List<LinkDto>();
            links.Add(new LinkDto(Url.Link(nameof(GetRoot), new { }),
                "self",
                "GET"));
            links.Add(new LinkDto(Url.Link(nameof(CompaniesController.GetCompanies), new { }),
                "companies",
                "GET"));
            links.Add(new LinkDto(Url.Link(nameof(CompaniesController.CreateCompany), new { }),
                "create_company",
                "POST"));
            var resource = new
            {
                links = links
            };
            return Ok(resource);
        }
    }
}