using Microsoft.AspNetCore.Identity;
using OneClickEcho.Domain.Common.Primitives;

namespace OneClickEcho.Domain.Common.Repositories;

public interface IRepository<T, TId> : IRepositoryGenericType<T, TId, Guid>
    where T : AggregateRoot<TId>
    where TId : AggregateRootId<Guid>
{ }

// Specific implementation for Identity
public interface IRepository<T> where T : IdentityUser<Guid>
{ }
