using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Magicodes.ExporterAndImporter.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MyDemo.Data;
using MyDemo.Models;
using MyDemo.Models.Elasticsearch;
using MyDemo.Services;
using Newtonsoft.Json.Serialization;

namespace MyDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// ��������ע��
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //֧�ָ߼� CacheHeaders��������ȫ�����ã�P48��
            services.AddHttpCacheHeaders(expires =>
            {
                expires.MaxAge = 60;
                expires.CacheLocation = Marvin.Cache.Headers.CacheLocation.Private;
            }, validation =>
            {
                //�����Ӧ���ڣ�����������֤
                validation.MustRevalidate = true;
            });

            //��ӻ������P46��
            services.AddResponseCaching();
            /*
            * .Net Core Ĭ��ʹ�� Problem details for HTTP APIs RFC (7807) ��׼
            * - Ϊ���������Ϣ��Ӧ�ã�������ͨ�õĴ����ʽ
            * - ����ʶ�����������ĸ� API
            */

            //������һ�ֽϾɵ�д�����ڱ���Ŀ�в�ʹ�ã�P8��
            //services.AddControllers(options =>
            //{
            //    //����406״̬��
            //    options.ReturnHttpNotAcceptable = true;

            //    //OutputFormatters Ĭ������ֻ�� Json ��ʽ
            //    //��Ӷ���� XML ��ʽ��֧��
            //    //��ʱĬ�������ʽ��Ȼ�� Json ,��Ϊ Json ��ʽλ�ڵ�һλ��
            //    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

            //    //����� Index 0 λ�ò���� XML ��ʽ��֧�֣���ôĬ�������ʽ�� XML
            //    //options.OutputFormatters.Insert(0, new XmlDataContractSerializerOutputFormatter());
            //});

            //api�����ǽ��µ�д����AddXmlDataContractSerializerFormatters() �ȷ���ʹ�ø����㡣
            services.AddControllers(setup =>
                {
                    //����406״̬�루P7��
                    //setup.ReturnHttpNotAcceptable = true;

                    //���û����ֵ䣨P46��
                    //options.CacheProfiles.Add("120sCacheProfile", new CacheProfile
                    //{
                    //    Duration = 120
                    //});

                    //setup.Filters.Add(typeof(MagicodesFilter));
                })
                //Ĭ�ϸ�ʽȡ�������л����ߵ����˳�� P32
                .AddNewtonsoftJson(options => //������ JSON ���л��ͷ����л����ߣ����滻��ԭ��Ĭ�ϵ� JSON ���л����ߣ���P32��
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                })
                .AddXmlDataContractSerializerFormatters(); //XML���л��ͷ����л����� ֧�ָ������ͣ���datetime offset��P8,P25)
            //.ConfigureApiBehaviorOptions(options =>   //�Զ�����󱨸棨P29��
            //{
            //    //����һ��ί�� context���� IsValid == false ʱִ��
            //    options.InvalidModelStateResponseFactory = context =>
            //    {
            //        var problemDetails = new ValidationProblemDetails(context.ModelState)
            //        {   //TypeĬ��RFC�ٷ��ĵ�
            //            Type = "http://www.baidu.com",
            //            Title = "���ִ���",
            //            Status = StatusCodes.Status422UnprocessableEntity,
            //            Detail = "�뿴��ϸ��Ϣ",
            //            Instance = context.HttpContext.Request.Path
            //        };
            //        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
            //        return new UnprocessableEntityObjectResult(problemDetails)
            //        {
            //            ContentTypes = { "application/problem+json" }
            //        };
            //    };
            //});

            //MvcOptionsp����
            services.Configure<MvcOptions>(options =>
            {
                //ȫ������ NewtonsoftJsonOutputFormatter��P43��
                var newtonSoftJsonOutputFormatter = options.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()
                    ?.FirstOrDefault();
                if (newtonSoftJsonOutputFormatter != null)
                {
                    //��NewtonsoftJsonOutputFormatter ��Ϊ "application/vnd.company.hateoas+json" �� Media type �������ʽ����
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
            //ʹ�� AutoMapper��ɨ�赱ǰӦ��������� Assemblies Ѱ�� AutoMapper �������ļ���P12��
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            //AddScoped��ÿ�����󶼻�ȡһ���µ�ʵ����ͬһ�������ȡ��λ�õ���ͬ��ʵ��
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IMyScopedSerivce, MyScopedSerivce>();
            //AddTransient˲ʱģʽ��ÿ�����󶼻�ȡһ���µ�ʵ������ʹͬһ�������ȡ���Ҳ���ǲ�ͬ��ʵ��   
            //�����������ʹ�� Transient ע������ʹ�õ�����ӳ�����P37��
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<IMyTransientService, MyTransientService>();
            //services.AddTransient<IOrderService, DisposableOrderService>();
            //�ж� Uri query �ַ����е� fields �Ƿ�Ϸ���P39��
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();
            //AddSingleton����ģʽ��ÿ�ζ���ȡͬһ��ʵ��
            //services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("10.211.55.2:6379"));
            services.AddSingleton<IMySingletonService, MySingletonService>();
            services.AddSingleton<OrderServiceOptions>();
            services.AddSingleton<IOrderService, OrderService>();

            services.AddSingleton<IESClientProvider, ESClientProvider>();

            var connection = Configuration.GetConnectionString("MySqlConnection");
            services.AddDbContext<DBContext>(options => options.UseMySql(connection));

            //SwaggerΪAPI�ĵ�����˵����Ϣ,��AddSwaggerGen����������˵����Ϣ
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
                c.OperationFilter<AddRequiredHeaderParameter>();

                // Ϊ Swagger JSON and UI����xml�ĵ�ע��·��
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var basePath =
                    Path.GetDirectoryName(typeof(Program).Assembly.Location); //��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
                var xmlPath = Path.Combine(basePath, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        /// <summary>
        /// ָ��asp.netcore web���������Ӧÿ��http����app����м��
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
                //500 ������Ϣ
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

            //�����м��
            app.UseResponseCaching(); //��P46��
            //app.UseHttpCacheHeaders(); //��P48��UseResponseCaching֮ǰʹ��

            //·���м��
            app.UseRouting();
            app.UseMagiCodesIE();

            //�����Ȩ
            app.UseAuthorization();

            //�˵� http��������ض�controller
            app.UseEndpoints(endpoints =>
            {
                //·�ɱ�
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}