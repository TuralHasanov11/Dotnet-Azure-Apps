using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace WebApp.Services;

public class LearningServiceClient: ILearningServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;

    public LearningServiceClient(HttpClient httpClient,
        ITokenAcquisition tokenAcquisition,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _tokenAcquisition = tokenAcquisition;
        _configuration = configuration;

        _httpClient.BaseAddress = new Uri(_configuration["LearningServiceApiEndpoint"]!);
    }

    public async Task<string> GetAuthorizedAsync()
    {
        var scopes = _configuration["ApiScopes"]?.Split(' ')!;

        string accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync("/api/authorized");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}