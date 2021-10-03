using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Converters;
using Microsoft.AspNetCore.Http.Features;

[assembly: WebJobsStartup(typeof(api.Startup))]
namespace api
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; 
            });

            builder.Services
                .AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions = ConverterOptions.JsonSerializerOptions;
                });
        }
    }
}
