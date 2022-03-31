namespace ProductStockMgr.Models
{
    public class Product
    {
        public int id { get; set; }
        public string name { get; set; }
        public int qty { get; set; }
    }

    public class StockIn
    {
        public int id { get; set; }
        public int productId { get; set; }
        public int qty { get; set; }
    }

    public class StockOut
    {
        public int id { get; set; }
        public int productId { get; set; }
        public int qty { get; set; }
    }

    public class Root
    {
        public List<Product> products { get; set; }
        public List<StockIn> stockIns { get; set; }
        public List<StockOut> stockOuts { get; set; }
    }
    public class RootStockDetailsIn
    {
        public List<StockDetailsIn> StockDetailsIn { get; set; }
    }
    public class StockDetailsIn
    {
        public int id { get; set; }
        public int productId { get; set; }
        public string name { get; set; }
        public int qty { get; set; }
    }
    public class StockDetailsOut
    {
        public int id { get; set; }
        public int productId { get; set; }
        public string name { get; set; }
        public int qty { get; set; }
    }
    public static class JSONPath
    {
        public const string strJSON_path = "ProductMaster.json";
    }
}
