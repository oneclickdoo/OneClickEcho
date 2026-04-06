using MediatR;
using OneClickEcho.Application.Scheduling.Commands.SendBirthdayMessages;
using Quartz;

namespace OneClickEcho.Infrastructure.Services.Scheduling.Jobs;

public class BirthdayCampaignsJob(IMediator mediator) : IJob
{
    private readonly IMediator _mediator = mediator;

    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine(DateTime.Now + " - Running BirthdayCampaigns job...");
        await _mediator.Send(new SendBirthdayMessagesCommand());
    }
}