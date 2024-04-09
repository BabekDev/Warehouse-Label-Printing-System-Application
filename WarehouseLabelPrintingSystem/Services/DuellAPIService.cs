using System.Net.Http.Headers;
using System.Net.Http;
using System.Windows;
using Newtonsoft.Json;

namespace WarehouseLabelPrintingSystem.Services
{
    public class DuellAPIService
    {
        public readonly HttpClient _httpClient = new();
        private const string clientNumber = "client_number=211922&client_token=08383741664a8983416f6d0dfc6fa775";
        private const string urlProductList = "https://api.kasseservice.no/v1/product/list?length=20&filter%5Bproduct_number%5D=11001";

        public async Task<string> GetAccessToken()
        {
            try
            {
                string urlToken = "https://api.kasseservice.no/v1/getaccesstokens";
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
