using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace WorkerService1.BulkTradeService;

public class PathOfExileApiClient : ITradeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PathOfExileApiClient> _logger;
    private readonly RateLimiter _rateLimiter;

    public PathOfExileApiClient(
        HttpClient httpClient, 
        ILogger<PathOfExileApiClient> logger,
        RateLimiter rateLimiter)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri("https://www.pathofexile.com");
        ConfigureHttpClientHeaders();
    }

    private void ConfigureHttpClientHeaders()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("flippingexiles/1.0");
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Clear();
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        _httpClient.DefaultRequestHeaders.AcceptCharset.Clear();
        _httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
    }

    public async Task<TradeResponse> ExecuteBulkSearchAsync(string leagueId, string haveCurrency, string wantCurrency, int minimum)
    {
        _logger.LogInformation("Executing bulk trade search for league: {LeagueId}", leagueId);
        _logger.LogDebug("Have Currency: {HaveCurrency}, Want Currency: {WantCurrency}, Minimum: {Minimum}", 
            haveCurrency, wantCurrency, minimum);

        try
        {
            // Apply rate limiting
            await _rateLimiter.WaitAsync();

            var url = $"/api/trade/exchange/{leagueId}";
            var requestBody = BuildTradeRequest(haveCurrency, wantCurrency, minimum);
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = httpContent;
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();
            LogResponseHeaders(response);

            return await DeserializeResponseAsync<TradeResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing bulk trade search.");
            throw;
        }
    }

    private TradeRequest BuildTradeRequest(string haveCurrency, string wantCurrency, int minimum)
    {
        return new TradeRequest
        {
            query = new TradeQuery
            {
                status = new TradeStatus { option = "online" },
                have = new[] { haveCurrency },
                want = new[] { wantCurrency },
                minimum = minimum
            }
        };
    }

    private void LogResponseHeaders(HttpResponseMessage response)
    {
        foreach (var header in response.Headers)
        {
            _logger.LogDebug("Header: {Key} = {Value}", header.Key, string.Join(", ", header.Value));
        }

        if (response.Content.Headers.TryGetValues("Content-Type", out var contentTypes))
        {
            _logger.LogDebug("Content-Type: {ContentType}", string.Join(", ", contentTypes));
        }

        if (response.Content.Headers.TryGetValues("Content-Encoding", out var contentEncodings))
        {
            _logger.LogDebug("Content-Encoding: {ContentEncoding}", string.Join(", ", contentEncodings));
        }
    }

    private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        using var responseStream = await response.Content.ReadAsStreamAsync();
        using var decompressedStream = GetDecompressedStream(response, responseStream);
        using var reader = new StreamReader(decompressedStream, Encoding.UTF8);

        var rawResponse = await reader.ReadToEndAsync();
        _logger.LogDebug("Raw API Response: {RawResponse}", rawResponse);

        var result = JsonConvert.DeserializeObject<T>(rawResponse);
        return result;
    }

    private Stream GetDecompressedStream(HttpResponseMessage response, Stream responseStream)
    {
        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            return new GZipStream(responseStream, CompressionMode.Decompress);
        }
        else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
        {
            return new DeflateStream(responseStream, CompressionMode.Decompress);
        }
        else if (response.Content.Headers.ContentEncoding.Contains("br"))
        {
            return new BrotliStream(responseStream, CompressionMode.Decompress);
        }
        else
        {
            return responseStream;
        }
    }
}