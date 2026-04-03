using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.CompanyAggregate.ValueObjects
{
    public sealed class SenderId : EntityId<Guid>
    {
        public override Guid Value { get; protected set; }

        private SenderId(Guid value)
        {
            Value = value;
        }

        public static new SenderId Create(Guid value)
        {
            return new SenderId(value);
        }

        public static new SenderId CreateUnique()
        {
            return new SenderId(Guid.NewGuid());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
