using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyDemo.Entities;
using MyDemo.Models.Dto;
using MyDemo.Models.DtoParamaters;
using MyDemo.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marvin.Cache.Headers;

namespace MyDemo.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId}/employees")]
    //[ResponseCache(CacheProfileName = "120sCacheProfile")]  //整个controller允许被缓存120秒（P46）
    public class EmployeesController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public EmployeesController(ICompanyRepository companyRepository, IMapper mapper)
        {
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 通过公司id获取所有员工
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        /// FromQuery(Name = "gender")指定参数名
        [HttpGet(Name = nameof(GetEmployeesForCompany))]
        //单独指定这个方法的缓存策略
        //[ResponseCache(Duration = 60)]                                           //（P46）
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 1800)] //（P48）
        [HttpCacheValidation(MustRevalidate = false)]                              //（P48）
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> 
            GetEmployeesForCompany(Guid companyId, [FromQuery]EmployeeDtoParameters parameters) 
             {

            if (await _companyRepository.CompanyExistsAsync(companyId))
            {
                var employees = await _companyRepository
                    .GetEmployeesAsync(companyId, parameters);
                var employeeDtos = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
                return Ok(employeeDtos);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// 通过公司id和员工id获取
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [HttpGet("{employeeId}", Name = nameof(GetEmployeeForCompany))]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            if (await _companyRepository.CompanyExistsAsync(companyId))
            {
                var employee = await _companyRepository.GetEmployeeAsync(companyId, employeeId);
                if (employee == null)
                {
                    return NotFound();
                }
                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return Ok(employeeDto);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// 公司创建职员
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost(Name = nameof(CreateEmployeeForCompany))]
        public async Task<IActionResult> CreateEmployeeForCompany([FromRoute]Guid companyId,
                                                                  [FromBody]EmployeeAddDto employeeAddDto)
        //此处的 [FromRoute] 与 [FromBody] 其实不指定也可以，会自动匹配
        {
            if (!await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }
            var entity = _mapper.Map<Employee>(employeeAddDto);
            _companyRepository.AddEmployee(companyId, entity);
            await _companyRepository.SaveAsync();
            var returnDto = _mapper.Map<EmployeeDto>(entity);
            return CreatedAtAction(nameof(GetEmployeeForCompany),
                                    new { companyId = returnDto.CompanyId, employeeId = returnDto.Id },
                                    returnDto);
        }

        /// <summary>
        /// 整体更新/替换，PUT不是安全的，但是幂等
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="employeeId"></param>
        /// <param name="employeeUpdateDto"></param>
        /// <returns></returns>
        [HttpPut("{employeeId}")]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId,
                                                                  Guid employeeId,
                                                                  EmployeeUpdateDto employeeUpdateDto)
        {
            if (!await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }         

            var employeeEntity = await _companyRepository.GetEmployeeAsync(companyId, employeeId);
            // put请求新增
            if (employeeEntity == null)
            {
                //不允许客户端生成 Guid
                //return NotFound();

                //允许客户端生成 Guid
                var employeeToAddEntity = _mapper.Map<Employee>(employeeUpdateDto);
                employeeToAddEntity.Id = employeeId;
                _companyRepository.AddEmployee(companyId, employeeToAddEntity);
                await _companyRepository.SaveAsync();
                var returnDto = _mapper.Map<EmployeeDto>(employeeToAddEntity);
                return CreatedAtAction(nameof(GetEmployeeForCompany),
                                        new { companyId = companyId, employeeId = employeeId },
                                        returnDto);
            }

            // entity转化为updateDto
            // 把传进来的employee更新到updateDto
            // 把 updateDto 映射到 entity
            _mapper.Map(employeeUpdateDto, employeeEntity);
            _companyRepository.UpdateEmployee(employeeEntity);
            await _companyRepository.SaveAsync();
            return NoContent(); //返回状态码204
        }

        /// <summary>
        /// HTTP PATCH 举例（P32）
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="employeeId"></param>
        /// <param name="patchDocument"></param>
        /// <returns></returns>
        [HttpPatch("{employeeId}")]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(
            Guid companyId,
            Guid employeeId,
            JsonPatchDocument<EmployeeUpdateDto> patchDocument)
        {
            if (!await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var employeeEntity = await _companyRepository.GetEmployeeAsync(companyId, employeeId);
            //patch新增
            if (employeeEntity == null)
            {
                //不允许客户端生成 Guid
                //return NotFound();

                //允许客户端生成 Guid
                var employeeDto = new EmployeeUpdateDto();
                //传入 ModelState 进行验证
                patchDocument.ApplyTo(employeeDto, ModelState);
                if (!TryValidateModel(employeeDto))
                {
                    return ValidationProblem(ModelState);
                }

                var employeeToAdd = _mapper.Map<Employee>(employeeDto);
                employeeToAdd.Id = employeeId;
                _companyRepository.AddEmployee(companyId, employeeToAdd);
                await _companyRepository.SaveAsync();
                var dtoToReturn = _mapper.Map<Employee>(employeeToAdd);

                return CreatedAtAction(nameof(GetEmployeeForCompany),
                                    new { companyId = companyId, employeeId = employeeId },
                                    dtoToReturn);
            }

            var dtoToPatch = _mapper.Map<EmployeeUpdateDto>(employeeEntity);
            //将 Patch 应用到 dtoToPatch（EmployeeUpdateDto）
            patchDocument.ApplyTo(dtoToPatch);
            //验证模型
            if (!TryValidateModel(dtoToPatch))
            {
                return ValidationProblem(ModelState); //返回状态码与错误信息
                //return new UnprocessableEntityObjectResult(ModelState);
            }
            _mapper.Map(dtoToPatch, employeeEntity);
            _companyRepository.UpdateEmployee(employeeEntity);
            await _companyRepository.SaveAsync();
            return NoContent(); //返回状态码204
        }

        /// <summary>
        /// 删除公司员工
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [HttpDelete("{employeeId}")]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            if (!await _companyRepository.CompanyExistsAsync(companyId))
            {
                return NotFound();
            }

            var employeeEntity = await _companyRepository.GetEmployeeAsync(companyId, employeeId);
            if (employeeEntity == null)
            {
                return NotFound();
            }

            _companyRepository.DeleteEmployee(employeeEntity);
            await _companyRepository.SaveAsync();
            return NoContent();
        }

        /// <summary>
        /// 重写 ValidationProblem
        /// 使 PartiallyUpdateEmployeeForCompany 中的 ValidationProblem() 返回状态码422而不是400
        /// </summary>
        /// <param name="modelStateDictionary"></param>
        /// <returns></returns>
        public override ActionResult ValidationProblem(ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}