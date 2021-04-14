// Copyright 2020-2021 王建军
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Swm.SRgv;
using Swm.SRgv.GS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Swm.Web
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("logsettings.json")
            .AddJsonFile("device.json")
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            try
            {
                Log.Information("程序正在启动");

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "程序意外停止");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(b => b.AddConfiguration(Configuration))
                .UseSerilog()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }


    public class RgvOptions
    {
        public string? Name { get; set; }

        public string? IP { get; set; }

        public int Port { get; set; }
    }


    public class RgvService : IHostedService
    {
        IOptions<List<RgvOptions>> _rgvOptions;
        List<SRgv.SRgv> _rgvList = new List<SRgv.SRgv>();

        public IReadOnlyList<SRgv.SRgv> RgvList
        {
            get
            {
                return _rgvList;
            }
        }


        public RgvService(IOptions<List<RgvOptions>> rgvOptions)
        {
            _rgvOptions = rgvOptions;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            List<SRgv.SRgv> list = new List<SRgv.SRgv>();
            foreach (var options in _rgvOptions.Value)
            {
                IPAddress ip = IPAddress.Parse(options.IP ?? throw new ());
                var port = options.Port;
                var endPoint = new IPEndPoint(ip, port);
                SRgvCommunicator communicator = new SRgvCommunicator(endPoint, Log.ForContext<SRgvCommunicator>());
                SRgv.SRgv rgv = new SRgv.SRgv(options.Name ?? throw new(),
                                              new FakeDeviceTaskNoGenerator(),
                                              communicator,
                                              Log.ForContext<SRgv.SRgv>());

                list.Add(rgv);
            }

            _rgvList = list;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (RgvList != null)
            {
                return Task.WhenAll(RgvList.Select(x => x.ShutdownAsync()));
            }
            return Task.CompletedTask;
        }
    }
}

#pragma warning restore 1591
