using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DigitalArchiveManagement
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
            services.AddControllersWithViews().AddNewtonsoftJson();//added on 2020.05.17
            
            services.AddHttpContextAccessor();//added on 2020.05.17
            // ���������������
            services.Configure<FormOptions>(x =>
            {
                x.ValueCountLimit = 524288000;   // ��������ĸ������ƣ�Ĭ��ֵ��1024��
                x.ValueLengthLimit = 524288000;   // �����������ֵ�ĳ������ƣ�Ĭ��ֵ��4194304 = 1024 * 1024 * 4��
                x.MultipartBodyLengthLimit = long.MaxValue;
            });

            //�ϴ��ļ���С����Kestrel���� added on 20201221
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 524288000;// Set the limit to 500 MB
            });
            //�ϴ��ļ���С����IIS���� added on 20201221
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 524288000;
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);//��¼��ʱʱ��10����
                options.Cookie.HttpOnly = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();//added on 20201015

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=WLogin}/{action=Index}/{id?}/{userid?}/{other?}/{othertwo?}/{stctype?}");//stctype��ͳ�����ͣ���Ҫ�Ӳ˵��д���
            });
        }
    }
}
