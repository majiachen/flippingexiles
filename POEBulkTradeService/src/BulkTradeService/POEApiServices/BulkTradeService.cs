using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace WorkerService1.BulkTradeService;

public class PathOfExileApiClient : ITradeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PathOfExileApiClient> _logger;

    public PathOfExileApiClient(HttpClient httpClient, ILogger<PathOfExileApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

    public async Task<TradeResponse> ExecuteBulkSearchAsync(string leagueId, string[] haveCurrencies, string wantCurrency, int minimum)
    {
        _logger.LogInformation("Executing bulk trade search for league: {LeagueId}", leagueId);
        _logger.LogDebug("Have Currencies: {HaveCurrencies}, Want Currency: {WantCurrency}, Minimum: {Minimum}", 
            string.Join(", ", haveCurrencies), wantCurrency, minimum);

        try
        {
            var url = $"/api/trade/exchange/{leagueId}";
            var requestBody = BuildTradeRequest(haveCurrencies, wantCurrency, minimum);
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = httpContent
            };

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            LogResponseHeaders(response);

            // **Update the rate limiter dynamically**
            UpdateRateLimit(response);

            return await DeserializeResponseAsync<TradeResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing bulk trade search.");
            throw;
        }
    }

    private void UpdateRateLimit(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("X-Rate-Limit-Ip", out var limitValues))
        {
            var limitParts = limitValues.First().Split(',')[^1].Split(':').Select(int.Parse).ToArray();
            int maxRequests = limitParts[0]; // Max requests
            int windowSeconds = limitParts[1]; // Time window

            RateLimiter.UpdateRateLimits(maxRequests, windowSeconds);
        }
    }

    private TradeRequest BuildTradeRequest(string[] haveCurrencies, string wantCurrency, int minimum)
    {
        return new TradeRequest
        {
            query = new TradeQuery
            {
                status = new TradeStatus { option = "online" },
                have = haveCurrencies, // Supports multiple have currencies
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
    }

    private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        using var responseStream = await response.Content.ReadAsStreamAsync();
        using var decompressedStream = GetDecompressedStream(response, responseStream);
        using var reader = new StreamReader(decompressedStream, Encoding.UTF8);

        var rawResponse = await reader.ReadToEndAsync();
        _logger.LogDebug("Raw API Response: {RawResponse}", rawResponse);

        return JsonConvert.DeserializeObject<T>(rawResponse);
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
