using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;
using OneClickEcho.Domain.TestMessageAggregate.ValueObjects;

namespace OneClickEcho.Domain.TestMessageAggregate;

public sealed class TestMessage : AggregateRoot<TestMessageId>
{
    public TestMessage(
        TestMessageId id,
        CompanyId companyId,
        bool isViber,
        string phoneNumber,
        long viberId
        ) : base(id)
    {
        CompanyId = companyId;
        PhoneNumber = phoneNumber;
        ViberId = viberId;
        IsViber = isViber;
    }

    public CompanyId CompanyId { get; set; }
    public string PhoneNumber { get; set; }
    public long ViberId { get; set; }
    public string SmsReferenceId { get; set; } = string.Empty;
    public bool IsViber { get; set; } = false;
    public bool IsDelivered { get; set; } = false; 
    public bool IsClicked { get; set; } = false; 

    // Used for EFCore
    public TestMessage() : base(TestMessageId.CreateUnique()) { }
}