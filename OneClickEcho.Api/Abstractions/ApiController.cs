using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OneClickEcho.App.Abstractions
{
    [ApiController]
    public abstract class ApiController(IMediator mediator) : ControllerBase
    {
        protected readonly IMediator Mediator = mediator;
    }
}