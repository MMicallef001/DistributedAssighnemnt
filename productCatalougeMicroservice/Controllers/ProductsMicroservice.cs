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
                    { "X-RapidAPI-Host", "axesso-axesso-amazon-data-service-v1.p.rapidapi.com" },
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

            //"https://axesso-axesso-amazon-data-service-v1.p.rapidapi.com/amz/amazon-search-by-keyword-asin?domainCode=de&keyword=Laptop&page=1&excludeSponsored=false&sortBy=relevanceblender&withCache=true"


            //string requestUri = "https://amazon-data-scraper124.p.rapidapi.com/search/"+category+"?api_key=393ff3e8406791ceb99f57836935aa52";

            string requestUri = "https://axesso-axesso-amazon-data-service-v1.p.rapidapi.com/amz/amazon-search-by-keyword-asin?domainCode=de&keyword="+category+"&page=1&excludeSponsored=false&sortBy=relevanceblender&withCache=true";

            JsonDocument jsonDoc =  GetResponse(requestUri).Result;

            string jsonString = jsonDoc.RootElement.ToString();

            var jsonData = JsonConvert.DeserializeObject<RootObject>(jsonString);

            // Create a list to hold the products
            /*
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
            */
            
            List<Product> products = jsonData.searchProductDetails.Select(p => new Product
            {
                Asin = p.asin,
                Image = p.imgUrl,
                Name = p.productDescription,
                price = p.price.ToString(),
                Url = p.dpUrl
            }).ToList();

            return products;
        }

        [HttpGet("{asin}")]
        public ProductDetail GetProductDetails(string asin)
        {
            //string id = ExtractProductId(url);

            //if (string.IsNullOrEmpty(id))
            //{
            //    return null;
            //}

            //string requestUri = "https://amazon-data-scraper124.p.rapidapi.com/products/"+id+"?api_key=393ff3e8406791ceb99f57836935aa52";

            string requestUri = "https://axesso-axesso-amazon-data-service-v1.p.rapidapi.com/amz/amazon-search-by-keyword-asin?domainCode=de&keyword=" + asin + "&page=1&excludeSponsored=false&sortBy=relevanceblender&withCache=true";


            JsonDocument jsonDoc = GetResponse(requestUri).Result;

            string jsonString = jsonDoc.RootElement.ToString();

            JObject jsonObj = JObject.Parse(jsonString);

            var productDetailsData = jsonObj["searchProductDetails"].First;


            ProductDetail productDetails = new ProductDetail
            {
                Id= asin,
                Pricing = productDetailsData["price"].ToString(),
                ProductName = (string)productDetailsData["productDescription"],
                FullDescription = (string)productDetailsData["productDescription"],
                Image = (string)productDetailsData["imgUrl"],
                Asin = (string)productDetailsData["asin"]

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
