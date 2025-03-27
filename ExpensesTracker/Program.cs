using ExpensesTracker.Extensions;
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

namespace ExpensesTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args).ConfigureServices();

            var app = builder.Build().ConfigureMiddlewares();

            app.Run();
        }
    }
}
