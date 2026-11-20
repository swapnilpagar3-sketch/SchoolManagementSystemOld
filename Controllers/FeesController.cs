using ClosedXML.Excel;
using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace EntityFrameworkCore.Controllers
{
    public class FeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FeesController> _logger;

        public FeesController(ApplicationDbContext context, ILogger<FeesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(string studentName, string paymentMode, int? page)
        {
            try
            {
                _logger.LogInformation("Fees Index page requested. Page: {Page}", page);

                int pageSize = 10;
                int pageNumber = page ?? 1;

                var fees = _context.Fees
                    .Include(x => x.Student)
                    .AsQueryable();

                // Student Name Filter
                if (!string.IsNullOrWhiteSpace(studentName))
                {
                    fees = fees.Where(x =>
                        x.Student.StudentName.Contains(studentName));
                }

                // Payment Mode Filter
                if (!string.IsNullOrWhiteSpace(paymentMode))
                {
                    fees = fees.Where(x =>
                        x.PaymentMode == paymentMode);
                }

                // Preserve filter values
                ViewBag.StudentName = studentName;
                ViewBag.PaymentMode = paymentMode;

                var pagedFees = fees
                    .OrderBy(x => x.Id)
                    .ToPagedList(pageNumber, pageSize);

                _logger.LogInformation("Fees records loaded successfully. Total Records: {Count}", pagedFees.TotalItemCount);

                ViewData["IsLoading"] = false;

                return View(pagedFees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Fees page.");
                TempData["Error"] = ex.Message;
                return View();
            }
        }

        public IActionResult Create()
        {
            try
            {
                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Create Fees page.");
                TempData["Error"] = "Error loading student list details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Fee fee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Fees.Add(fee);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Fee collected successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving a new fee record.");
                ModelState.AddModelError("", $"Unable to save changes: {ex.Message}");
            }

            // Repopulate ViewBag if model state processing or database storage fails
            try
            {
                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName", fee.StudentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error repopulating student drop-down menu on collection validation failure.");
            }

            return View(fee);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var fee = await _context.Fees.FindAsync(id);

                if (fee == null)
                    return NotFound();

                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName", fee.StudentId);

                return View(fee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching fee modifications layout for Fee ID: {Id}", id);
                TempData["Error"] = $"Error loading collection data: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Fee fee)
        {
            if (id != fee.Id)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(fee);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Fee updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing ledger adjustments for Fee ID: {Id}", id);
                ModelState.AddModelError("", $"Unable to update record: {ex.Message}");
            }

            // Repopulate ViewBag if validation fails
            try
            {
                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName", fee.StudentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error repopulating student options layout on edit runtime mismatch.");
            }

            return View(fee);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var fee = await _context.Fees
                    .Include(x => x.Student)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (fee == null)
                    return NotFound();

                return View(fee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering confirmation layouts for discarding Fee ID: {Id}", id);
                TempData["Error"] = $"Error displaying balance removal criteria: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var fee = await _context.Fees.FindAsync(id);
                if (fee != null)
                {
                    _context.Fees.Remove(fee);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Fee deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Fee ledger entry not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard-deleting record item indices for Fee ID: {Id}", id);
                TempData["Error"] = $"Error resolving database drop pipeline: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ExportToExcel(string studentName, string paymentMode)
        {
            try
            {
                _logger.LogInformation("Fee Excel Export started.");

                var fees = _context.Fees
                    .Include(f => f.Student)
                    .AsQueryable();

                // Student Name Filter
                if (!string.IsNullOrWhiteSpace(studentName))
                {
                    fees = fees.Where(f =>
                        f.Student.StudentName.Contains(studentName));
                }

                // Payment Mode Filter
                if (!string.IsNullOrWhiteSpace(paymentMode))
                {
                    fees = fees.Where(f =>
                        f.PaymentMode == paymentMode);
                }

                var feeList = fees.ToList();

                _logger.LogInformation("Total Fee records fetched for export: {Count}", feeList.Count);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Fees");

                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Student Name";
                    worksheet.Cell(1, 3).Value = "Fee Date";
                    worksheet.Cell(1, 4).Value = "Amount";
                    worksheet.Cell(1, 5).Value = "Payment Mode";
                    worksheet.Cell(1, 6).Value = "Remarks";

                    int row = 2;

                    foreach (var item in feeList)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.Student?.StudentName ?? "";
                        worksheet.Cell(row, 3).Value = item.FeeDate.ToString("dd-MM-yyyy");
                        worksheet.Cell(row, 4).Value = item.Amount;
                        worksheet.Cell(row, 5).Value = item.PaymentMode;
                        worksheet.Cell(row, 6).Value = item.Remarks;

                        row++;
                    }

                    // Header Formatting
                    worksheet.Range("A1:F1").Style.Font.Bold = true;
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        _logger.LogInformation("Fee Excel Export completed successfully.");

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Fees_{DateTime.Now:MMddHH}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting Fees to Excel.");
                TempData["Error"] = "Error occurred while exporting Fee records.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult DailyFeesCollectionChart()
        {
            try
            {
                var data = _context.Fees
                    .GroupBy(x => x.FeeDate)
                    .Select(g => new
                    {
                        FeeDate = g.Key,
                        TotalAmount = g.Sum(x => x.Amount)
                    })
                    .OrderBy(x => x.FeeDate)
                    .ToList();

                ViewBag.Dates = data.Select(x => x.FeeDate.ToString("dd-MMM-yyyy")).ToList();
                ViewBag.Amounts = data.Select(x => x.TotalAmount).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing trends inside DailyFeesCollectionChart visualization tracking.");
                TempData["Error"] = $"Error loading financial processing metrics data: {ex.Message}";
                return View();
            }
        }

        public IActionResult PaymentModeDistribution()
        {
            try
            {
                var data = _context.Fees
                    .GroupBy(x => x.PaymentMode)
                    .Select(g => new
                    {
                        PaymentMode = g.Key,
                        Total = g.Count()
                    })
                    .OrderBy(x => x.PaymentMode)
                    .ToList();

                ViewBag.PaymentModes = data.Select(x => x.PaymentMode).ToList();
                ViewBag.TotalPayments = data.Select(x => x.Total).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering analytical breakdown calculations inside PaymentModeDistribution.");
                TempData["Error"] = $"Error formatting distribution index counts: {ex.Message}";
                return View();
            }
        }

        public IActionResult PaymentModeCollectionChart()
        {
            try
            {
                var data = _context.Fees
                    .GroupBy(x => x.PaymentMode)
                    .Select(g => new
                    {
                        PaymentMode = g.Key,
                        TotalAmount = g.Sum(x => x.Amount)
                    })
                    .OrderBy(x => x.PaymentMode)
                    .ToList();

                ViewBag.PaymentModes = data.Select(x => x.PaymentMode).ToList();
                ViewBag.TotalAmounts = data.Select(x => x.TotalAmount).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying aggregate values inside PaymentModeCollectionChart calculation routines.");
                TempData["Error"] = $"Error pulling channel balance tracking insights: {ex.Message}";
                return View();
            }
        }
    }
}