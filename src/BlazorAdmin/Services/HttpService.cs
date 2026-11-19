using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared;
using Microsoft.Extensions.Options;

namespace BlazorAdmin.Services;

public class HttpService
{
    private readonly HttpClient _httpClient;
    private readonly ToastService _toastService;
    private readonly string _apiUrl;


    public HttpService(HttpClient httpClient, IOptions<BaseUrlConfiguration> baseUrlConfiguration, ToastService toastService)
    {
        _httpClient = httpClient;
        _toastService = toastService;
        _apiUrl = baseUrlConfiguration.Value.ApiBase;
    }

    public async Task<T> HttpGet<T>(string uri)
        where T : class
    {
        var result = await _httpClient.GetAsync($"{_apiUrl}{uri}");
        if (!result.IsSuccessStatusCode)
        {
            return null;
        }

        return await FromHttpResponseMessage<T>(result);
    }

    public async Task<T> HttpDelete<T>(string uri, int id)
        where T : class
    {
        var result = await _httpClient.DeleteAsync($"{_apiUrl}{uri}/{id}");
        if (!result.IsSuccessStatusCode)
        {
            return null;
        }

        return await FromHttpResponseMessage<T>(result);
    }

    public async Task<T> HttpDelete<T>(string uri, string id)
    where T : class
    {
        var result = await _httpClient.DeleteAsync($"{_apiUrl}{uri}/{id}");
        if (!result.IsSuccessStatusCode)
        {
            await ShowErrorToast(result);
            return null;
        }
        _toastService.ShowToast($"Deleted successfully!", ToastLevel.Success);

        return await FromHttpResponseMessage<T>(result);
    }

    public async Task<HttpStatusCode> HttpDelete(string uri)
    {
        var result = await _httpClient.DeleteAsync($"{_apiUrl}{uri}");
        if (!result.IsSuccessStatusCode)
        {
            await ShowErrorToast(result);
            return result.StatusCode;
        }
        _toastService.ShowToast($"Deleted successfully!", ToastLevel.Success);

        return result.StatusCode;
    }

    public async Task<T> HttpDelete<T>(string uri)
    where T : class
    {
        var result = await _httpClient.DeleteAsync($"{_apiUrl}{uri}");
        if (!result.IsSuccessStatusCode)
        {
            await ShowErrorToast(result);
            return null;
        }
        _toastService.ShowToast($"Deleted successfully!", ToastLevel.Success);

        return await FromHttpResponseMessage<T>(result);
    }

    public async Task<T> HttpPost<T>(string uri, object dataToSend)
        where T : class
    {
        var content = ToJson(dataToSend);

        var result = await _httpClient.PostAsync($"{_apiUrl}{uri}", content);
        if (!result.IsSuccessStatusCode)
        {
            await ShowErrorToast(result);

            return null;
        }
        _toastService.ShowToast($"Created successfully!", ToastLevel.Success);

        return await FromHttpResponseMessage<T>(result);
    }

    public async Task<T> HttpPut<T>(string uri, object dataToSend)
        where T : class
    {
        var content = ToJson(dataToSend);

        var result = await _httpClient.PutAsync($"{_apiUrl}{uri}", content);
        if (!result.IsSuccessStatusCode)
        {
            await ShowErrorToast(result);
            return null;
        }
        _toastService.ShowToast($"Updated successfully!", ToastLevel.Success);

        return await FromHttpResponseMessage<T>(result);
    }

    public async Task<HttpStatusCode> HttpPut(string uri, object dataToSend)
    {
        var content = ToJson(dataToSend);

        var result = await _httpClient.PutAsync($"{_apiUrl}{uri}", content);
        if (!result.IsSuccessStatusCode)
        {
            await ShowErrorToast(result);
            return result.StatusCode;
        }
        _toastService.ShowToast($"Updated successfully!", ToastLevel.Success);

        return result.StatusCode;
    }

    private async Task ShowErrorToast(HttpResponseMessage result)
    {
        var body = await result.Content.ReadAsStringAsync();
        _toastService.ShowToast(GetErrorMessage(body) ?? $"The request failed ({(int)result.StatusCode}).", ToastLevel.Error);
    }

    // FastEndpoints validation errors put a generic "message" ("One or more errors occurred!")
    // next to the useful details in "errors" (property name -> messages), so the first of those
    // wins. ExceptionMiddleware errors only have "message". Some errors (e.g. 404) have no body.
    private static string GetErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        JsonDocument json;
        try
        {
            json = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            return null; // Not JSON, so there's no message to show.
        }

        using (json)
        {
            if (json.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (TryGetPropertyIgnoreCase(json.RootElement, "errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in errors.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }
                    foreach (var error in property.Value.EnumerateArray())
                    {
                        if (error.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(error.GetString()))
                        {
                            return error.GetString();
                        }
                    }
                }
            }

            if (TryGetPropertyIgnoreCase(json.RootElement, "message", out var message)
                && message.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(message.GetString()))
            {
                return message.GetString();
            }

            return null;
        }
    }

    // FastEndpoints writes camelCase; ExceptionMiddleware writes ErrorDetails in PascalCase.
    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private StringContent ToJson(object obj)
    {
        return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
    }

    private async Task<T> FromHttpResponseMessage<T>(HttpResponseMessage result)
    {
        return JsonSerializer.Deserialize<T>(await result.Content.ReadAsStringAsync(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
