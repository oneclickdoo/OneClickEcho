using Microsoft.Extensions.Options;
using OneClickEcho.Application.Common.Services.GptService;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.GptRequestAggregate;
using OneClickEcho.Domain.GptRequestAggregate.Enums;
using OneClickEcho.Domain.NounCaseAggregate;
using OneClickEcho.Domain.NounCaseAggregate.Repositories;
using OneClickEcho.Infrastructure.Settings;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace OneClickEcho.Infrastructure.Services.GptService;

public class ConvertNounCasesStrategy(IHttpClientFactory httpClientFactory, IOptions<OpenAiSettings> openAiSettings,
    INounCaseRepository nounCaseRepository) : IGptRequestStrategy
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IOptions<OpenAiSettings> _openAiSettings = openAiSettings;
    private readonly INounCaseRepository _nounCaseRepository = nounCaseRepository;

    public async Task<Result<GptRequest>> SendGptRequestAsync(GptRequestDto request, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("OpenAiHttpClient");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync("/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(new
                {
                    model = _openAiSettings.Value.Model,
                    messages = new object[]
                    {
                        new
                        {
                            role = "system",
                            content = "Pretvori sledeće ime u vokativ, vodeći računa o pravilima srpskog jezika i specifičnim " +
                                      "slučajevima (npr. 'Predrag' postaje 'Predraže'). Nemoj menjati ime ako nije tipično za balkanski " +
                                      "prostor ili ako je jasno da nije deo slovenske grupe imena. Ako nisi siguran, vrati ime nepromenjeno. " +
                                      "Vrati samo ime, ništa više. Takođe, treba da se koriste oblici koji se najviše koriste (npr. " +
                                      "vokativ imena Branka je Branka, ne Branko...) i ne menjaj strane oblike u srpsku verziju (npr. " +
                                      "Anastasiia ostavi Anastasiia, ne pretvaraj u Anastasija)"
                        },
                        new
                        {
                            role = "user",
                            content = $"Ime: {request.RequestMessage}"
                        }
                    }
                }), Encoding.UTF8, "application/json"),
                cancellationToken);
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

        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        ChatCompletionResponseDto? responseContent = JsonSerializer
            .Deserialize<ChatCompletionResponseDto>(responseStream, options);

        if (responseContent == null)
        {
            return Result.Failure<GptRequest>(new Error(
                "GptRequest.BadRequest",
                "The GptRequest could not be deserialized."
            ));
        }

        NounCase? prev = await _nounCaseRepository.GetByNominativeAsync(request.RequestMessage, cancellationToken);

        if (prev is null)
        {
            _nounCaseRepository.Add(new NounCase
            {
                Nominative = request.RequestMessage,
                Vocative = responseContent.Choices[0].Message.Content
            });
        }

        return new GptRequest(
            request.RequestMessage,
            responseContent.Choices[0].Message.Content,
            GptRequestType.ConvertNounCases,
            null,
            null
        );
    }
}