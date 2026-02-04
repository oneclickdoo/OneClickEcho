using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Application.Common.Services;
using OneClickEcho.Domain.CampaignAggregate.Enums;
using OneClickEcho.Domain.CampaignAggregate.Repositories;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Repositories;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.TestMessageAggregate;
using OneClickEcho.Domain.TestMessageAggregate.Repositories;
using OneClickEcho.Domain.TestMessageAggregate.ValueObjects;

namespace OneClickEcho.Application.Campaign.Commands.TestCampaign
{
    public class TestCampaignHandler(
        ICampaignRepository campaignRepository,
        IMessageSendingService messageSendingService,
        ITestMessageRepository testMessageRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<TestCampaignCommand, TestCampaignResponse>
    {
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly IMessageSendingService _messageSendingService = messageSendingService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITestMessageRepository _testMessageRepository = testMessageRepository;

        public async Task<Result<TestCampaignResponse>> Handle(TestCampaignCommand request, CancellationToken cancellationToken)
        {
            // get campaign
            Domain.CampaignAggregate.Campaign? campaign = await _campaignRepository
                .GetByIdAsync(CampaignId.Create(request.CampaignId), cancellationToken);

            if (campaign is null)
            {
                return Result.Failure<TestCampaignResponse>(new Error(
                    "Campaign.NotFound",
                    $"The Campaign with Id:\"{request.CampaignId}\" does not exist."
                ));
            }

            // check if campaign is draft
            if (campaign.Status != CampaignStatus.Draft)
            {
                return Result.Failure<TestCampaignResponse>(new Error(
                    "Campaign.BadRequest",
                    $"The Campaign is already scheduled."
                ));
            }

            // check if campaign has selected Viber or SMS channel
            if (!campaign.IsViber && !campaign.IsSms)
            {
                return Result.Failure<TestCampaignResponse>(new Error(
                    "Campaign.BadRequest",
                    $"The Campaign doesn't have selected channel."
                ));
            }
            
            Random random = new();
            
            // Test message
            TestMessage testMessage = new TestMessage(
                TestMessageId.CreateUnique(),
                campaign.CompanyId,
                campaign.IsViber,
                request.PhoneNumber,
                random.NextInt64(10000000000000, long.MaxValue)
            );
            
            _testMessageRepository.Add(testMessage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            await _messageSendingService.SendTestMessages(campaign, testMessage);

            return new TestCampaignResponse(campaign.Id.Value);
        }
    }
}
