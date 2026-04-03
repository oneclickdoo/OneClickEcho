using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.GptRequest.Commands.ConvertNounCases;

public sealed record ConvertNounCasesCommand(string Name) : ICommand<ConvertNounCasesResponse>;