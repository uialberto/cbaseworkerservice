using BaseWorkService.DataAccess.Repositories.Core;
using BaseWorkService.Domain.Interfaces.Repositories.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseWorkService.Domain.Core.Facturas
{

    public static class FacturasServiceExtensions
    {
        //Squidex
        public static void AddMyFacturas(this IServiceCollection services, IConfiguration config)
        {
            //var options = config.GetSection("pipeline:confirms").Get<ConfirmPipelineOptions>() ?? new ConfirmPipelineOptions();

            //services.ConfigureAndValidate<UserNotificationsOptions>(config, "notifications");


            services.AddSingleton<IMongoRepoFacturas, MongoRepoFacturas>();
            //services.AddSingletonAs<UserNotificationStore>()
            //    .As<IUserNotificationStore>();

            //services.AddSingletonAs<UserNotificationFactory>()
            //    .As<IUserNotificationFactory>();

            //services.AddSingletonAs<UserNotificationService>()
            //    .As<IUserNotificationService>().AsSelf().As<IScheduleHandler<UserEventMessage>>().As<IMessageHandler<ConfirmMessage>>();

            //services.AddS<UserEventMessage>("UserNotifications");
        }

        public static void AddMyMongoFacturas(this IServiceCollection services)
        {
            services.AddSingleton<IMongoRepoFacturas, MongoRepoFacturas>();
        }
    }
}
