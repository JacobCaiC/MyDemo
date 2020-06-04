using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyDemo.Entities;
using MyDemo.Helpers;
using MyDemo.Models.Dto;
using MyDemo.Models.DtoParamaters;
using MyDemo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;


namespace MyDemo.Controllers
{
    /// <summary>
    /// Controller继承ControllerBase，增加视图支持
    /// [ApiController] 属性并不是强制要求的，但是它会使开发体验更好
    /// 它会启用以下行为：
    /// 1.要求使用属性路由（Attribute Routing）2.自动HTTP 400响应 
    /// 3.推断参数的绑定源 4.Multipart/form-data请求推断 5.错误状态代码的问题详细信息
    /// </summary>
    [ApiController]
    [Route("api/companies")]
    //[Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public CompaniesController(ICompanyRepository companyRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        /// <summary>
        /// 查询所有企业;Head 请求只会返回 Header 信息，没有 Body
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = nameof(GetCompanies))]
        [HttpHead] //即支持GET又支持HEAD,GET返回状态码body,添加对 Http Head 的支持，Head 请求只会返回 Header 信息，没有 Body（P16）
        public async Task<IActionResult> GetCompanies(
            [FromQuery]CompanyDtoParameters parameters)
        {
            //判断Uri query 字符串中的 orderby 是否合法（P38）
            if (!_propertyMappingService.ValidMappingExistsFor<CompanyDto, Company>(parameters.OrderBy))
            {
                return BadRequest();  //返回状态码400
            }

            //判断Uri query 字符串中的 fields 是否合法（P39）
            if (!_propertyCheckerService.TypeHasProperties<CompanyDto>(parameters.Fields))
            {
                return BadRequest();  //返回状态码400
            }

            //GetCompaniesAsync(parameters) 返回的是经过翻页处理的 PagedList<T>（P35）
            var companies = await _companyRepository.GetCompaniesAsync(parameters);

            ////向 Header 中添加翻页信息（P35）
            ////上一页的 URI
            //var previousPageLink = companies.HasPrevious
            //    ? CreateCompaniesResourceUri(parameters, ResourceUriType.PreviousPage)
            //    : null;
            ////下一页的 URI
            //var nextPageLink = companies.HasNext
            //    ? CreateCompaniesResourceUri(parameters, ResourceUriType.NextPage)
            //    : null;


            //页面元数据
            var paginationMetdata = new
            {
                totalCount = companies.TotalCount,
                pageSize = companies.PageSize,
                currentPage = companies.CurrentPage,
                totalPages = companies.TotalPages
            };
            // 自定义header
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetdata,
                new JsonSerializerOptions //URI 中的‘&’、‘？’符号不应该被转义，因此改用不安全的 Encoder
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));

            //使用 AutoMapper（P12）
            var companyDtos = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            //不使用 AutoMapper
            //var companyDtos = new List<CompanyDto>();
            //foreach (var company in companies)
            //{
            //    companyDtos.Add(new CompanyDto
            //    {
            //        Id = company.Id,
            //        Name = company.Name
            //    });
            //}

            var shapedData = companyDtos.ShapeData(parameters.Fields);
            var links = CreateLinksForCompany(parameters, companies.HasPrevious, companies.HasNext);
            //{value:[xxx],links}
            //首先创建 Companies 集合中每个 Company 自己的 Links
            var shapedCompaniesWithLinks = shapedData.Select(c =>
            {
                var companyDict = c as IDictionary<string, object>;
                var links = CreateLinksForCompany((Guid)companyDict["Id"], null);
                companyDict.Add("links", links);
                return companyDict;
            });
            //然后创建整个 Companies 集合的 Links
            var linkedCollectionResource = new
            {
                value = shapedCompaniesWithLinks,
                links = CreateLinksForCompany(parameters, companies.HasPrevious, companies.HasNext)
            };
            return Ok(linkedCollectionResource);  //返回状态码200
            //return Ok(companyDtos.ShapeData(parameters.Fields));
            //return new JsonResult(companies);
        }

        /// <summary>
        /// 由id获取企业
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet("{companyId}", Name = nameof(GetCompany))]
        public async Task<IActionResult> GetCompany(Guid companyId,
            string fields,
            [FromHeader(Name="Accept")] string acceptMediaType) //从header把accept取出来赋给acceptMediaType
        {
            //var exist = await _companyRepository.CompanyExistsAsync(companyId);

            //判断Uri query 字符串中的 fields 是否合法（P39）
            if (!_propertyCheckerService.TypeHasProperties<CompanyDto>(fields))
            {
                return BadRequest();  //返回状态码400
            }

            //尝试解析 MediaTypeHeaderValue（P43）
            if (!MediaTypeHeaderValue.TryParse(acceptMediaType, out MediaTypeHeaderValue parsedAcceptMediaType))
            {
                //解析不成功
                return BadRequest();
            }

            var company = await _companyRepository.GetCompanyAsync(companyId);
            if (company == null)
            {
                return NotFound();
            }

            var links = CreateLinksForCompany(companyId, fields);

            var linkedDict = _mapper.Map<CompanyDto>(company).ShapeData(fields)
                as IDictionary<string, object>;
            linkedDict.Add("links",links);

            return Ok(linkedDict);
        }

        /// <summary>
        /// CompanyAddDto创建公司 
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        [HttpPost(Name = nameof(CreateCompany))]
        public async Task<IActionResult> CreateCompany([FromBody]CompanyAddDto companyAddDto){   //Task<IActionResult> = Task<ActionResult<CompanyDto>
            //使用 [ApiController] 属性后，会自动返回400错误，无需再使用以下代码：
            //if (!ModelState.IsValid)
            //{
            //    return UnprocessableEntity(ModelState);
            //}

            //Core 3.x 使用 [ApiController] 属性后，无需再使用以下代码：
            //if (company == null)
            //{
            //    return BadRequest(); //返回状态码400
            //}

            var entity = _mapper.Map<Company>(companyAddDto);
            _companyRepository.AddCompany(entity);
            //controller不需要知道repository细节
            await _companyRepository.SaveAsync();
            var returnDto = _mapper.Map<CompanyDto>(entity);

            var links = CreateLinksForCompany(returnDto.Id, null);
            var linkedDict = returnDto.ShapeData(null)
                as IDictionary<string, object>;
            linkedDict.Add("links",links);
            //返回状态码201
            //通过使用 CreatedAtRoute 返回时可以在 Header 中添加一个地址（Loaction）包含一个url找到新建资源
            return CreatedAtRoute(nameof(GetCompany), new { companyId = linkedDict["Id"] },
                linkedDict);
        }

        /// <summary>
        /// HttpOptions 获取针对某个webapi的通信选项的信息
        /// </summary>
        /// <returns></returns>
        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "DELETE,GET,PATCH,PUT,OPTIONS");
            return Ok();
        }

        /// <summary>
        /// 删除公司
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpDelete("{companyId}", Name = nameof(DeleteCompany))]
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            var companyEntity = await _companyRepository.GetCompanyAsync(companyId);
            if (companyEntity == null)
            {
                return NotFound();
            }
            //把 Employees 加载到内存中，使删除时可以追踪 ？？？（P33）
            // await _companyRepository.GetEmployeesAsync(companyId, new EmployeeDtoParameters());
            // await _companyRepository.GetEmployeesAsync(companyId, null,null);

            _companyRepository.DeleteCompany(companyEntity);
            await _companyRepository.SaveAsync();
            return NoContent();
        }

        /// <summary>
        /// 生成上一页或下一页的 URI（P35）
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private string CreateCompaniesResourceUri(CompanyDtoParameters parameters,
                                                  ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage: //上一页
                    return Url.Link(
                        //API 名
                        nameof(GetCompanies),
                        //Uri Query 字符串参数 匿名类
                        new
                        {
                            pageNumber = parameters.PageNumber - 1,
                            pageSize = parameters.PageSize,
                            companyName = parameters.CompanyName,
                            searchTerm = parameters.SearchTerm,
                            orderBy = parameters.OrderBy, //排序（P38）
                            fields = parameters.Fields  //数据塑形（P39）
                        }); ;
                case ResourceUriType.NextPage: //下一页
                    return Url.Link(
                        nameof(GetCompanies),
                        new
                        {
                            pageNumber = parameters.PageNumber + 1,
                            pageSize = parameters.PageSize,
                            companyName = parameters.CompanyName,
                            searchTerm = parameters.SearchTerm,
                            orderBy = parameters.OrderBy,
                            fields = parameters.Fields
                        });
                default: //当前页
                    return Url.Link(
                        nameof(GetCompanies),
                        new
                        {
                            pageNumber = parameters.PageNumber,
                            pageSize = parameters.PageSize,
                            companyName = parameters.CompanyName,
                            searchTerm = parameters.SearchTerm,
                            orderBy = parameters.OrderBy,
                            fields = parameters.Fields
                        });
            }
        }

        /// <summary>
        /// 为Company单个资源创建 HATEOAS 的 links（P41）
        /// </summary>
        /// <param name="companyId">Company Id</param>
        /// <param name="fields">fields 字符串</param>
        /// <returns>Company 单个资源的 links</returns>
        private IEnumerable<LinkDto> CreateLinksForCompany(Guid companyId, string fields)
        {
            var links = new List<LinkDto>();
            //GetCompany 的 link
            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDto(Url.Link(nameof(GetCompany), new { companyId }),//href - 超链接
                        "self",                                                             //rel - 与当前资源的关系或描述
                        "GET"));                                                         //method - 方法
            }
            else
            {
                links.Add(
                    new LinkDto(Url.Link(nameof(GetCompany), new { companyId, fields }),
                        "self",
                        "GET"));
            }
            //DeleteCompany 的 link
            links.Add(
                new LinkDto(Url.Link(nameof(DeleteCompany), new { companyId }),
                    "delete_company",
                    "DELETE"));
            //CreateEmployeeForCompany 的 link
            links.Add(
                new LinkDto(Url.Link(nameof(EmployeesController.CreateEmployeeForCompany), new { companyId }),
                    "create_employee_for_company",
                    "POST"));
            //GetEmployeesForCompany 的 link
            links.Add(
                new LinkDto(Url.Link(nameof(EmployeesController.GetEmployeesForCompany), new { companyId }),
                    "employees",
                    "GET"));
            return links;
        }

        /// <summary>
        /// 为 Companies 集合资源创建 HATEOAS 的 links（P42）
        /// </summary>
        /// <param name="parameters">CompanyDtoParameters</param>
        /// <param name="hasPrevious">是否有上一页</param>
        /// <param name="hasNext">是否有下一页</param>
        /// <returns>GetCompanies 集合资源的 links</returns>
        private IEnumerable<LinkDto> CreateLinksForCompany(CompanyDtoParameters parameters, bool hasPrevious, bool hasNext)
        {
            var links = new List<LinkDto>();
            //CurrentPage 当前页链接
            links.Add(new LinkDto(CreateCompaniesResourceUri(parameters, ResourceUriType.CurrentPage),
                "self",
                "GET"));
            if (hasPrevious)
            {
                //PreviousPage 上一页链接
                links.Add(new LinkDto(CreateCompaniesResourceUri(parameters, ResourceUriType.PreviousPage),
                    "previous_page",
                    "GET"));
            }
            if (hasNext)
            {
                //NextPage 下一页链接
                links.Add(new LinkDto(CreateCompaniesResourceUri(parameters, ResourceUriType.NextPage),
                    "next_page",
                    "GET"));
            }
            return links;
        }


    }
}