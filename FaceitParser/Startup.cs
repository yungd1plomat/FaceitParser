using FaceitParser.Abstractions;
using FaceitParser.Data;
using FaceitParser.Helpers;
using FaceitParser.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FaceitParser
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["CONNECTION_STRING"] ?? throw new InvalidOperationException("connection string not found");
            var steamApiKey = Configuration["STEAM_API_KEY"] ?? throw new InvalidOperationException("steam api key not found");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 27))));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                // enables immediate logout, after updating the user's security stamp.
                options.ValidationInterval = TimeSpan.Zero;
            });

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/users/login";
                options.AccessDeniedPath = "/";
            });
            services.AddSingleton<ISteamApi, SteamApi>(api => new SteamApi(steamApiKey));
            services.AddSingleton<IServiceResolver, ServiceResolver>();
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallback(async context =>
                {
                    context.Response.Redirect("/");
                }); // Redirect not found maps to home page
            });
        }



    }

}
