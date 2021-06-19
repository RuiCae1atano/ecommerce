using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Api.Search.Interfaces;
using Ecommerce.Api.Search.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Http;

namespace Ecommerce.Api.Search
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IOrdersService, OrdersService>();
            services.AddScoped<IProductsService, ProductsService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddHttpClient("OrdersService", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Orders"]);
            });
            services.AddHttpClient("ProductsService", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Products"]);
            }).AddPolicyHandler((services, request) => HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10)
        },
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            services.GetService<ILogger<ProductsService>>()
                .LogWarning("Delaying for {delay}ms, then making retry {retry}.", timespan.TotalMilliseconds, retryAttempt);
        }
        ));
            services.AddHttpClient("CustomersService", config =>
            {
                config.BaseAddress = new Uri(Configuration["Services:Customers"]);
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
