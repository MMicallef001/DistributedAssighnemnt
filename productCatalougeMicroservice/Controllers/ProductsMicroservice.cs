using Common;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using productCatalougeMicroservice.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

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

        [HttpGet("{url}")]
        public ProductDetail GetProductDetails(string url)
        {
            string id = ExtractProductId(url);

            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            string requestUri = "https://amazon-data-scraper124.p.rapidapi.com/products/"+id+"?api_key=393ff3e8406791ceb99f57836935aa52";

            JsonDocument jsonDoc = GetResponse(requestUri).Result;

            string jsonString = jsonDoc.RootElement.ToString();

            JObject jsonObj = JObject.Parse(jsonString);

            ProductDetail productDetails = new ProductDetail
            {
                Id= id,
                Brand = (string)jsonObj["brand"],
                Pricing = (string)jsonObj["pricing"],
                ProductName = (string)jsonObj["name"],
                FullDescription = (string)jsonObj["full_description"],
                ShippingPrice = (string)jsonObj["shipping_price"],
                EncodedUrl = url,
                Image = (string)jsonObj["images"][0] 
            };

            return productDetails;
        }

        private string ExtractProductId(String url)
        {
            string pattern = @"/dp/(\w+)/";

            string encodedUrl = HttpUtility.UrlDecode(url);

            Regex regex = new Regex(pattern);
            Match match = regex.Match(encodedUrl);

            if (match.Success)
            {
                string extractedString = match.Groups[1].Value;
                return extractedString;
            }
            return null;
        }
    }
}
