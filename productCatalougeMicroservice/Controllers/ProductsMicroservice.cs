using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using productCatalougeMicroservice.Models;
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

        [HttpPost("LoadProducts/{category}")]
        public async Task<List<Product>> LoadProducts(string category)
        {
            string requestUri = "https://amazon-data-scraper124.p.rapidapi.com/search/"+category+"?api_key=393ff3e8406791ceb99f57836935aa52";

            JsonDocument jsonDoc = GetResponse(requestUri).Result;

            string jsonString = jsonDoc.RootElement.ToString();

            var jsonData = JsonConvert.DeserializeObject<RootObject>(jsonString);

            // Create a list to hold the products
            List<Product> products = new List<Product>();

            // Iterate through the results and create Product objects
            foreach (var result in jsonData.results)
            {
                Product product = new Product
                {
                    Image = result.image,
                    Name = result.name,
                    price = result.price_string,
                    Url = result.url
                };

                products.Add(product);
            }

            return products;
        }

        [HttpGet("GetProductDetails/{url}")]
        public ProblemDetails GetProductDetails(string url)
        {
            string id = ExtractProductId(url);
            string requestUri = "https://amazon-data-scraper124.p.rapidapi.com/products/"+id+"?api_key=393ff3e8406791ceb99f57836935aa52";

            JsonDocument jsonDoc = GetResponse(requestUri).Result;

            string jsonString = jsonDoc.RootElement.ToString();

            ProblemDetails problemDetails = new ProblemDetails();


            return problemDetails;
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
