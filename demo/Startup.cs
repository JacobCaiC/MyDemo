using AutoMapper;
using demo.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDemo.Services;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using MyDemo.Models.Elasticsearch;

namespace demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// 服务容器注册
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //支持高级 CacheHeaders，并进行全局配置（P48）
            services.AddHttpCacheHeaders(expires =>
            {
                expires.MaxAge = 60;
                expires.CacheLocation = Marvin.Cache.Headers.CacheLocation.Private;
            }, validation =>
            {
                //如果响应过期，必须重新验证
                validation.MustRevalidate = true;
            });

            //添加缓存服务（P46）
            services.AddResponseCaching();
            /*
            * .Net Core 默认使用 Problem details for HTTP APIs RFC (7807) 标准
            * - 为所需错误信息的应用，定义了通用的错误格式
            * - 可以识别问题属于哪个 API
            */

            //以下是一种较旧的写法，在本项目中不使用（P8）
            //services.AddControllers(options =>
            //{
            //    //启用406状态码
            //    options.ReturnHttpNotAcceptable = true;

            //    //OutputFormatters 默认有且只有 Json 格式
            //    //添加对输出 XML 格式的支持
            //    //此时默认输出格式依然是 Json ,因为 Json 格式位于第一位置
            //    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

            //    //如果在 Index 0 位置插入对 XML 格式的支持，那么默认输出格式是 XML
            //    //options.OutputFormatters.Insert(0, new XmlDataContractSerializerOutputFormatter());
            //});

            //api以下是较新的写法，AddXmlDataContractSerializerFormatters() 等方法使用更方便。
            services.AddControllers(setup =>
                {
                    //启用406状态码（P7）
                    setup.ReturnHttpNotAcceptable = true;

                    //配置缓存字典（P46）
                    //options.CacheProfiles.Add("120sCacheProfile", new CacheProfile
                    //{
                    //    Duration = 120
                    //});
                })
                //默认格式取决于序列化工具的添加顺序 P32
                .AddNewtonsoftJson(options => //第三方 JSON 序列化和反序列化工具（会替换掉原本默认的 JSON 序列化工具）（P32）
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                })
                .AddXmlDataContractSerializerFormatters(); //XML序列化和反序列化工具 支持更多类型，如datetime offset（P8,P25)
            //.ConfigureApiBehaviorOptions(options =>   //自定义错误报告（P29）
            //{
            //    //创建一个委托 context，在 IsValid == false 时执行
            //    options.InvalidModelStateResponseFactory = context =>
            //    {
            //        var problemDetails = new ValidationProblemDetails(context.ModelState)
            //        {   //Type默认RFC官方文档
            //            Type = "http://www.baidu.com",
            //            Title = "出现错误",
            //            Status = StatusCodes.Status422UnprocessableEntity,
            //            Detail = "请看详细信息",
            //            Instance = context.HttpContext.Request.Path
            //        };
            //        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
            //        return new UnprocessableEntityObjectResult(problemDetails)
            //        {
            //            ContentTypes = { "application/problem+json" }
            //        };
            //    };
            //});

            //MvcOptionsp配置
            services.Configure<MvcOptions>(options =>
            {
                //全局设置 NewtonsoftJsonOutputFormatter（P43）
                var newtonSoftJsonOutputFormatter = options.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()
                    ?.FirstOrDefault();
                if (newtonSoftJsonOutputFormatter != null)
                {
                    //将NewtonsoftJsonOutputFormatter 设为 "application/vnd.company.hateoas+json" 等 Media type 的输出格式化器
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.company.hateoas+json");
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.company.friendly+json");
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add(
                        "application/vnd.company.friendly.hateoas+json");
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.company.full+json");
                    newtonSoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.company.full.hateoas+json");
                }
            });
            //mvc
            services.AddControllersWithViews().AddXmlDataContractSerializerFormatters();
            //使用 AutoMapper，扫描当前应用域的所有 Assemblies 寻找 AutoMapper 的配置文件（P12）
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            //AddScoped：每次请求都获取一个新的实例。同一个请求获取多次会得到相同的实例
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IMyScopedSerivce, MyScopedSerivce>();
            //AddTransient瞬时模式：每次请求都获取一个新的实例。即使同一个请求获取多次也会是不同的实例   
            //轻量服务可以使用 Transient 注册排序使用的属性映射服务（P37）
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<IMyTransientService, MyTransientService>();
            //services.AddTransient<IOrderService, DisposableOrderService>();
            //判断 Uri query 字符串中的 fields 是否合法（P39）
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();
            //AddSingleton单例模式：每次都获取同一个实例
            //services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("10.211.55.2:6379"));
            services.AddSingleton<IMySingletonService, MySingletonService>();
            services.AddSingleton<OrderServiceOptions>();
            services.AddSingleton<IOrderService, OrderService>();

            services.AddSingleton<IESClientProvider, ESClientProvider>();

            var connection = Configuration.GetConnectionString("MySqlConnection");
            services.AddDbContext<DBContext>(options => options.UseMySql(connection));

            //Swagger为API文档增加说明信息,在AddSwaggerGen方法中配置说明信息
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "DEMO API",
                    Description = "ASP.NET Core Web API",
                    Contact = new OpenApiContact()
                    {
                        Name = "JacobCai",
                        Email = "cbb2@shanghai-electric.com",
                        //Url = new Uri("https://twitter.com/spboyer"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });

                // 为 Swagger JSON and UI设置xml文档注释路径
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var basePath =
                    Path.GetDirectoryName(typeof(Program).Assembly.Location); //获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                var xmlPath = Path.Combine(basePath, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        /// <summary>
        /// 指定asp.netcore web程序如何响应每个http请求，app添加中间件
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
                c.DocumentTitle = "JacobCai API";
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //500 错误信息
                //app.UseExceptionHandler(appBuilder =>
                //{
                //    appBuilder.Run(async context =>
                //    {
                //        context.Response.StatusCode = 500;
                //        await context.Response.WriteAsync("Unexpected Error!");
                //    });
                //});
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();


            //缓存中间件
            app.UseResponseCaching(); //（P46）
            //app.UseHttpCacheHeaders(); //（P48）UseResponseCaching之前使用

            //路由中间件
            app.UseRouting();

            //添加授权
            app.UseAuthorization();

            //端点 http请求分配特定controller
            app.UseEndpoints(endpoints =>
            {
                //路由表
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}