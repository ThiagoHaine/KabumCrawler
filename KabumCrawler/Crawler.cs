using HtmlAgilityPack;
using KabumCrawler.Classes;
using System.Net.Http.Headers;
using System;
using Newtonsoft.Json;
using KabumCrawler.DAL;
using KabumCrawler.Models;

namespace KabumCrawler
{
    public class Crawler
    {
        private string result;
        private string kabumUrl = "https://www.kabum.com.br";

        private string xpathFindByClass(string className)
        {
            return $".//*[contains(@class, '{className}')]";
        }

        public Crawler() { }

        public Crawler(string query) {
            var url = String.Format("{0}/busca/{1}?page_number=1&page_size=1000", kabumUrl, query);
            downloadInfo(url);
        }

        public List<KabumReview> crawlReviews(string productCode)
        {
            var url = String.Format("https://servicespub.prod.api.aws.grupokabum.com.br/opinioes/v2/opinioes/{0}?limit=5", productCode);
            var result = downloadInfo(url);
            var reviewList = JsonConvert.DeserializeObject<dynamic>(result);

            List<KabumReview> kabumReviews = new List<KabumReview>();

            foreach(var review in reviewList.opinioes)
            {
                KabumReview kabumReview = new KabumReview();
                kabumReview.avaliacao = review.avaliacao;
                kabumReview.titulo = review.titulo;
                kabumReview.data = review.data;
                kabumReview.opiniao = review.opiniao;
                kabumReview.info_negativo = review.info_negativo;
                kabumReview.info_positivo = review.info_positivo;

                kabumReviews.Add(kabumReview);
            }

            return kabumReviews;
        }

        private string downloadInfo(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.1234.5678 Safari/537.36");

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                    return result;
                }
            }

            return "";
        }

        public List<KabumProduct> parseData()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(result);

            var products = doc.DocumentNode.SelectNodes(xpathFindByClass("productCard"));

            List<KabumProduct> kabumProducts = new List<KabumProduct>();

            foreach (var htmlNode in products)
            {
                KabumProduct kabumProduct = new KabumProduct();

                kabumProduct.name = htmlNode.SelectNodes(xpathFindByClass("nameCard")).FirstOrDefault().InnerHtml;
                kabumProduct.image = htmlNode.Descendants("img").FirstOrDefault().OuterHtml;
                kabumProduct.price = htmlNode.SelectNodes(xpathFindByClass("priceCard")).FirstOrDefault().InnerHtml;
                kabumProduct.link = kabumUrl + htmlNode.Descendants("a").FirstOrDefault().Attributes["href"].Value;
                kabumProduct.oldPrice = htmlNode.SelectNodes(xpathFindByClass("oldPriceCard")).FirstOrDefault().InnerHtml;
                kabumProduct.isPromotional = !String.IsNullOrEmpty(kabumProduct.oldPrice.Trim());

                var rating = htmlNode.SelectNodes(xpathFindByClass("estrelasAvaliacao"));
                kabumProduct.rating = rating == null ? "" : rating.First().OuterHtml;

                kabumProduct.id = kabumProduct.link.Split("/").Where(x => int.TryParse(x, out _)).First();

                kabumProducts.Add(kabumProduct);

                //Salvo no banco de dados usando Entity
                using(var repo = new CrawlerContext())
                {
                    Product product = new Product();
                    product.KabumID = long.Parse(kabumProduct.id);
                    product.Name = kabumProduct.name;
                    product.Image = kabumProduct.image;
                    product.Link = kabumProduct.link;
                    product.Price = kabumProduct.isPromotional ? kabumProduct.oldPrice : kabumProduct.price;
                    product.PromotionalPrice = kabumProduct.isPromotional ? kabumProduct.price : "";

                    repo.Products.Add(product);
                    repo.SaveChanges();
                }
            }

            return kabumProducts;
        }
    }
}
