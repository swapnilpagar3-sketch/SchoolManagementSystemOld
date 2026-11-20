using ClosedXML.Excel;
using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace EntityFrameworkCore.Controllers
{
    public class ExaminationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExaminationController> _logger;

        public ExaminationController(ApplicationDbContext context, ILogger<ExaminationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? page)
        {
            try
            {
                _logger.LogInformation("Exam Index page requested. Page: {Page}", page);

                int pageSize = 5;
                int pageNumber = page ?? 1;

                var exams = _context.Exams
                    .OrderBy(x => x.Id)
                    .ToPagedList(pageNumber, pageSize);

                ViewData["IsLoading"] = false;

                return View(exams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Exam list.");
                TempData["Error"] = ex.Message;
                return View();
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
                _logger.LogError(ex, "Error occurred while rendering Create Exam view.");
                TempData["Error"] = "An error occurred while loading the creation page.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam exam)
        {
            try
            {
                if (exam.PassingMarks > exam.TotalMarks)
                {
                    ModelState.AddModelError("PassingMarks", "Passing Marks cannot be greater than Total Marks.");
                }

                if (ModelState.IsValid)
                {
                    _context.Exams.Add(exam);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Exam created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating an exam record.");
                ModelState.AddModelError("", $"Unable to save changes: {ex.Message}");
            }

            return View(exam);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(id);
                if (exam == null)
                {
                    return NotFound();
                }

                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching Exam ID: {Id} for editing.", id);
                TempData["Error"] = $"Error loading record for edit: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Exam exam)
        {
            if (id != exam.Id)
            {
                return NotFound();
            }

            try
            {
                if (exam.PassingMarks > exam.TotalMarks)
                {
                    ModelState.AddModelError("PassingMarks", "Passing Marks cannot be greater than Total Marks.");
                }

                if (ModelState.IsValid)
                {
                    _context.Update(exam);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Exam updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving modifications to Exam ID: {Id}.", id);
                ModelState.AddModelError("", $"Unable to save updates: {ex.Message}");
            }

            return View(exam);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(id);
                if (exam == null)
                {
                    return NotFound();
                }

                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while displaying delete screen for Exam ID: {Id}.", id);
                TempData["Error"] = $"Error loading delete confirmation: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var exam = await _context.Exams.FindAsync(id);
                if (exam != null)
                {
                    _context.Exams.Remove(exam);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Exam deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "The exam record could not be found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while confirming deletion of Exam ID: {Id}.", id);
                TempData["Error"] = $"Error deleting item: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            try
            {
                _logger.LogInformation("Exam Excel Export started.");

                var exams = _context.Exams.ToList();

                _logger.LogInformation("Total Exams fetched for export: {Count}", exams.Count);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Exams");

                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Exam Name";
                    worksheet.Cell(1, 3).Value = "Exam Date";
                    worksheet.Cell(1, 4).Value = "Total Marks";
                    worksheet.Cell(1, 5).Value = "Passing Marks";

                    int row = 2;

                    foreach (var item in exams)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.ExamName;
                        worksheet.Cell(row, 3).Value = item.ExamDate.ToString("dd-MM-yyyy");
                        worksheet.Cell(row, 4).Value = item.TotalMarks;
                        worksheet.Cell(row, 5).Value = item.PassingMarks;

                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        _logger.LogInformation("Exam Excel Export completed successfully.");

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Exams_{DateTime.Now:MMddHH}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting Exams to Excel.");
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult ExamWiseTotalMarksChart()
        {
            try
            {
                var exams = _context.Exams
                    .OrderBy(x => x.ExamDate)
                    .ToList();

                ViewBag.ExamNames = exams.Select(x => x.ExamName).ToList();
                ViewBag.TotalMarks = exams.Select(x => x.TotalMarks).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating ExamWiseTotalMarksChart dataset.");
                TempData["Error"] = $"Error loading dashboard chart data: {ex.Message}";
                return View();
            }
        }

        public IActionResult TotalMarksVsPassingMarksChart()
        {
            try
            {
                var exams = _context.Exams
                    .OrderBy(x => x.ExamName)
                    .ToList();

                ViewBag.ExamNames = exams.Select(x => x.ExamName).ToList();
                ViewBag.TotalMarks = exams.Select(x => x.TotalMarks).ToList();
                ViewBag.PassingMarks = exams.Select(x => x.PassingMarks).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating TotalMarksVsPassingMarksChart dataset.");
                TempData["Error"] = $"Error loading validation chart datasets: {ex.Message}";
                return View();
            }
        }

        public IActionResult ExamsConductedMonthWiseChart()
        {
            try
            {
                var data = _context.Exams
                    .GroupBy(x => x.ExamDate.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        TotalExams = g.Count()
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                string[] monthNames =
                {
                    "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
                };

                ViewBag.Months = data.Select(x => monthNames[x.Month]).ToList();
                ViewBag.TotalExams = data.Select(x => x.TotalExams).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while computing monthly trends inside ExamsConductedMonthWiseChart.");
                TempData["Error"] = $"Error generating historical monthly analysis: {ex.Message}";
                return View();
            }
        }
    }
}