using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using dsmm_server.Areas.Identity;
using dsmm_server.Data;
using dsweb_electron6.Data;
using Blazor.FileReader;
using Microsoft.AspNetCore.Http;
using s2decode;
using dsmm_server.Models;
using DSmm;
using DSmm.Attributes;
using DSmm.Middleware;
using DSmm.Repositories;
using dsmm_server.Repositories;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace dsmm_server
{
    [DbContext(typeof(MMdb))]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=/data/app.db"));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddDbContext<MMdb>(options => options.UseSqlite("Data Source=/data/mm.db"));
            var optionsBuilder = new DbContextOptionsBuilder<MMdb>();
            optionsBuilder.UseSqlite("Data Source=/data/mm.db");
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddFileReaderService(options => options.InitializeOnFirstCall = true);
            services.AddScoped<AuthenticationStateProvider, RevalidatingAuthenticationStateProvider<IdentityUser>>();
            services.AddScoped<ScanStateChange>();
            services.AddSingleton<S2decode>();
            services.AddScoped<ReportService>();
            services.AddScoped<MMservice>();
            services.AddScoped<MMserviceNG>();
            services.AddScoped<DSdyn>();

            services.Configure<AppConfig>(Configuration);
            services.AddSingleton<IMMrepository, MMrepository>();
            services.AddSingleton<IMMrepositoryNG, MMrepositoryNG>();
            services.AddSingleton(new StartUp(optionsBuilder.Options));
            services.AddScoped<AuthenticationFilterAttribute>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_BASEPATH");
            if (!string.IsNullOrEmpty(basePath))
            {
                app.Use((context, next) =>
                {
                    context.Request.PathBase = new PathString(basePath);
                    if (context.Request.Path.StartsWithSegments(basePath, out var remainder))
                    {
                        context.Request.Path = remainder;
                    }
                    return next();
                });
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseMiddleware<HttpExceptionMiddleware>();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
