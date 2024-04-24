using System.Net.Http.Headers;
using System.Net.Http;
using System.Windows;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Text;

namespace WarehouseLabelPrintingSystem.Services
{
    /// <summary>
    /// Service class for interacting with the Duell API. Handles token retrieval and fetching product lists.
    /// </summary>
    public class DuellAPIService
    {
        private readonly IConfiguration _configuration; // Configuration settings from appsettings.json
        private readonly HttpClient _httpClient = new(); // HTTP client for making API calls
        private readonly string _clientNumber; // Client number for authentication
        private readonly string _urlProductList; // URL for fetching the product list
        private readonly ILogger<DuellAPIService> _logger; // Logger for this service

        public DuellAPIService()
        {
            // Set up configuration from appsettings.json
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = configBuilder.Build();

            _clientNumber = _configuration["Authentication:ClientNumber"]!;
            _urlProductList = _configuration["Authentication:ProductListURL"]!;

            // Logger instance for logging API-related actions
            _logger = App.LoggerFactory!.CreateLogger<DuellAPIService>();

            _logger.LogInformation("DuellAPIService initialized.");
        }

        /// <summary>
        /// Retrieves an access token from the API for authentication.
        /// </summary>
        /// <returns>The access token as a string, or an error message in case of failure.</returns>
        public async Task<string> GetAccessToken()
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve access token.");

                // URL for getting the access token
                string urlToken = _configuration["Authentication:TokenURL"]!;
                var requestBody = new StringContent(_clientNumber, Encoding.UTF8, "application/x-www-form-urlencoded");

                // Send POST request to retrieve the token
                var response = await _httpClient.PostAsync(urlToken, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    // Log and return an error message if the request fails
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to retrieve access token. Status: {response.StatusCode}, Details: {errorContent}");
                    return $"Error: {response.StatusCode}";
                }

                // Parse the response to extract the token
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string token = tokenData?.token ?? string.Empty;

                _logger.LogInformation("Access token retrieved successfully.");
                return token;
            }
            catch (Exception ex)
            {
                // Log the exception and return the error message
                _logger.LogError(message: "An error occurred while retrieving the access token.", ex);
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves the product list from the API using the provided access token.
        /// </summary>
        /// <param name="token">Access token for authentication.</param>
        /// <returns>The product list as a JSON string, or null if the request fails.</returns>
        public async Task<string?> GetProductList(string token)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve product list.");

                // Set up the HTTP request
                var request = new HttpRequestMessage(HttpMethod.Get, _urlProductList);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Send the request and get the response
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Product list retrieved successfully.");
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
                else
                {
                    // Log the failure and display a message box
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to retrieve product list. Status: {response.StatusCode}, Details: {errorDetails}");
                    MessageBox.Show($"Failed to get response: {response.StatusCode}\n {errorDetails}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log the exception and inform the user
                _logger.LogError(message: "An error occurred while retrieving the product list.", ex);
                MessageBox.Show($"Error: {ex.Message}");
                return null;
            }
        }
    }
}
