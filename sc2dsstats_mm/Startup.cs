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
using sc2dsstats_mm.Areas.Identity;
using sc2dsstats_mm.Data;
using Blazor.FileReader;
using dsmm_server.Models;
using dsmm_server.Data;
using sc2dsstats.Data;
using s2decode;
using DSmm.Repositories;
using dsmm_server.Repositories;
using DSmm;

namespace sc2dsstats_mm
{
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<MMdb>(options => options.UseSqlite("Data Source=/data/mm.db"));
            var optionsBuilder = new DbContextOptionsBuilder<MMdb>();
            optionsBuilder.UseSqlite("Data Source=/data/mm.db");
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingAuthenticationStateProvider<IdentityUser>>();
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


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
                endpoints.MapControllers();
                endpoints.MapBlazorHub<App>(selector: "app");
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
