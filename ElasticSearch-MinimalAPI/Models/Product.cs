namespace ElasticSearch_MinimalAPI.Models
{
    public class Product
    {
        public Product()
        {
            Id = Guid.NewGuid();    // Her newlendiginde yeni bir guild üretmesi için.
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }

    }
}
