using Microsoft.AspNetCore.Mvc;
using ProductStockMgr.Models;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProductStockMgr.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Products()
        {
            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);
            ViewBag.data = product_details_full.products.Take(25).OrderByDescending(i => i.id).ToList();
            return View(ViewBag.data);
        }
        public IActionResult CreateProduct()
        {
            return View();
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(Product obj)
        {
            var now = DateTime.Now;
            var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
            int uniqueId = (int)(zeroDate.Ticks / 10000);

            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            if (product_details_full.products.Where(i => i.name.ToUpper().Contains(obj.name.ToUpper().Trim())).Count() > 0)
            {
                ViewBag.Duplicate = "Product name " + obj.name + " already exists !";
                return View();
            }

            product_details_full.products.Add(new Product()
            {
                id = uniqueId,
                name = obj.name,
                qty = obj.qty

            });
            product_details_full.stockIns.Add(new StockIn()
            {
                id = uniqueId,
                productId = uniqueId,
                qty = 0
            });
            product_details_full.stockOuts.Add(new StockOut()
            {
                id = uniqueId,
                productId = uniqueId,
                qty = 0
            });

            string jsonData = JsonConvert.SerializeObject(product_details_full);
            System.IO.File.WriteAllText(JSONPath.strJSON_path, jsonData);

            TempData["success"] = "Product created successfully";
            return RedirectToAction("Products");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult DeleteProduct(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            var obj = product_details_full.products.Single(i => i.id == id);

            var objStockIn = product_details_full.stockIns.Where(i => i.productId == id).ToList();
            var objStockOut = product_details_full.stockOuts.Where(i => i.productId == id).ToList();

            foreach (var Obj_remove in objStockIn)
                product_details_full.stockIns.Remove(Obj_remove);

            foreach (var Obj_remove in objStockOut)
                product_details_full.stockOuts.Remove(Obj_remove);

            product_details_full.products.Remove(obj);

            string jsonData = JsonConvert.SerializeObject(product_details_full);
            System.IO.File.WriteAllText(JSONPath.strJSON_path, jsonData);

            TempData["success"] = "Product removed successfully";

            return RedirectToAction("Products");
        }
        public IActionResult ProductStockIn()
        {
            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            List<StockDetailsIn> obj_StockDetailsIn = new List<StockDetailsIn>();

            foreach (var objProduct in product_details_full.products)
            {
                foreach (var objStockIns in product_details_full.stockIns.Where(i => i.productId == objProduct.id).ToList())
                {
                    obj_StockDetailsIn.Add(new StockDetailsIn()
                    {
                        id = objStockIns.id,
                        productId = objStockIns.productId,
                        name = objProduct.name,
                        qty = objStockIns.qty
                    });
                }
            }
            ViewBag.data = obj_StockDetailsIn.OrderByDescending(i => i.id).Take(10).ToList();
            return View(ViewBag.data);
        }
        public IActionResult AddStocksIn(int? productId)
        {
            if (productId == null || productId == 0)
            {
                return NotFound();
            }

            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            product_details_full.products.Single(i => i.id == productId).qty = 0;

            return View(product_details_full.products.Single(i => i.id == productId));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStocksInJSON(Product obj)
        {
            var now = DateTime.Now;
            var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
            int uniqueId = (int)(zeroDate.Ticks / 10000);

            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            product_details_full.stockIns.Add(new StockIn()
            {
                id = uniqueId,
                productId = obj.id,
                qty = obj.qty
            });

            foreach (var rec in product_details_full.stockIns.Where(i => i.productId == obj.id && i.qty == 0).ToList())
                product_details_full.stockIns.Remove(rec);

            product_details_full.products.Single(i => i.id == obj.id).qty = (product_details_full.products.Where(i => i.id == obj.id).Sum(j => j.qty) + obj.qty);

            string jsonData = JsonConvert.SerializeObject(product_details_full);
            System.IO.File.WriteAllText(JSONPath.strJSON_path, jsonData);

            TempData["success"] = "Stocks in added successfully";
            return RedirectToAction("ProductStockIn");
        }

        public IActionResult ProductStockOut()
        {
            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            List<StockDetailsOut> obj_StockDetailsOut = new List<StockDetailsOut>();

            foreach (var objProduct in product_details_full.products)
            {
                foreach (var objStockOut in product_details_full.stockOuts.Where(i => i.productId == objProduct.id).ToList())
                {
                    obj_StockDetailsOut.Add(new StockDetailsOut()
                    {
                        id = objStockOut.id,
                        productId = objStockOut.productId,
                        name = objProduct.name,
                        qty = objStockOut.qty
                    });
                }
            }
            ViewBag.data = obj_StockDetailsOut.OrderByDescending(i => i.id).Take(10).ToList();
            return View(ViewBag.data);
        }
        public IActionResult AddStocksOut(int? productId)
        {
            if (productId == null || productId == 0)
            {
                return NotFound();
            }

            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            product_details_full.products.Single(i => i.id == productId).qty = 0;

            return View(product_details_full.products.Single(i => i.id == productId));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStocksOutJSON(Product obj)
        {
            var now = DateTime.Now;
            var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
            int uniqueId = (int)(zeroDate.Ticks / 10000);

            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            product_details_full.stockOuts.Add(new StockOut()
            {
                id = uniqueId,
                productId = obj.id,
                qty = obj.qty
            });

            foreach (var rec in product_details_full.stockOuts.Where(i => i.productId == obj.id && i.qty == 0).ToList())
                product_details_full.stockOuts.Remove(rec);

            product_details_full.products.Single(i => i.id == obj.id).qty = (product_details_full.products.Where(i => i.id == obj.id).Sum(j => j.qty) - obj.qty);

            string jsonData = JsonConvert.SerializeObject(product_details_full);
            System.IO.File.WriteAllText(JSONPath.strJSON_path, jsonData);

            TempData["success"] = "Stocks out successfully";
            return RedirectToAction("ProductStockOut");
        }
        //[HttpPost]
        public IActionResult SearchProduct([FromBody] string productname)
        {
            if (productname == null) productname = "";
            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);
            var returnval = product_details_full.products.Where(i => i.name.ToUpper().Contains(productname.ToUpper())).ToList().OrderByDescending(j => j.id).Take(10);
            return Json(returnval);
        }
        public IActionResult SearchProductStockIn([FromBody] string productname)
        {
            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            List<StockDetailsIn> obj_StockDetailsIn = new List<StockDetailsIn>();

            foreach (var objProduct in product_details_full.products)
            {
                foreach (var objStockIns in product_details_full.stockIns.Where(i => i.productId == objProduct.id).ToList())
                {
                    obj_StockDetailsIn.Add(new StockDetailsIn()
                    {
                        id = objStockIns.id,
                        productId = objStockIns.productId,
                        name = objProduct.name,
                        qty = objStockIns.qty
                    });
                }
            }
            return Json(obj_StockDetailsIn.Where(i => i.name.ToUpper().Contains(productname.ToUpper())).ToList().OrderByDescending(i => i.id).Take(10).ToList());
        }
        public IActionResult SearchProductStockOut([FromBody] string productname)
        {
            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            List<StockDetailsOut> obj_StockDetailsOut = new List<StockDetailsOut>();

            foreach (var objProduct in product_details_full.products)
            {
                foreach (var objStockOut in product_details_full.stockOuts.Where(i => i.productId == objProduct.id).ToList())
                {
                    obj_StockDetailsOut.Add(new StockDetailsOut()
                    {
                        id = objStockOut.id,
                        productId = objStockOut.productId,
                        name = objProduct.name,
                        qty = objStockOut.qty
                    });
                }
            }
            return Json(obj_StockDetailsOut.Where(i => i.name.ToUpper().Contains(productname.ToUpper())).ToList().OrderByDescending(i => i.id).Take(10).ToList());
        }
        public IActionResult checkIfProductExists([FromBody] string productname)
        {
            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);
            var returnval = product_details_full.products.Where(i => i.name.ToUpper() == productname.ToUpper()).ToList().Take(1);
            return Json(returnval);
        }
        public IActionResult AddNewProduct([FromBody] string productname, [FromBody] Int32 quantity)
        {
            var now = DateTime.Now;
            var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
            int uniqueId = (int)(zeroDate.Ticks / 10000);

            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);
            string value = productname;
            productname = value.Split("~")[0];
            quantity = Convert.ToInt32(value.Split("~")[1]);

            product_details_full.products.Add(new Product()
            {
                id = uniqueId,
                name = productname,
                qty = Convert.ToInt32(quantity)

            });
            product_details_full.stockIns.Add(new StockIn()
            {
                id = uniqueId,
                productId = uniqueId,
                qty = 0
            });
            product_details_full.stockOuts.Add(new StockOut()
            {
                id = uniqueId,
                productId = uniqueId,
                qty = 0
            });

            string jsonData = JsonConvert.SerializeObject(product_details_full);
            System.IO.File.WriteAllText(JSONPath.strJSON_path, jsonData);

            var returnval = product_details_full.products.Single(i => i.name.ToUpper() == productname.ToUpper());
            return Json(returnval);
        }
        public IActionResult DeleteProd([FromBody] int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var webclient = new WebClient();
            var json = webclient.DownloadString(JSONPath.strJSON_path);
            var product_details_full = JsonConvert.DeserializeObject<Root>(json);

            var obj = product_details_full.products.Single(i => i.id == id);

            var objStockIn = product_details_full.stockIns.Where(i => i.productId == id).ToList();
            var objStockOut = product_details_full.stockOuts.Where(i => i.productId == id).ToList();

            foreach (var Obj_remove in objStockIn)
                product_details_full.stockIns.Remove(Obj_remove);

            foreach (var Obj_remove in objStockOut)
                product_details_full.stockOuts.Remove(Obj_remove);

            product_details_full.products.Remove(obj);

            string jsonData = JsonConvert.SerializeObject(product_details_full);
            System.IO.File.WriteAllText(JSONPath.strJSON_path, jsonData);

            return Json("");

        }
    }

}