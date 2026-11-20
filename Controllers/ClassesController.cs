using ClosedXML.Excel;
using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using X.PagedList;
using X.PagedList.Extensions;

namespace EntityFrameworkCore.Controllers
{
    public class ClassesController : Controller
    {
        private readonly ILogger<ClassesController> _logger;
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context, ILogger<ClassesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(int? page)
        {
            try
            {
                int pageSize = 5;
                int pageNumber = page ?? 1;

                var classes = _context.Classes
                                      .OrderBy(x => x.Id)
                                      .ToPagedList(pageNumber, pageSize);

                return View(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Classes Index page.");
                TempData["Error"] = $"Error loading classes: {ex.Message}";
                return View(new List<Class>().ToPagedList(1, 5));
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rendering the Create Class view.");
                TempData["Error"] = "An error occurred while loading the creation page.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Class cls)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(cls);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Class created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new class.");
                ModelState.AddModelError("", $"Unable to save changes: {ex.Message}");
            }

            return View(cls);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var cls = await _context.Classes.FindAsync(id);

                if (cls == null)
                    return NotFound();

                return View(cls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching Class ID: {Id} for editing.", id);
                TempData["Error"] = $"Error loading record for edit: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Class cls)
        {
            if (id != cls.Id)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(cls);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Class updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving modifications to Class ID: {Id}.", id);
                ModelState.AddModelError("", $"Unable to save updates: {ex.Message}");
            }

            return View(cls);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var cls = await _context.Classes
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (cls == null)
                    return NotFound();

                return View(cls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading item details for Class ID: {Id}.", id);
                TempData["Error"] = $"Error loading details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var cls = await _context.Classes
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (cls == null)
                    return NotFound();

                return View(cls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while displaying delete screen for Class ID: {Id}.", id);
                TempData["Error"] = $"Error loading delete screen: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var cls = await _context.Classes.FindAsync(id);

                if (cls != null)
                {
                    _context.Classes.Remove(cls);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Class deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "The class record could not be found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while confirming deletion of Class ID: {Id}.", id);
                TempData["Error"] = $"Error deleting item: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            try
            {
                _logger.LogInformation("Class Excel Export started.");

                var classes = _context.Classes.ToList();

                _logger.LogInformation("Total Classes fetched for export: {Count}", classes.Count);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Classes");

                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Class Name";

                    int row = 2;

                    foreach (var item in classes)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.ClassName;

                        row++;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        _logger.LogInformation("Class Excel Export completed successfully.");

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Classes_{DateTime.Now:MMddHH}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting Classes to Excel.");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}