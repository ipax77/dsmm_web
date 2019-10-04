using Blazor.FileReader;
using DSmm;
using DSmm.Repositories;
using dsmm_server.Data;
using dsmm_server.Models;
using dsmm_server.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sc2dsstats.Data;
using sc2dsstats_mm_dev.Areas.Identity;
using sc2dsstats_mm_dev.Data;
using System;

namespace sc2dsstats_mm_dev
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            /**
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            **/
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=/data/app.db"));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddDbContext<MMdb>(options => options.UseSqlite("Data Source=/data/mm.db"));
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

            services.AddFileReaderService(options => options.InitializeOnFirstCall = true);

            services.AddScoped<ScanStateChange>();
            services.AddScoped<MMservice>();
            services.AddScoped<MMserviceNG>();
            services.AddScoped<DSdyn>();
            services.AddScoped<GameChartService>();
            services.AddScoped<DSdyn_filteroptions>();

            services.Configure<AppConfig>(Configuration);

            services.AddSingleton(_env.ContentRootFileProvider);
            services.AddSingleton<IMMrepository, MMrepository>();
            services.AddSingleton<IMMrepositoryNG, MMrepositoryNG>();
            services.AddSingleton<ReportService>();
            var optionsBuilder = new DbContextOptionsBuilder<MMdb>();
            optionsBuilder.UseSqlite("Data Source=/data/mm.db");
            services.AddSingleton<StartUp>((prov) => new StartUp(optionsBuilder.Options));
            StartUp startup = new StartUp(optionsBuilder.Options);
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_BASEPATH");
            if (!string.IsNullOrEmpty(basePath))
            {
                app.Use((context, next) =>
                {
                    context.Request.Scheme = "https";
                    return next();
                });

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
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                //ForwardedHeaders = ForwardedHeaders.All
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
