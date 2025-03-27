using ExpensesTracker.Middlewares;
using ExpensesTracker.Models;
using ExpensesTracker.Models.IdentityEntities;
using ExpensesTracker.Repositories;
using ExpensesTracker.Repositories.Interfaces;
using ExpensesTracker.Services;
using ExpensesTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using Serilog;

namespace ExpensesTracker.Extensions
{
    public static class StartupExtensionMethods
    {
        /// <summary>
        /// Configure Application Services.
        /// </summary>
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllersWithViews();

            // Services
            builder.Services.AddScoped<IEntryRepository, EntryRepository>();
            builder.Services.AddScoped<IEntryService, EntryService>();
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, CustomClaimsPrincipalFactory>();

            // DBContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // Serilog
            builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
            });

            // Rotativa (~\wwwroot\Rotativa\wkhtmltopdf.exe must exist)
            RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

            // Identity
            builder.Services.AddIdentity<AppUser, AppRole>(
                                options =>
                                {
                                    options.Password.RequiredLength = 4;
                                    options.Password.RequireNonAlphanumeric = false;
                                    options.Password.RequireDigit = false;
                                    options.Password.RequireLowercase = false;
                                    options.Password.RequireUppercase = false;
                                    options.Password.RequiredUniqueChars = 0;
                                }
                            )
                            .AddEntityFrameworkStores<ApplicationDbContext>()
                            .AddDefaultTokenProviders()
                            .AddUserStore<UserStore<AppUser, AppRole, ApplicationDbContext, Guid>>()
                            .AddRoleStore<RoleStore<AppRole, ApplicationDbContext, Guid>>();

            // Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); // requires use to be logged in; adds 'ReturnUrl' after FallbackUrl/LoginUrl
                options.AddPolicy("NotLoggedIn", policy => // policy for Not Logged In user.
                {
                    policy.RequireAssertion(context =>
                    {
                        return (context.User.Identity == null ? true : !(context.User.Identity.IsAuthenticated));
                    });
                });
            });

            // Cookies
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/SignIn"; // fall back URL / Login URL
            });

            return builder;
        }

        /// <summary>
        /// Configure the Application Middleware Pipeline.
        /// </summary>
        public static WebApplication ConfigureMiddlewares(this WebApplication app)
        {
            app.UseHsts();
            app.UseHttpsRedirection();

            // Custom Middleware for CorrelationID & Request logging
            app.UseRequestIdLoggingMiddleware();
            app.UseHttpLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute( // area routing
                    name: "areas",
                    pattern: "{area:exists}/{controller}/{action}"
                    );

                endpoints.MapControllerRoute( // default routing
                    name: "default",
                    pattern: "{controller}/{action}/{id?}"
                    );
            });

            return app;
        }
    }
}
