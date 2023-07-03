using KabumCrawler.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace KabumCrawler.Pages
{
    public class SearchModel : PageModel
    {
        public List<KabumProduct> products;

        public void OnGet(string query)
        {
            Crawler crawler = new Crawler(query);
            products = crawler.parseData();
        }
    }
}
