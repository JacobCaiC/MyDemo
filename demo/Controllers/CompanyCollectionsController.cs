using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyDemo.Entities;
using MyDemo.Helpers;
using MyDemo.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyDemo.Services;

namespace MyDemo.Controllers
{
    [ApiController]
    [Route("api/companycollections")]
    public class CompanyCollectionsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICompanyRepository _companyRepository;

        public CompanyCollectionsController(IMapper mapper, ICompanyRepository companyRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _companyRepository = companyRepository ??
                                    throw new ArgumentNullException(nameof(companyRepository));
        }

        /// <summary>
        /// 传递多个key:1,2,3,4
        /// key1=value1,key2=value2
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpGet("({ids})", Name = nameof(GetCompanyCollection))]
        public async Task<IActionResult> GetCompanyCollection([FromRoute]
                                                              [ModelBinder(BinderType = typeof(ArrayModelBinder))]
                                                              IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }
            var entities = await _companyRepository.GetCompaniesAsync(ids);

            if (ids.Count() != entities.Count())
            {
                return NotFound();
            }

            var dtosToReturn = _mapper.Map<IEnumerable<CompanyDto>>(entities);
            return Ok(dtosToReturn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyCollection"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCompanyCollection(
            IEnumerable<CompanyAddDto> companyCollection) //Task<IActionResult> = Task<ActionResult<IEnumerable<CompanyDto>>>
        {
            //此方法没有对 Employees 进行模型验证
            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var company in companyEntities)
            {
                _companyRepository.AddCompany(company);
            }
            await _companyRepository.SaveAsync();
            var dtosToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var idsString = string.Join(",", dtosToReturn.Select(x => x.Id));
            return CreatedAtRoute(nameof(GetCompanyCollection), new { ids = idsString }, dtosToReturn);
            //return Ok();
        }
    }
}