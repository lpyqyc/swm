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

using Arctic.AppCodes;
using Arctic.AppSeqs;
using Arctic.AppSettings;
using Arctic.AspNetCore;
using Arctic.EventBus;
using Arctic.NHibernateExtensions;
using Autofac;
using AutofacSerilogIntegration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Context;
using Swm.InboundOrders;
using Swm.Locations;
using Swm.Materials;
using Swm.Model;
using Swm.Model.Extentions;
using Swm.OutboundOrders;
using Swm.Palletization;
using Swm.StorageLocationAssignment;
using Swm.TransportTasks;
using Swm.TransportTasks.Cfg;
using System;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Security.Principal;
using System.Text;

#pragma warning disable 1591

namespace Swm.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            AutofacContainer = default!;
        }

        public IConfiguration Configuration { get; }

        public ILifetimeScope AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 集成 Microsoft.AspNetCore.Identity

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("auth")));

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
                .AddSignInManager<SignInManager<ApplicationUser>>()
                .AddUserManager<UserManager<ApplicationUser>>()
                // .AddDefaultTokenProviders()
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            #endregion

            services.AddDbContext<LogDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("LogDb")));

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var result = new BadRequestObjectResult(context.ModelState);

                        result.ContentTypes.Add(MediaTypeNames.Application.Json);
                        result.ContentTypes.Add(MediaTypeNames.Application.Xml);

                        return result;
                    };
                });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swm.Web", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddHttpContextAccessor();
            services.AddTransient<IPrincipal>(provider =>
                provider.GetService<IHttpContextAccessor>()?.HttpContext?.User!
                );

            services.AddAuthentication(options => {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    JwtSetting jwtSetting = Configuration.GetSection("JwtSetting").Get<JwtSetting>();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtSetting.Issuer,
                        ValidAudience = jwtSetting.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SecurityKey)),
                    };
                })
                .AddIdentityCookies(o => { });

            services.AddOperationType();
            services.AddAuthorization(options =>
            {
            });

            services.Configure<TransportTasksOptions>(options => Configuration.GetSection("Swm:TransportTasks").Bind(options));

            services.Configure<JwtSetting>(options =>
            {
                Configuration.GetSection("JwtSetting").Bind(options);
            });
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterLogger();

            builder.AddAppCodes();
            builder.AddAppSeqs();
            builder.AddAppSettings();

            builder.RegisterModule<OpsModule>();
            builder.RegisterModule<MaterialsModule>();
            builder.RegisterModule<LocationsModule>();
            builder.RegisterModule<StorageLocationAssignmentModule>();
            builder.RegisterModule(new PalletizationModule
            {
                PalletCodePattern = Configuration.GetSection("Swm:Palletization:PalletCodePattern").Value,
            });

            builder.RegisterModule(new TransportTasksModule
            {
                Options = Configuration.GetSection("Swm:TransportTasks").Get<TransportTasksOptions>(),
            });
            
            builder.RegisterModule<OutboundOrdersModule>();
            builder.RegisterModule<InboundOrdersModule>();

            builder.AddEx();

            builder.AddEventBus(Configuration.GetSection("EventBus").Get<SimpleEventBusOptions>());
            builder.AddNHibernate();

            builder.RegisterType<OpHelper>().AsSelf().InstancePerLifetimeScope();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseExceptionHandler("/error-local-development");
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arctic.Web v1"));
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            // TODO 改为安全的设置
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.Use(async (context, next) =>
            {
                using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
                {
                    await next();
                }
            });

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();

//            if (env.IsDevelopment())
//            {
//#warning 仅用于调试期间
//                app.Use(async (context, next) =>
//                {
//                    var claims = new List<Claim>
//                    {
//                        new Claim(ClaimTypes.Name, "admin"),
//                        new Claim(ClaimTypes.Role, "admin"),
//                        new Claim(ClaimTypes.Role, "dev"),
//                    };

//                    ClaimsIdentity identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
//                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
//                    context.User = principal;
//                    await next();
//                });
//            }

            app.Use(async (context, next) =>
            {
                using (LogContext.PushProperty("UserName", context.User?.Identity?.Name ?? "-"))
                {
                    await next();
                }
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class JwtSetting
    {
        /// <summary>
        /// 安全密钥
        /// </summary>
        public string? SecurityKey { get; set; }

        /// <summary>
        /// 颁发者
        /// </summary>
        public string? Issuer { get; set; }

        /// <summary>
        /// 接收者
        /// </summary>
        public string? Audience { get; set; }

        /// <summary>
        /// Token 的过期时间，单位为分钟
        /// </summary>
        public int TokenExpiry { get; set; }
    }

}

#pragma warning restore 1591
