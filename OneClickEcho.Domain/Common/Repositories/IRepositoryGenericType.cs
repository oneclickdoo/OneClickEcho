using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.Common.Repositories;

public interface IRepositoryGenericType<T, TId, TIdType>
    where T : AggregateRootGenericType<TId, TIdType>
    where TId : AggregateRootId<TIdType>
{

}