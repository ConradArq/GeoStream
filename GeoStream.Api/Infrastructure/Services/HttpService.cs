using System.Text;
using System.Text.Json;
using GeoStream.Api.Infrastructure.Interfaces.Services;

namespace GeoStream.Api.Infrastructure.Services
{
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Generic method to send HTTP requests to an API (supports GET, POST, PUT, DELETE, form-data, URL-encoded, and JSON payloads).
        /// - For <typeparamref name="T"/> as a specific object: The response will be deserialized into the specified object type.
        /// - For <typeparamref name="T"/> as a string: The raw response content will be returned, useful when the response is not JSON or the type is unknown.
        /// - Automatically handles query parameters, request headers, and different content types (e.g., JSON, form-data, URL-encoded).
        /// 
        /// Usage Examples:
        /// - await SendAsync<MyResponseDto>(HttpMethod.Get, "https://api.example.com/resource");
        /// - await SendAsync<string>(HttpMethod.Get, "https://api.example.com/resource");
        /// </summary>
        public async Task<T> SendAsync<T>(
            HttpMethod method,
            string endpoint,
            object? payload = null,
            string? contentType = null,
            Dictionary<string, string>? headers = null,
            Dictionary<string, string>? queryParams = null)
        {
            var httpClient = _httpClientFactory.CreateClient();

            if (queryParams != null && queryParams.Any())
            {
                endpoint += "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            }

            var request = new HttpRequestMessage(method, endpoint);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (payload != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Delete))
            {
                if (contentType == "application/json")
                {
                    var jsonPayload = JsonSerializer.Serialize(payload);
                    request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                }
                else if (contentType == "application/x-www-form-urlencoded")
                {
                    if (payload is Dictionary<string, string> formFields)
                    {
                        request.Content = new FormUrlEncodedContent(formFields);
                    }
                    else
                    {
                        throw new ArgumentException("Payload must be a Dictionary<string, string> for URL-encoded content.");
                    }
                }
                else if (contentType == "multipart/form-data")
                {
                    if (payload is MultipartFormDataContent formDataContent)
                    {
                        request.Content = formDataContent;
                    }
                    else
                    {
                        throw new ArgumentException("Payload must be a MultipartFormDataContent for form-data content.");
                    }
                }
                else
                {
                    throw new ArgumentException("Unsupported content type.");
                }
            }

            var response = await httpClient.SendAsync(request);

            return await HandleResponse<T>(request, response);
        }

        /// <summary>
        /// Processes the HTTP response and deserializes the result into the specified type
        /// </summary>
        private async Task<T> HandleResponse<T>(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                var requestDetails = $@"
                    Request:
                    - URL: {request.RequestUri}
                    - Method: {request.Method}
                    - Headers: {string.Join("; ", request.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}
                    - Content: {(request.Content != null ? await request.Content.ReadAsStringAsync() : "No Content")}

                    Response:
                    - Status Code: {response.StatusCode}
                    - Reason Phrase: {response.ReasonPhrase}
                    - Headers: {string.Join("; ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}
                    - Content: {errorContent}
                    ";

                throw new Exception($"API Error: {requestDetails}");
            }

            var responseData = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseData))
            {
                return default!;
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)responseData;
            }

            return JsonSerializer.Deserialize<T>(responseData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }
    }
}
