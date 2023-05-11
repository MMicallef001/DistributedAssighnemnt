using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace productCatalougeMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsMicroservice : ControllerBase
    {

        private async Task<JsonDocument> GetResponse(string requestURI)
        {

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(requestURI),
                Headers =
                {
                    { "X-RapidAPI-Key", "e06f6b3ae2msh236205caad93a4dp1c44b1jsn57b46b439a7d" },
                    { "X-RapidAPI-Host", "amazon-data-scraper124.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return JsonDocument.Parse(body);
            }
        }

        [HttpGet("LoadProducts/{category}")]
        public IActionResult LoadProducts(string category)
        {
            string requestUri = "https://amazon-data-scraper124.p.rapidapi.com/search/"+category+"?api_key=393ff3e8406791ceb99f57836935aa52";

            JsonDocument jsonDoc = GetResponse(requestUri).Result;

            string jsonString = jsonDoc.RootElement.ToString();

            return Content(jsonString, "application/json");
        }

        [HttpGet("{url}")]
        public IActionResult GetProductDetails(string url)
        {
            string id = ExtractProductId(url);
            string requestUri = "https://amazon-data-scraper124.p.rapidapi.com/products/"+id+"?api_key=393ff3e8406791ceb99f57836935aa52";

            JsonDocument jsonDoc = GetResponse(requestUri).Result;

            string jsonString = jsonDoc.RootElement.ToString();

            return Content(jsonString, "application/json");
        }

        private string ExtractProductId(String url)
        {
            string pattern = @"/dp/(\w+)/";

            Regex regex = new Regex(pattern);
            Match match = regex.Match(url);

            if (match.Success)
            {
                string extractedString = match.Groups[1].Value;
                return extractedString;
            }
            return null;
        }
    }
}
