
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
            //��ӱ��ز��Կ�������
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

            // ��ȡHttp֧��
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // ��ʼ����ʱ����
            services.AddTimedJob();
            // ����httpѹ��
            services.AddResponseCompression();
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //UseDefaultFiles ���ض���default.htm��default.html��index.htm��index.html
            app.UseDefaultFiles();
            //�����.net core webapi ����wwwroot�ļ��е����ã�������̬�ļ�
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
            // ��������
            app.UseCors(corsPolicyBuilder => corsPolicyBuilder.AllowAnyMethod().AllowAnyOrigin());
            app.UseHsts();
        }
    }
}
