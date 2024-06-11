using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DigitalArchiveManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);//�������������,�����Ϳ���ʹ�� dotnet myweb.dll --urls "http://*:5200;https://*:5100" ��ʽ�򿪲������վ�� 20200813
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        //����Ӧ�÷�����Kestrel���������Ϊ500MB added on 20201221
                        options.Limits.MaxRequestBodySize = 524288000;
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
