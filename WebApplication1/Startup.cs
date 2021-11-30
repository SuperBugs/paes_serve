
using paems.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });
            //添加本地测试跨域请求
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    builder =>
                    {
                        builder.WithOrigins(
                            "http://localhost:8088",
                            "http://localhost:8001",
                            "http://localhost:8000",
                            "http://192.168.137.240:51972",
                            "http://192.168.137.240:8000",
                            "http://10.0.2.2:51972",
                            "http://129.211.3.240:8088",
                            "http://localhost:51972",
                            "http://lcfcsgx.ink:8088/"
                            ).AllowAnyHeader();
                    });
            });

            // 获取Http支持
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // 初始化定时工具
            services.AddTimedJob();
            // 开启http压缩
            services.AddResponseCompression();
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //UseDefaultFiles 会重定向到default.htm、default.html、index.htm、index.html
            app.UseDefaultFiles();
            //这个是.net core webapi 访问wwwroot文件夹的配置，开启静态文件
            app.UseStaticFiles(new StaticFileOptions()
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/x-msdownload"
            });

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowSpecificOrigins");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseTimedJob();
            app.UseResponseCompression();
            // 跨域问题
            app.UseCors(corsPolicyBuilder => corsPolicyBuilder.AllowAnyMethod().AllowAnyOrigin());
            app.UseHsts();
        }
    }
}
