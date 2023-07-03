using KabumCrawler.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KabumCrawler.Pages
{
    public class ReviewsModel : PageModel
    {
        public List<KabumReview> reviews;
        public string productId;
        public void OnGet(string productId)
        {
            Crawler crawler= new Crawler();
            reviews = crawler.crawlReviews(productId);
            this.productId = productId;
        }
    }
}
