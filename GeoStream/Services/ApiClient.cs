using System.Text;
using Newtonsoft.Json;
using GeoStream.Dtos;

namespace GeoStream.Services
{
    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<ApiClient> _logger;
        private readonly Dictionary<string, (string Url, HttpMethod Method)> _endpointMappings = new Dictionary<string, (string, HttpMethod)>();

        public ApiClient(IHttpClientFactory clientFactory, ILogger<ApiClient> logger, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            LoadEndpointMappings(configuration);
        }

        private void LoadEndpointMappings(IConfiguration configuration)
        {
            var apis = configuration.GetSection("ExternalApis").GetChildren();

            foreach (var api in apis)
            {
                var baseUrl = api["BaseUrl"];
                var controllers = api.GetSection("Endpoints").GetChildren();

                var controllerCommon = api.GetSection("Endpoints:Common");

                Dictionary<string, (string, HttpMethod)> commonActionDictionary = new Dictionary<string, (string, HttpMethod)>();

                foreach (var action in controllerCommon.GetSection("Actions").GetChildren())
                {
                    var path = action["Path"] ?? string.Empty;
                    var method = action["Method"] ?? string.Empty;

                    if (string.IsNullOrEmpty(method))
                    {
                        _logger.LogWarning("Method for common action not specified. The action will be ignored.");
                        continue;
                    }

                    commonActionDictionary[action.Key] = (path, new HttpMethod(method));
                }

                foreach (var controller in controllers)
                {
                    var controllerBasePath = baseUrl + controller["Path"];

                    //Loads common actions for each controller
                    foreach (var commonAction in commonActionDictionary)
                    {
                        var key = $"{api.Key}.{controller.Key}.{commonAction.Key}";
                        var method = commonAction.Value.Item2;
                        if (method == null)
                        {
                            _logger.LogWarning($"Method for {key} not specified. The default GET method will be used.");
                            method = new HttpMethod("GET");
                        }

                        var path = commonAction.Value.Item1;

                        _endpointMappings[key] = ($"{controllerBasePath}{path}", method);
                    }

                    //Loads specific actions for each controller
                    var actions = controller.GetSection("Actions").GetChildren();

                    foreach (var action in actions)
                    {
                        var key = $"{api.Key}.{controller.Key}.{action.Key}";
                        var methodValue = action["Method"];
                        if (string.IsNullOrEmpty(methodValue))
                        {
                            _logger.LogWarning($"Method for {key} not specified. The default GET method will be used.");
                            methodValue = "GET";
                        }
                        var method = new HttpMethod(methodValue);
                        var path = action["Path"];
                        _endpointMappings[key] = ($"{controllerBasePath}{path}", method);
                    }
                }


            }
        }

        public async Task<TResponse> SendRequest<TResponse>(string key, object? content = null,
            Dictionary<string, string>? pathParams = null, Dictionary<string, string>? queryParams = null)
            where TResponse : IApiResponseDto, new()
        {
            var apiResponseDto = new TResponse();

            if (!_endpointMappings.TryGetValue(key, out var endpointInfo))
            {
                _logger.LogError("No mapping found for the endpoint {Key}", key);
                apiResponseDto.Errors = new List<string>() { $"No mapping found for the endpoint {key}" };
                return apiResponseDto;
            }

            var url = endpointInfo.Url;
            if (pathParams != null && pathParams.Any() && pathParams.All(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Value)))
            {
                url = pathParams.Aggregate(url, (current, param) => current.Replace($"{{{param.Key}}}", param.Value));
            }

            if (queryParams != null && queryParams.Any() && queryParams.All(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Value)))
            {
                var queryBuilder = new StringBuilder(url.Contains('?') ? "&" : "?");
                foreach (var param in queryParams)
                {
                    queryBuilder.Append($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}&");
                }
                url += queryBuilder.ToString().TrimEnd('&');
            }

            var client = _clientFactory.CreateClient("BypassSSLClient");
            HttpRequestMessage request = new HttpRequestMessage(endpointInfo.Method, url);

            if (content != null && (endpointInfo.Method == HttpMethod.Post || endpointInfo.Method == HttpMethod.Put || 
                endpointInfo.Method == HttpMethod.Patch || endpointInfo.Method == HttpMethod.Delete))
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            }

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("The response from {Url} was not successful: {ResponseContent}. Status Code: {StatusCode}.", url, json, response.StatusCode);
                    apiResponseDto.Errors = new List<string>() { $"The operation was not successful." };
                    return apiResponseDto;
                }

                var apiResponseDtoResponse = JsonConvert.DeserializeObject<TResponse>(json);

                if (apiResponseDtoResponse == null)
                {
                    apiResponseDto.Errors = new List<string>() { "Error deserializing the API response." };
                    return apiResponseDto;
                }
                else
                {
                    return apiResponseDtoResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "The request to {Url} could not be made.", url);
                apiResponseDto.Errors = new List<string>() { $"The request to {url} could not be made." };
                return apiResponseDto;
            }
        }
    }
}
