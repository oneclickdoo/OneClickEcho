using Microsoft.Extensions.Options;
using OneClickEcho.Application.Common.Services.GptService;
using OneClickEcho.Domain.CampaignAggregate.ValueObjects;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.GptRequestAggregate;
using OneClickEcho.Domain.GptRequestAggregate.Enums;
using OneClickEcho.Infrastructure.Settings;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace OneClickEcho.Infrastructure.Services.GptService;

public class GenerateNewCampaignMessageStrategy(IHttpClientFactory httpClientFactory, IOptions<OpenAiSettings> openAiSettings)
    : IGptRequestStrategy
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IOptions<OpenAiSettings> _openAiSettings = openAiSettings;

    public async Task<Result<GptRequest>> SendGptRequestAsync(GptRequestDto request, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("OpenAiHttpClient");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync("/v1/chat/completions", new StringContent(JsonSerializer.Serialize(new
            {
                model = _openAiSettings.Value.Model,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "You are an assistant for writing phone message campaigns that are intended to be sent out" +
                                  "either via SMS or other messaging apps. The user will provide a short description in" +
                                  "either Serbian or English containing a general description about what the campaign should be about." +
                                  "Your reply should be in Serbian, should contain diacritical characters wherever needed, and can contain" +
                                  "the following two placeholders: {firstName:vocative} and/or {lastName:vocative} when addressing the consumer directly," +
                                  "{firstName:nominative} and/or {lastName:nominative} otherwise, but neither of these are mandatory."
                    },
                    new
                    {
                        role = "user",
                        content = request.RequestMessage
                    }
                }
            }), Encoding.UTF8, "application/json"), cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is IOException
            || (ex is TaskCanceledException t && !t.CancellationToken.IsCancellationRequested))
        {
            return OpenAiConnectionErrors.AsFailure(ex, httpClient.BaseAddress);
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return Result.Failure<GptRequest>(new Error(
                "GptRequest.BadRequest",
                "The GptRequest failed with status code: " + response.StatusCode
            ));
        }

        string responseStream;
        try
        {
            responseStream = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is IOException
            || (ex is TaskCanceledException t && !t.CancellationToken.IsCancellationRequested))
        {
            return OpenAiConnectionErrors.AsFailure(ex, httpClient.BaseAddress);
        }

        ChatCompletionResponseDto? responseContent = JsonSerializer.Deserialize<ChatCompletionResponseDto>(responseStream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return responseContent == null
            ? Result.Failure<GptRequest>(new Error(
                "GptRequest.BadRequest",
                "The GptRequest could not be deserialized."
            ))
            : (Result<GptRequest>)new GptRequest(
            request.RequestMessage,
            responseContent.Choices[0].Message.Content,
            GptRequestType.GenerateNewCampaignMessage,
            CampaignId.Create(request.CampaignId!.Value), // Safe to use null suppression because validation will run first 
            null
        );
    }
}