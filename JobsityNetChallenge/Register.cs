using JobsityNetChallenge.StockBot;
using JobsityNetChallenge.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobsityNetChallenge
{
    public class Register
    {
        public static void BindSectionToConfigObject<TType>(IConfiguration configuration, IServiceCollection services) where TType : class, new()
        {
            var typeConfig = new TType();
            configuration.Bind(typeConfig.GetType().Name, typeConfig);
            services.AddSingleton(typeConfig);
        }

        public static void SetServices(IServiceCollection services)
        {
            //http
            services.AddHttpClient<IStockBotClient, StockBotClient>();
            services.AddSingleton<IChatStorage, ChatStorage>();
        }
    }
}
