using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // ⚠️ IMPORTANT: Ensure this port matches your API project exactly!
    private readonly string _baseUrl = "http://localhost:5000/api";

    public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    // ==========================================
    // COOKIE HELPERS
    // ==========================================

    private void AttachCookiesToRequest()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        // Get the cookie from the MVC browser
        var sessionCookie = context.Request.Cookies[".RaceDay.Auth"];

        if (!string.IsNullOrEmpty(sessionCookie))
        {
            // Clear old headers to avoid duplicate cookie errors
            _httpClient.DefaultRequestHeaders.Remove("Cookie");
            // Attach it to the API request
            _httpClient.DefaultRequestHeaders.Add("Cookie", $".RaceDay.Auth={sessionCookie}");
        }
    }

    private void SaveCookiesFromResponse(HttpResponseMessage response)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            foreach (var cookie in cookies)
            {
                if (cookie.StartsWith(".RaceDay.Auth="))
                {
                    // Extract just the name=value part, ignoring path/expiry for simplicity
                    var cookieValue = cookie.Split(';')[0];
                    var parts = cookieValue.Split('=');

                    if (parts.Length == 2)
                    {
                        // Save it to the MVC browser
                        context.Response.Cookies.Append(".RaceDay.Auth", parts[1], new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true, // Required for HTTPS
                            SameSite = SameSiteMode.Lax
                        });
                    }
                }
            }
        }
    }

    // ==========================================
    // GENERIC HTTP METHODS
    // ==========================================

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AttachCookiesToRequest();
        var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");

        SaveCookiesFromResponse(response); // Catch any cookie updates

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        return default;
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        AttachCookiesToRequest();

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", content);

        // This is crucial! It saves the API's session cookie to the browser.
        SaveCookiesFromResponse(response);

        return response;
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        AttachCookiesToRequest();

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"{_baseUrl}/{endpoint}", content);

        SaveCookiesFromResponse(response);
        return response;
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        AttachCookiesToRequest();
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/{endpoint}");
        SaveCookiesFromResponse(response);
        return response;
    }

    // ==========================================
    // FILE UPLOAD (For Part 3 Azure Blob Storage)
    // ==========================================
    public async Task<string?> UploadImageAsync(IFormFile file)
    {
        AttachCookiesToRequest();

        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        var response = await _httpClient.PostAsync($"{_baseUrl}/files/upload", content);

        SaveCookiesFromResponse(response);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("url").GetString();
        }
        return null;
    }
}