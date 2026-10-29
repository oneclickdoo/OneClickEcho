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
            SenderType type,
            bool isTwoWay = false) : base(SenderId.CreateUnique())
        {
            CompanyId = companyId;
            Name = name;
            Type = type;
            IsTwoWay = type == SenderType.Viber && isTwoWay;
        }

        public CompanyId CompanyId { get; set; } = default!;

        public string Name { get; set; } = string.Empty;

        public SenderType Type { get; set; }

        /// <summary>Comtrade/Viber service is opened for two-way replies (types 206–210 and survey 801).</summary>
        public bool IsTwoWay { get; set; }

        // Used for EFCore
        public Sender() : base(SenderId.CreateUnique())
        {
        }
    }
}
