using ClosedXML.Excel;
using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList.Extensions;

namespace EntityFrameworkCore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(int? page)
        {
            try
            {
                _logger.LogInformation("Product Index page requested. Page Number: {Page}", page);

                int pageSize = 5;
                int pageNumber = page ?? 1;

                var products = _context.Products
                                       .OrderBy(p => p.ProductId)
                                       .ToPagedList(pageNumber, pageSize);

                _logger.LogInformation(
            "Product records loaded successfully. Total Records: {Count}, Current Page: {Page}",
            products.TotalItemCount,
            pageNumber);

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Product Index page.");

                TempData["Error"] = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("Product Create page requested.");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Product Create page.");

                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            try
            {
                _logger.LogInformation("Product Create initiated. Product Name: {ProductName}", product.ProductName);

                if (ModelState.IsValid)
                {
                    _context.Products.Add(product);

                    _logger.LogInformation("Product entity added to context. Product Name: {ProductName}",product.ProductName);

                    _context.SaveChanges();

                    TempData["Success"] = "Product Added Successfully";

                    return RedirectToAction("Index");
                }

                _logger.LogWarning("Product creation failed due to invalid model state. Product Name: {ProductName}", product.ProductName);

                TempData["Error"] = "Invalid Product Data";

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error occurred while creating Product. Product Name: {ProductName}",product.ProductName);

                TempData["Error"] = ex.Message;

                return View(product);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                _logger.LogInformation("Product Edit page requested. Product Id: {ProductId}",id);

                var product = _context.Products.Find(id);

                if (product == null)
                {
                    _logger.LogWarning("Product not found for Edit. Product Id: {ProductId}", id);

                    TempData["Error"] = "Product Not Found";
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex,"Error occurred while loading Product Edit page. Product Id: {ProductId}", id);

                TempData["Error"] = ex.Message;

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            try
            {
                _logger.LogInformation("Product Update initiated. Product Id: {ProductId}, Product Name: {ProductName}",product.ProductId,product.ProductName);

                if (ModelState.IsValid)
                {
                    _context.Products.Update(product);
                    _context.SaveChanges();

                    _logger.LogInformation("Product updated successfully. Product Id: {ProductId}, Product Name: {ProductName}",product.ProductId,product.ProductName);

                    TempData["Success"] = "Product Updated Successfully";

                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Invalid Product Data";

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error occurred while updating Product. Product Id: {ProductId}, Product Name: {ProductName}",product.ProductId,product.ProductName);

                TempData["Error"] = ex.Message;

                return View(product);
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                _logger.LogInformation("Product Delete page requested. Product Id: {ProductId}",id);

                var product = _context.Products.Find(id);

                if (product == null)
                {
                    TempData["Error"] = "Product Not Found";
                    return RedirectToAction("Index");
                }
                _logger.LogInformation("Product found for Delete. Product Id: {ProductId}, Product Name: {ProductName}",product.ProductId,product.ProductName);

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error occurred while loading Product Delete page. Product Id: {ProductId}",id);

                TempData["Error"] = ex.Message;

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Delete(Product product)
        {
            try
            {
                _logger.LogInformation("Product Delete initiated. Product Id: {ProductId}",product.ProductId);

                var data = _context.Products.Find(product.ProductId);

                if (data != null)
                {
                    _context.Products.Remove(data);
                    _context.SaveChanges();

                    _logger.LogInformation("Product deleted successfully. Product Id: {ProductId}, Product Name: {ProductName}",data.ProductId,data.ProductName);

                    TempData["Success"] = "Product Deleted Successfully";
                }
                else
                {
                    TempData["Error"] = "Product Not Found";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error occurred while deleting Product. Product Id: {ProductId}",product.ProductId);

                TempData["Error"] = ex.Message;

                return RedirectToAction("Index");
            }
        }

        public IActionResult Details(int id)
        {
            try
            {
                _logger.LogInformation("Product Details page requested. Product Id: {ProductId}",id);

                var product = _context.Products.Find(id);

                if (product == null)
                {
                    TempData["Error"] = "Product Not Found";
                    return RedirectToAction("Index");
                }

                _logger.LogInformation( "Product Details loaded successfully. Product Id: {ProductId}, Product Name: {ProductName}",product.ProductId,product.ProductName);

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error occurred while loading Product Details page. Product Id: {ProductId}",id);

                TempData["Error"] = ex.Message;

                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            try
            {
                _logger.LogInformation(
                    "Product Excel Export started.");

                var products = _context.Products.ToList();

                _logger.LogInformation(
                    "Total Products fetched for export: {Count}",
                    products.Count);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Products");

                    worksheet.Cell(1, 1).Value = "Product Id";
                    worksheet.Cell(1, 2).Value = "Product Name";
                    worksheet.Cell(1, 3).Value = "Category";
                    worksheet.Cell(1, 4).Value = "Quantity";
                    worksheet.Cell(1, 5).Value = "Price";

                    int row = 2;

                    foreach (var item in products)
                    {
                        worksheet.Cell(row, 1).Value = item.ProductId;
                        worksheet.Cell(row, 2).Value = item.ProductName;
                        worksheet.Cell(row, 3).Value = item.Category;
                        worksheet.Cell(row, 4).Value = item.Quantity;
                        worksheet.Cell(row, 5).Value = item.Price;

                        row++;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        var content = stream.ToArray();

                        _logger.LogInformation(
                            "Product Excel Export completed successfully.");

                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Products_{DateTime.Now:MMddHH}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while exporting Products to Excel.");

                TempData["Error"] = ex.Message;

                return RedirectToAction("Index");
            }
        }

        public IActionResult ProductPieChart()
        {
            try
            {
                _logger.LogInformation(
                    "Product Pie Chart requested.");

                var chartData = _context.Products
                    .Select(p => new
                    {
                        ProductName = p.ProductName,
                        Quantity = p.Quantity
                    })
                    .ToList();

                ViewBag.ChartData =
                    JsonConvert.SerializeObject(chartData);

                ViewBag.TotalProducts =
                    _context.Products.Count();

                ViewBag.TotalQuantity =
                    _context.Products.Sum(x => x.Quantity);

                ViewBag.MaxQuantity =
                    _context.Products.Max(x => x.Quantity);

                ViewBag.MinQuantity =
                    _context.Products.Min(x => x.Quantity);

                _logger.LogInformation(
                    "Product Pie Chart loaded successfully. Products Count: {Count}",
                    chartData.Count);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while loading Product Pie Chart.");

                TempData["Error"] =
                    "Unable to load Product Pie Chart";

                return RedirectToAction("Index");
            }
        }

        public IActionResult ProductStockValueChart()
        {
            try
            {
                _logger.LogInformation(
                    "Product Stock Value Chart requested.");

                var chartData = _context.Products
                    .Select(p => new
                    {
                        ProductName = p.ProductName,
                        StockValue = p.Quantity * p.Price
                    })
                    .OrderByDescending(x => x.StockValue)
                    .ToList();

                ViewBag.ChartData =
                    JsonConvert.SerializeObject(chartData);

                ViewBag.TotalProducts =
                    _context.Products.Count();

                ViewBag.TotalStockValue =
                    _context.Products.Sum(x => x.Quantity * x.Price);

                ViewBag.MaxStockValue =
                    _context.Products.Max(x => x.Quantity * x.Price);

                ViewBag.AverageStockValue =
                    Math.Round(
                        _context.Products.Average(x => x.Quantity * x.Price),
                        2);

                _logger.LogInformation(
                    "Product Stock Value Chart loaded successfully. Products Count: {Count}",
                    chartData.Count);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while loading Product Stock Value Chart.");

                TempData["Error"] =
                    "Unable to load Product Stock Value Chart";

                return RedirectToAction("Index");
            }
        }

        public IActionResult ProductCategoryChart()
        {
            try
            {
                _logger.LogInformation(
                    "Product Category Chart requested.");

                var chartData = _context.Products
                    .GroupBy(x => x.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        ProductCount = g.Count()
                    })
                    .OrderByDescending(x => x.ProductCount)
                    .ToList();

                ViewBag.ChartData =
                    JsonConvert.SerializeObject(chartData);

                ViewBag.TotalCategories =
                    chartData.Count();

                ViewBag.TotalProducts =
                    _context.Products.Count();

                ViewBag.MaxProductsInCategory =
                    chartData.Any()
                        ? chartData.Max(x => x.ProductCount)
                        : 0;

                ViewBag.AverageProductsPerCategory =
                    chartData.Any()
                        ? Math.Round(
                            chartData.Average(x => x.ProductCount),
                            2)
                        : 0;

                _logger.LogInformation(
                    "Product Category Chart loaded successfully. Categories Count: {Count}",
                    chartData.Count);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while loading Product Category Chart.");

                TempData["Error"] =
                    "Unable to load Product Category Chart";

                return RedirectToAction("Index");
            }
        }
    }
}