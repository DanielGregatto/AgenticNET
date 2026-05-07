using Domain.Configs;
using Domain.Contracts.API;
using Microsoft.Extensions.Options;
using Services.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class TurnstileValidatorService : ITurnstileValidatorService
{
    private readonly HttpClient _httpClient;
    private readonly TurnstileConfig _config;

    public TurnstileValidatorService(HttpClient httpClient, IOptions<TurnstileConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<bool> ValidateAsync(string token)
    {
        var values = new Dictionary<string, string>
        {
            { "secret", _config.SecretKey },
            { "response", token }
        };

        var response = await _httpClient.PostAsync(
            "https://challenges.cloudflare.com/turnstile/v0/siteverify",
            new FormUrlEncodedContent(values)
        );

        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TurnstileValidationDto>(json);

        return result?.success == true;
    }
}
