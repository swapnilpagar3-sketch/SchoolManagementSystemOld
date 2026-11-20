using ClosedXML.Excel;
using EntityFrameworkCore.Models;
using EntityFrameworkCore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace EntityFrameworkCore.Controllers
{
    public class MarksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MarksController> _logger;

        public MarksController(ApplicationDbContext context, ILogger<MarksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(string studentName, int? examId, string result, int? page)
        {
            try
            {
                _logger.LogInformation("Marks Index page requested. Page: {Page}", page);

                int pageSize = 10;
                int pageNumber = page ?? 1;

                var marks = _context.StudentExamMarks
                    .Include(x => x.Student)
                    .Include(x => x.Exam)
                    .AsQueryable();

                // Student Filter
                if (!string.IsNullOrWhiteSpace(studentName))
                {
                    marks = marks.Where(x =>
                        x.Student.StudentName.Contains(studentName));
                }

                // Exam Filter
                if (examId.HasValue)
                {
                    marks = marks.Where(x =>
                        x.ExamId == examId.Value);
                }

                // Result Filter
                if (!string.IsNullOrWhiteSpace(result))
                {
                    if (result == "Pass")
                    {
                        marks = marks.Where(x =>
                            x.Exam != null &&
                            x.ObtainedMarks >= x.Exam.PassingMarks);
                    }
                    else if (result == "Fail")
                    {
                        marks = marks.Where(x =>
                            x.Exam != null &&
                            x.ObtainedMarks < x.Exam.PassingMarks);
                    }
                }

                ViewBag.StudentName = studentName;
                ViewBag.ExamId = examId;
                ViewBag.Result = result;

                ViewBag.Exams = new SelectList(
                    _context.Exams.OrderBy(x => x.ExamName),
                    "Id",
                    "ExamName");

                var pagedData = marks
                    .OrderBy(x => x.Id)
                    .ToPagedList(pageNumber, pageSize);

                _logger.LogInformation("Student marks processed successfully. Count: {Count}", pagedData.TotalItemCount);

                return View(pagedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Marks index catalog records.");
                TempData["Error"] = "Unable to process student marks ledger records layout visual rendering.";
                return View();
            }
        }

        public IActionResult Create()
        {
            try
            {
                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName");
                ViewBag.ExamId = new SelectList(_context.Exams, "Id", "ExamName");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading context relational tables data for score configuration layout.");
                TempData["Error"] = "Error building dependent entry reference selectors.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentExamMarks marks)
        {
            try
            {
                var exam = await _context.Exams
                    .FirstOrDefaultAsync(x => x.Id == marks.ExamId);

                if (exam != null && marks.ObtainedMarks > exam.TotalMarks)
                {
                    ModelState.AddModelError(
                        "ObtainedMarks",
                        "Obtained Marks cannot be greater than Total Marks.");
                }

                if (ModelState.IsValid)
                {
                    _context.StudentExamMarks.Add(marks);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Exam Result added successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database save failure while creating evaluation item ledger index mapping rules entries.");
                ModelState.AddModelError("", $"Failed to log transaction: {ex.Message}");
            }

            // Repopulate ViewBags on validation or save exception
            try
            {
                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName", marks.StudentId);
                ViewBag.ExamId = new SelectList(_context.Exams, "Id", "ExamName", marks.ExamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recovery fallback rendering error on data selector validation exception workflows.");
            }

            return View(marks);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var marks = await _context.StudentExamMarks.FindAsync(id);

                if (marks == null)
                    return NotFound();

                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName", marks.StudentId);
                ViewBag.ExamId = new SelectList(_context.Exams, "Id", "ExamName", marks.ExamId);

                return View(marks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing edit staging for record item verification layout rules: {Id}", id);
                TempData["Error"] = "Unable to fetch requested academic evaluation entry records.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentExamMarks marks)
        {
            if (id != marks.Id)
                return NotFound();

            try
            {
                var exam = await _context.Exams
                    .FirstOrDefaultAsync(x => x.Id == marks.ExamId);

                if (exam != null && marks.ObtainedMarks > exam.TotalMarks)
                {
                    ModelState.AddModelError(
                        "ObtainedMarks",
                        "Obtained Marks cannot be greater than Total Marks.");
                }

                if (ModelState.IsValid)
                {
                    _context.Update(marks);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Exam Result updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on modifying transaction ledger boundaries for score card entry items: {Id}", id);
                ModelState.AddModelError("", $"Database updates structural violation: {ex.Message}");
            }

            // Repopulate ViewBags if operations fall through
            try
            {
                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName", marks.StudentId);
                ViewBag.ExamId = new SelectList(_context.Exams, "Id", "ExamName", marks.ExamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding dependency dropdown list data on Edit runtime exception recovery.");
            }

            return View(marks);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var marks = await _context.StudentExamMarks
                    .Include(x => x.Student)
                    .Include(x => x.Exam)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (marks == null)
                    return NotFound();

                return View(marks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering row removal diagnostic guidelines for assessment item: {Id}", id);
                TempData["Error"] = "Error launching data deletion inspection confirmation page.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var marks = await _context.StudentExamMarks.FindAsync(id);
                if (marks != null)
                {
                    _context.StudentExamMarks.Remove(marks);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Exam Result deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Target data record trace index could not be located.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal application error occurred executing structural array entity delete for transaction ID: {Id}", id);
                TempData["Error"] = $"Process interrupted: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ExportToExcel(string studentName, int? examId, string result)
        {
            try
            {
                _logger.LogInformation("Student Marks Excel Export started.");

                var marksQuery = _context.StudentExamMarks
                    .Include(x => x.Student)
                    .Include(x => x.Exam)
                    .AsQueryable();

                // Student Name Filter
                if (!string.IsNullOrWhiteSpace(studentName))
                {
                    marksQuery = marksQuery.Where(x =>
                        x.Student.StudentName.Contains(studentName));
                }

                // Exam Filter
                if (examId.HasValue)
                {
                    marksQuery = marksQuery.Where(x =>
                        x.ExamId == examId.Value);
                }

                // Result Filter
                if (!string.IsNullOrWhiteSpace(result))
                {
                    if (result == "Pass")
                    {
                        marksQuery = marksQuery.Where(x =>
                            x.Exam != null &&
                            x.ObtainedMarks >= x.Exam.PassingMarks);
                    }
                    else if (result == "Fail")
                    {
                        marksQuery = marksQuery.Where(x =>
                            x.Exam != null &&
                            x.ObtainedMarks < x.Exam.PassingMarks);
                    }
                }

                var marksList = marksQuery.ToList();

                _logger.LogInformation("Total Student Marks records fetched for export: {Count}", marksList.Count);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Student Results");

                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Student Name";
                    worksheet.Cell(1, 3).Value = "Exam Name";
                    worksheet.Cell(1, 4).Value = "Exam Date";
                    worksheet.Cell(1, 5).Value = "Total Marks";
                    worksheet.Cell(1, 6).Value = "Passing Marks";
                    worksheet.Cell(1, 7).Value = "Obtained Marks";
                    worksheet.Cell(1, 8).Value = "Result";

                    int row = 2;

                    foreach (var item in marksList)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.Student?.StudentName ?? "";
                        worksheet.Cell(row, 3).Value = item.Exam?.ExamName ?? "";
                        worksheet.Cell(row, 4).Value = item.Exam?.ExamDate.ToString("dd-MM-yyyy") ?? "";
                        worksheet.Cell(row, 5).Value = item.Exam?.TotalMarks ?? 0;
                        worksheet.Cell(row, 6).Value = item.Exam?.PassingMarks ?? 0;
                        worksheet.Cell(row, 7).Value = item.ObtainedMarks;
                        worksheet.Cell(row, 8).Value = item.Exam != null && item.ObtainedMarks >= item.Exam.PassingMarks ? "Pass" : "Fail";

                        row++;
                    }

                    worksheet.Range("A1:H1").Style.Font.Bold = true;
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        _logger.LogInformation("Student Marks Excel Export completed successfully.");

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"StudentResults_{DateTime.Now:MMddHH}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting Student Marks to Excel sheets layout engines.");
                TempData["Error"] = "Error occurred while exporting Student Marks compilation rules documents.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult ExamWisePassFailChart()
        {
            try
            {
                var data = _context.StudentExamMarks
                    .Include(x => x.Exam)
                    .GroupBy(x => new
                    {
                        x.ExamId,
                        x.Exam.ExamName,
                        x.Exam.PassingMarks
                    })
                    .Select(g => new
                    {
                        ExamName = g.Key.ExamName,
                        PassCount = g.Count(x => x.ObtainedMarks >= g.Key.PassingMarks),
                        FailCount = g.Count(x => x.ObtainedMarks < g.Key.PassingMarks)
                    })
                    .OrderBy(x => x.ExamName)
                    .ToList();

                ViewBag.ExamNames = data.Select(x => x.ExamName).ToList();
                ViewBag.PassCount = data.Select(x => x.PassCount).ToList();
                ViewBag.FailCount = data.Select(x => x.FailCount).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quantitative indicators compilation logic on ExamWisePassFailChart data pipeline lookup.");
                TempData["Error"] = $"Error loading graphical visualization assets data metrics: {ex.Message}";
                return View();
            }
        }

        public IActionResult PassPercentagePerExamChart()
        {
            try
            {
                var data = _context.StudentExamMarks
                    .Include(x => x.Exam)
                    .GroupBy(x => new
                    {
                        x.ExamId,
                        x.Exam.ExamName,
                        x.Exam.PassingMarks
                    })
                    .Select(g => new
                    {
                        ExamName = g.Key.ExamName,
                        TotalStudents = g.Count(),
                        PassStudents = g.Count(x => x.ObtainedMarks >= g.Key.PassingMarks)
                    })
                    .ToList();

                ViewBag.ExamNames = data.Select(x => x.ExamName).ToList();
                ViewBag.PassPercentage = data.Select(x =>
                    x.TotalStudents == 0 ? 0 : Math.Round(((double)x.PassStudents / x.TotalStudents) * 100, 2)).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analytical distribution calculations broken during dynamic percentage resolution algorithms.");
                TempData["Error"] = $"Error parsing matrix ratios charts context fields: {ex.Message}";
                return View();
            }
        }

        public IActionResult AverageMarksVsTotalMarksChart()
        {
            try
            {
                var data = _context.StudentExamMarks
                    .Include(x => x.Exam)
                    .GroupBy(x => new
                    {
                        x.ExamId,
                        x.Exam.ExamName,
                        x.Exam.TotalMarks
                    })
                    .Select(g => new
                    {
                        ExamName = g.Key.ExamName,
                        TotalMarks = g.Key.TotalMarks,
                        AverageMarks = g.Average(x => x.ObtainedMarks)
                    })
                    .OrderBy(x => x.ExamName)
                    .ToList();

                ViewBag.ExamNames = data.Select(x => x.ExamName).ToList();
                ViewBag.TotalMarks = data.Select(x => x.TotalMarks).ToList();
                ViewBag.AverageMarks = data.Select(x => Math.Round(x.AverageMarks, 2)).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Structural query evaluation failure within mean comparison algorithms routines updates pipeline.");
                TempData["Error"] = $"Chart generation metadata parse exception issues detected: {ex.Message}";
                return View();
            }
        }

        public IActionResult ExamWiseTopper()
        {
            try
            {
                var topperList = _context.StudentExamMarks
                    .Include(x => x.Student)
                    .Include(x => x.Exam)
                    .AsEnumerable()
                    .GroupBy(x => x.ExamId)
                    .Select(g =>
                    {
                        var topper = g
                            .OrderByDescending(x => x.ObtainedMarks)
                            .First();

                        return new ExamTopperVM
                        {
                            ExamName = topper.Exam!.ExamName,
                            StudentName = topper.Student!.StudentName,
                            ObtainedMarks = topper.ObtainedMarks,
                            TotalMarks = topper.Exam.TotalMarks,
                            Percentage = topper.Exam.TotalMarks == 0 ? 0 : Math.Round((double)topper.ObtainedMarks * 100 / topper.Exam.TotalMarks, 2)
                        };
                    })
                    .OrderBy(x => x.ExamName)
                    .ToList();

                return View(topperList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "In-memory processing collections failure computing student honors ranking profiles arrays mapping index rules.");
                TempData["Error"] = $"High scores ranking index metrics mapping processing trace failed: {ex.Message}";
                return View(new List<ExamTopperVM>());
            }
        }
    }
}