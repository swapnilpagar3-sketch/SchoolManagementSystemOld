using ClosedXML.Excel;
using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using X.PagedList;
using X.PagedList.Extensions;

namespace EntityFrameworkCore.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubjectsController> _logger;

        public SubjectsController(ApplicationDbContext context, ILogger<SubjectsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(int? page)
        {
            try
            {
                _logger.LogInformation("Loading subjects index page. Page number: {PageNumber}", page);

                int pageSize = 5;
                int pageNumber = page ?? 1;

                var subjects = _context.Subjects
                                       .OrderBy(x => x.Id)
                                       .ToPagedList(pageNumber, pageSize);

                return View(subjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the subjects index list.");
                TempData["Error"] = "Unable to load subject records at this time.";
                return View(new List<Subject>().ToPagedList(1, 5));
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
                _logger.LogError(ex, "An error occurred while rendering the Subject Create view.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subject subject)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Subjects.Add(subject);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Subject created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to create subject. Please verify form details.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving a new subject.");
                ModelState.AddModelError(string.Empty, "An unexpected system issue occurred while saving. Please try again.");
                TempData["Error"] = "An internal error occurred while trying to create the subject.";
            }

            return View(subject);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var subject = await _context.Subjects.FindAsync(id);

                if (subject == null)
                    return NotFound();

                return View(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Subject ID: {Id} for editing.", id);
                TempData["Error"] = "Could not retrieve subject details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subject subject)
        {
            if (id != subject.Id)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(subject);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Subject updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to update subject. Validation errors found.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating Subject ID: {Id}.", id);
                ModelState.AddModelError(string.Empty, "Unable to save adjustments to the database entry.");
                TempData["Error"] = "An internal error occurred while performing the update operation.";
            }

            return View(subject);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var subject = await _context.Subjects
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (subject == null)
                    return NotFound();

                return View(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving data for Subject ID: {Id}.", id);
                TempData["Error"] = "Unable to fetch subject details.";
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
                var subject = await _context.Subjects
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (subject == null)
                    return NotFound();

                return View(subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the deletion layout for Subject ID: {Id}.", id);
                TempData["Error"] = "Could not process your request to review this record for removal.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(id);

                if (subject != null)
                {
                    _context.Subjects.Remove(subject);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Subject deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Subject record could not be found to delete.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while processing deletion routine execution for Subject ID: {Id}.", id);
                TempData["Error"] = "A database disruption prevented record removal.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            try
            {
                _logger.LogInformation("Subject Excel Export started.");

                var subjects = _context.Subjects.ToList();

                _logger.LogInformation("Total Subjects fetched for export: {Count}", subjects.Count);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Subjects");

                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Subject Name";

                    int row = 2;

                    foreach (var item in subjects)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.SubjectName;

                        row++;
                    }

                    // Auto Fit Columns for clean formatting
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        _logger.LogInformation("Subject Excel Export completed successfully.");

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Subjects_{DateTime.Now:MMddHH}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting Subjects to Excel.");
                TempData["Error"] = "Error occurred while exporting Subject data.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}