using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem_DAL.Repository.Implementation;
using OrderManagementSystem_DAL.Repository.Interface;

namespace OrderManagementSystem_DAL.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCommonRepository(this IServiceCollection services)
        {
            services.AddScoped<IOrderListRepository, OrderListRepository>();
            services.AddScoped<IAgencyRepository, AgencyRepository>();

            return services;
        }
    }
}
