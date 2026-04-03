using Microsoft.Extensions.DependencyInjection;
using OneClickEcho.Infrastructure.Services.Scheduling.Jobs;
using Quartz;
using Quartz.Logging;

namespace OneClickEcho.Infrastructure.Services.Scheduling;

public class ConsoleLogProvider : ILogProvider
{
    public Logger GetLogger(string name)
    {
        return (level, func, exception, parameters) =>
        {
            if (level >= LogLevel.Info && func != null)
            {
                Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
            }

            return true;
        };
    }

    public IDisposable OpenNestedContext(string message)
    {
        throw new NotImplementedException();
    }

    public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
    {
        throw new NotImplementedException();
    }
}

public static class SchedulingServiceConfiguration
{
    public static IServiceCollection AddSchedulingService(this IServiceCollection services)
    {
        LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

        services.AddQuartz(q =>
        {
            // @TODO: Default setting, verify if needs to be changed
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });

            q.ScheduleJob<BirthdayCampaignsJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("birthday-campaigns-trigger")
                    .ForJob("birthday-campaigns-job")
                    .WithCronSchedule("0 0 9 1/1 * ? *"), // Every day at 9 A.M local (server) time
                                                          //.WithSimpleSchedule(x => x // Testing
                                                          //    .WithIntervalInSeconds(30)
                                                          //    .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("birthday-campaigns-job")
            );

            q.ScheduleJob<ImmediateCampaignsJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("immediate-campaigns-trigger")
                    .ForJob("immediate-campaigns-job")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(30)
                        .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("immediate-campaigns-job")
            );

            q.ScheduleJob<UpcomingCampaignsJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("upcoming-campaigns-trigger")
                    .ForJob("upcoming-campaigns-job")
                    // .WithCronSchedule("0 0/30 * 1/1 * ? *") // Every 30 minutes, starting at minute 0 (not app start time)
                    .WithSimpleSchedule(x => x // Testing
                        .WithIntervalInSeconds(30)
                        .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("upcoming-campaigns-job")
            );
            
            q.ScheduleJob<ViberDeliveryJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("viber-campaigns-last-48h-delivery-trigger")
                    .ForJob("viber-campaigns-last-48h-delivery-job")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(1)
                        .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("viber-campaigns-last-48h-delivery-job")
            );
            
            q.ScheduleJob<ViberAnswersJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("viber-campaigns-last-48h-answers-trigger")
                    .ForJob("viber-campaigns-last-48h-answers-job")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(1)
                        .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("viber-campaigns-last-48h-answers-job")
            );
            
            q.ScheduleJob<ViberTestDeliveryJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("viber-test-delivery-trigger")
                    .ForJob("viber-test-delivery-job")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(10)
                        .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("viber-test-delivery-job")
            );
            
            q.ScheduleJob<SmsTestDeliveryJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("sms-test-delivery-trigger")
                    .ForJob("sms-test-delivery-job")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(10)
                        .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("sms-test-delivery-job")
            );

            q.ScheduleJob<EnqueueApiMessagesJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("enqueue-api-messages-trigger")
                    .ForJob("enqueue-api-messages-job")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("enqueue-api-messages-job")
            );
            
            q.ScheduleJob<ApiMessageDeliveryJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("api-messages-delivery-trigger")
                    .ForJob("api-messages-delivery-job")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(1)
                        .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("api-messages-delivery-job")
            );
            
            q.ScheduleJob<ExpirePendingViberMessagesJob>(
                triggerConfigurator => triggerConfigurator
                    .WithIdentity("expire-pending-messages-trigger")
                    .ForJob("expire-pending-messages-job")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(1)
                        .RepeatForever()),
                jobConfigurator => jobConfigurator
                    .WithIdentity("expire-pending-messages-job")
            );
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }
}