using Microsoft.EntityFrameworkCore;
using MyDemo.Entities;
using MyDemo.Models.DtoParamaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyDemo.Data;
using MyDemo.Helpers;
using MyDemo.Models.Dto;

namespace MyDemo.Services
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly DBContext _context;

        private readonly IPropertyMappingService _propertyMappingService;

        public CompanyRepository(DBContext context, IPropertyMappingService propertyMappingService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _propertyMappingService = propertyMappingService
                                      ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        /// <summary>
        /// GetAllCompanies
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Company> GetAllCompanies()
        {
            return _context.Companies.ToList();
        }

        /// <summary>
        /// GetCompaniesAsync
        /// </summary>
        /// <returns></returns>
        public async Task<List<Company>> GetCompaniesAsync()
        {
            return await _context.Companies.ToListAsync();
        }

        /// <summary>
        /// 条件查询
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<PagedList<Company>> GetCompaniesAsync(CompanyDtoParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var queryExpression = _context.Companies as IQueryable<Company>;

            //查找指定公司
            if (!string.IsNullOrWhiteSpace(parameters.CompanyName))
            {
                parameters.CompanyName = parameters.CompanyName.Trim();
                queryExpression = queryExpression.Where(x => x.Name == parameters.CompanyName);
            }

            //模糊搜索
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                parameters.SearchTerm = parameters.SearchTerm.Trim();
                queryExpression = queryExpression.Where(x => x.Name.Contains(parameters.SearchTerm)
                                                             || x.Introduction.Contains(parameters.SearchTerm));
            }

            //排序P38
            if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
            {
                //取得映射关系字典
                var mappingDictionary = _propertyMappingService.GetPropertyMapping<CompanyDto, Company>();
                //ApplySort 是一个自己定义的拓展方法
                //传入 FormQuery 中的 OrderBy 字符串与映射关系字典，返回排序好的字符串
                queryExpression = queryExpression.ApplySort(parameters.OrderBy, mappingDictionary);
            }

            //queryExpression = queryExpression.Skip(parameters.PageSize * (parameters.PageNumber - 1))
            //    .Take(parameters.PageSize);
            ////ToListAsync真正查询数据库
            return await PagedList<Company>.CreateAsync(queryExpression, parameters.PageNumber, parameters.PageSize);
        }

        /// <summary>
        /// 通过id获取
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<Company> GetCompanyAsync(Guid companyId)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            return await _context.Companies
                .FirstOrDefaultAsync(x => x.Id == companyId);
        }


        /// <summary>
        /// 通过id集合获取
        /// </summary>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Company>>
            GetCompaniesAsync(IEnumerable<Guid> companyIds)
        {
            if (companyIds == null)
            {
                throw new ArgumentNullException(nameof(companyIds));
            }

            return await _context.Companies
                .Where(x => companyIds.Contains(x.Id))
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        /// <summary>
        /// 增
        /// </summary>
        /// <param name="company"></param>
        public void AddCompany(Company company)
        {
            if (company == null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            //id可以服务端生成，也可以客户端生成
            company.Id = Guid.NewGuid();

            if (company.Employees != null)
            {
                foreach (var employee in company.Employees)
                {
                    employee.Id = Guid.NewGuid();
                }
            }

            _context.Companies.Add(company);
        }

        /// <summary>
        /// 改
        /// </summary>
        /// <param name="company"></param>
        public void UpdateCompany(Company company)
        {
            // _context.Entry(company).State = EntityState.Modified;
        }

        /// <summary>
        /// 删除公司
        /// </summary>
        /// <param name="company"></param>
        public void DeleteCompany(Company company)
        {
            if (company == null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            _context.Companies.Remove(company);
        }

        /// <summary>
        /// 通过id判断存在
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<bool> CompanyExistsAsync(Guid companyId)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            return await _context.Companies.AnyAsync(x => x.Id == companyId);
        }

        /// <summary>
        /// 通过公司id和员工id获取
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            if (employeeId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            return await _context.Employees
                .Where(x => x.CompanyId == companyId && x.Id == employeeId)
                .FirstOrDefaultAsync();
        }


        /// <summary>
        /// 公司id条件查询
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId, EmployeeDtoParameters parameters)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            var queryExpression = _context.Employees.Where(x => x.CompanyId == companyId);

            //性别筛选
            if (!string.IsNullOrWhiteSpace(parameters.Gender))
            {
                parameters.Gender = parameters.Gender.Trim();
                var gender = Enum.Parse<Gender>(parameters.Gender);
                queryExpression = queryExpression.Where(x => x.Gender == gender);
            }

            //查询
            if (!string.IsNullOrWhiteSpace(parameters.Q))
            {
                parameters.Q = parameters.Q.Trim();
                queryExpression = queryExpression.Where(x => x.EmployeeNo.Contains(parameters.Q)
                                                             || x.FirstName.Contains(parameters.Q)
                                                             || x.LastName.Contains(parameters.Q));
            }

            //if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
            //{
            //    if (parameters.OrderBy.ToLower() == "name")
            //    {
            //        queryExpression = queryExpression.OrderBy(x => x.FirstName).ThenBy(x => x.LastName);
            //    }
            //}

            //排序（P36 P37）
            if (!string.IsNullOrWhiteSpace(parameters.OrderBy))
            {
                //取得映射关系字典
                var mappingDictionary = _propertyMappingService.GetPropertyMapping<EmployeeDto, Employee>();
                //ApplySort是一个自己定义的拓展方法
                //传入 FormQuery 中的 OrderBy 字符串与映射关系字典
                //返回排序好的字符串
                queryExpression = queryExpression.ApplySort(parameters.OrderBy, mappingDictionary);
            }

            return await queryExpression.ToListAsync();
        }

        /// <summary>
        /// 根据公司添加employee
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="employee"></param>
        public void AddEmployee(Guid companyId, Employee employee)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            employee.CompanyId = companyId;
            _context.Employees.Add(employee);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="employee"></param>
        public void UpdateEmployee(Employee employee)
        {
            //使用 EF，无需显式地声明
            // _context.Entry(employee).State = EntityState.Modified;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="employee"></param>
        public void DeleteEmployee(Employee employee)
        {
            _context.Employees.Remove(employee);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }

        /// <summary>
        /// 基础查询
        /// https://mp.weixin.qq.com/s/Pzqr6a4bfTgFoB4_40JxrA
        /// </summary>
        public void Query()
        {
            _context.Set<Company>().ToList();


            //查询单个
            //如果有多个实体符合筛选部分， SingleOrDefaultAsync 将引发异常。
            //如果有多个实体符合筛选部分， FirstOrDefaultAsync 不引发异常。
            _context.Movie.FirstOrDefaultAsync(m => m.Id == 1);
            //在大部分基架代码中，FindAsync 可用于替代 FirstOrDefaultAsync ，查找具有主键 (PK) 的实体。
            //如果具有 PK 的实体正在由上下文跟踪，会返回该实体且不向 DB 发出请求。
            _context.Movie.FindAsync(1);

            //查询指定列
            _context.Set<Company>().AsNoTracking().Where(t => t.Id == Guid.NewGuid())
                .Select(t => new {t.Country, t.Industry})
                .FirstOrDefaultAsync();

            //join()ß
            //两表不必含有外键关系，需要代码手动指定连接外键相等（具有可拓展性，除了值相等，还能指定是>,<以及其他对两表的相应键的关系）以及结果字段。
            var wholeRecord1 =
                _context.Employees.Join(_context.Companies, a => a.CompanyId, g => g.Id,
                    (a, g) => new {a.FirstName, a.CompanyId, g.Name});
            //Include()
            //两表必须含有外键关系，只需要指定键名对应的类属性名即可，不需指定结果字段（即全部映射）。
            //默认搜索某表时，不会顺带查询外键表，直到真正使用时才会再读取数据库查询；若是使用 Include()，则会在读取本表时把指定的外键表信息也读出来
            //var wholeRecord = dc.Album.Include("Genre");
            //或者
            var wholeRecord = _context.Employees.Include(a => a.Company);

            //跟踪查询
            _context.Companies.ToListAsync();
            //非跟踪查询
            _context.Companies.AsNoTracking().ToListAsync();


        }
    }
}