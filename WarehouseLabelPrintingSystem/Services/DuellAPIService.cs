using System.Net.Http.Headers;
using System.Net.Http;
using System.Windows;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;

namespace WarehouseLabelPrintingSystem.Services
{
    public class DuellAPIService
    {
        private readonly IConfiguration _configuration;
        public readonly HttpClient _httpClient = new();
        private readonly string clientNumber = "";
        private readonly string urlProductList = "";

        public DuellAPIService()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true, true);

            _configuration = builder.Build();

            clientNumber = _configuration["Authentication:ClientNumber"];
            urlProductList = _configuration["Authentication:ProductListURL"];
        }

        public async Task<string> GetAccessToken()
        {
            try
            {
                string urlToken = _configuration["Authentication:TokenURL"];
                var requestBody = new StringContent(clientNumber, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

                var responseToken = await _httpClient.PostAsync(urlToken, requestBody);
                var tokenData = JsonConvert.DeserializeObject<dynamic>(await responseToken.Content.ReadAsStringAsync());

                return tokenData!.token;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string?> GetProductList(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, urlProductList);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            else
            {
                MessageBox.Show($"Failed to get response: {response.StatusCode}\n {await response.Content.ReadAsStringAsync()}");
                return null;
            }
        }
    }
}
