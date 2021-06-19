using Ecommerce.Api.Search.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Api.Search.Services
{
    public class SearchService : ISearchService
    {
        private readonly IOrdersService ordersService;
        private readonly IProductsService productsService;
        private readonly ICustomerService customerService;

        public SearchService(IOrdersService ordersService, IProductsService productsService, ICustomerService customerService)
        {
            this.ordersService = ordersService;
            this.productsService = productsService;
            this.customerService = customerService;
        }
        public async Task<(bool IsSuccess, dynamic SearchResults)> SearchAsync(int customerId)
        {
            //await Task.Delay(1);
            //return (true, new { Message = "Hello" });
            var ordersResult = await ordersService.GetOrdersAsync(customerId);
            var productsResult = await productsService.GetProductsAsync();
            var customerResult = await customerService.GetCustomerAsync(customerId);

            if (!customerResult.IsSuccess)
            {
                return (false, null);
            }

            if (productsResult.IsSuccess)
            {
                foreach (var order in ordersResult.Orders)
                {
                    foreach (var item in order.Items)
                    {
                        item.ProductName = productsResult.IsSuccess ?
                            productsResult.Products.FirstOrDefault(p => p.Id == item.ProductId)?.Name :
                            "Product Information is not Available";
                    }
                }
            }

            if (ordersResult.IsSuccess)
            {
                var result = new { Orders = ordersResult.Orders,Customer = customerResult.IsSuccess ?
                                        customerResult.Customer : new Models.Customer { Name = "Customer Information is not Available" } };
                return (true, result);
            };
            return (false, null);
        }
    }
}
