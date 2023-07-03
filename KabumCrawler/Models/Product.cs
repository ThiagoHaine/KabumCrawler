namespace KabumCrawler.Models
{
    public class Product
    {
        public int ID { get; set; }
        public long KabumID { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string PromotionalPrice { get; set; }
        public string Link { get; set; }
        public string Price { get; set; }
    }
}
