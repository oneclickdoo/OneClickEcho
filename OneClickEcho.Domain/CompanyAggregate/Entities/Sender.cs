using OneClickEcho.Domain.Common.Primitives;
using OneClickEcho.Domain.CompanyAggregate.Enums;
using OneClickEcho.Domain.CompanyAggregate.ValueObjects;

namespace OneClickEcho.Domain.CompanyAggregate.Entities
{
    public sealed class Sender : Entity<SenderId>
    {
        public Sender(
            CompanyId companyId,
            string name,
            SenderType type) : base(SenderId.CreateUnique())
        {
            CompanyId = companyId;
            Name = name;
            Type = type;
        }

        public CompanyId CompanyId { get; set; } = default!;

        public string Name { get; set; } = string.Empty;

        public SenderType Type { get; set; }

        // Used for EFCore
        public Sender() : base(SenderId.CreateUnique())
        {
        }
    }
}
