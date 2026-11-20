using ClosedXML.Excel;
using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using X.PagedList.Extensions;

namespace EntityFrameworkCore.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ILogger<AttendanceController> _logger;
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context, ILogger<AttendanceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string studentName, string attendanceDateRange, string status, int? page)
        {
            try
            {
                _logger.LogInformation("Attendance Index page requested. Page: {Page}", page);

                int pageSize = 10;
                int pageNumber = page ?? 1;

                var attendance = _context.Attendance
                    .Include(a => a.Student)
                    .AsQueryable();

                // Student Name Filter
                if (!string.IsNullOrWhiteSpace(studentName))
                {
                    attendance = attendance.Where(a =>
                        a.Student.StudentName.Contains(studentName));
                }

                // Attendance Date Range Filter
                if (!string.IsNullOrWhiteSpace(attendanceDateRange))
                {
                    string[] dates;

                    if (attendanceDateRange.Contains(" to "))
                    {
                        dates = attendanceDateRange.Split(" to ");
                    }
                    else
                    {
                        dates = attendanceDateRange.Split(" - ");
                    }

                    if (dates.Length == 2)
                    {
                        if (DateTime.TryParse(dates[0], out DateTime fromDate) &&
                            DateTime.TryParse(dates[1], out DateTime toDate))
                        {
                            attendance = attendance.Where(a =>
                                a.AttendanceDate >= fromDate.Date &&
                                a.AttendanceDate <= toDate.Date);
                        }
                    }
                }

                // Status Filter
                if (!string.IsNullOrWhiteSpace(status))
                {
                    attendance = attendance.Where(a =>
                        a.Status == status);
                }

                var result = attendance
                    .OrderBy(a => a.Id)
                    .ToPagedList(pageNumber, pageSize);

                ViewBag.StudentName = studentName;
                ViewBag.AttendanceDateRange = attendanceDateRange;
                ViewBag.Status = status;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Attendance Index page.");
                TempData["Error"] = $"Error loading attendance data: {ex.Message}";
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
                _logger.LogError(ex, "Error occurred while loading Create Attendance page.");
                TempData["Error"] = "Error loading student list.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Attendance attendance)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Attendance.Add(attendance);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Attendance marked successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating attendance record.");
                ModelState.AddModelError("", $"Unable to save changes: {ex.Message}");
            }

            // Repopulate ViewBag if something fails
            try
            {
                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName", attendance.StudentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error repopulating Student drop-down list on Create failure.");
            }

            return View(attendance);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var attendance = await _context.Attendance.FindAsync(id);

                if (attendance == null)
                    return NotFound();

                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName", attendance.StudentId);

                return View(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Edit page for Attendance ID: {Id}", id);
                TempData["Error"] = $"Error loading record: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Attendance attendance)
        {
            if (id != attendance.Id)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(attendance);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Attendance updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating Attendance ID: {Id}", id);
                ModelState.AddModelError("", $"Unable to save updates: {ex.Message}");
            }

            // Repopulate ViewBag if model state validation or update process fails
            try
            {
                ViewBag.StudentId = new SelectList(_context.Students, "Id", "StudentName", attendance.StudentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error repopulating Student drop-down list on Edit failure.");
            }

            return View(attendance);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var attendance = await _context.Attendance
                    .Include(a => a.Student)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (attendance == null)
                    return NotFound();

                return View(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Delete page for Attendance ID: {Id}", id);
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
                var attendance = await _context.Attendance.FindAsync(id);
                if (attendance != null)
                {
                    _context.Attendance.Remove(attendance);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Attendance deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Attendance record not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting Attendance ID: {Id}", id);
                TempData["Error"] = $"Error deleting attendance: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ExportToExcel(string studentName, string attendanceDateRange, string status)
        {
            try
            {
                _logger.LogInformation("Attendance Excel Export started.");

                var attendanceQuery = _context.Attendance
                    .Include(a => a.Student)
                    .AsQueryable();

                // Student Name Filter
                if (!string.IsNullOrWhiteSpace(studentName))
                {
                    attendanceQuery = attendanceQuery.Where(a =>
                        a.Student != null &&
                        a.Student.StudentName.Contains(studentName));
                }

                // Attendance Date Range Filter
                if (!string.IsNullOrWhiteSpace(attendanceDateRange))
                {
                    string[] dates;

                    if (attendanceDateRange.Contains(" to "))
                    {
                        dates = attendanceDateRange.Split(" to ");
                    }
                    else
                    {
                        dates = attendanceDateRange.Split(" - ");
                    }

                    if (dates.Length == 2)
                    {
                        if (DateTime.TryParse(dates[0], out DateTime fromDate) &&
                            DateTime.TryParse(dates[1], out DateTime toDate))
                        {
                            attendanceQuery = attendanceQuery.Where(a =>
                                a.AttendanceDate >= fromDate.Date &&
                                a.AttendanceDate <= toDate.Date);
                        }
                    }
                }

                // Status Filter
                if (!string.IsNullOrWhiteSpace(status))
                {
                    attendanceQuery = attendanceQuery.Where(a =>
                        a.Status == status);
                }

                var attendanceList = attendanceQuery
                    .OrderByDescending(a => a.Id)
                    .ToList();

                _logger.LogInformation("Total Attendance records fetched for export: {Count}", attendanceList.Count);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Attendance");

                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Student Name";
                    worksheet.Cell(1, 3).Value = "Attendance Date";
                    worksheet.Cell(1, 4).Value = "Status";

                    int row = 2;

                    foreach (var item in attendanceList)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.Student?.StudentName ?? "";
                        worksheet.Cell(row, 3).Value = item.AttendanceDate.ToString("dd-MM-yyyy");
                        worksheet.Cell(row, 4).Value = item.Status;

                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        _logger.LogInformation("Attendance Excel Export completed successfully.");

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Attendance_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting Attendance to Excel.");
                TempData["Error"] = "Error occurred while exporting Attendance data.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult PresentVsAbsent()
        {
            try
            {
                var attendanceData = _context.Attendance
                    .GroupBy(x => x.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Total = g.Count()
                    })
                    .ToList();

                ViewBag.ChartData = JsonConvert.SerializeObject(attendanceData);
                ViewBag.TotalRecords = attendanceData.Sum(x => x.Total);

                ViewBag.PresentCount = attendanceData
                    .Where(x => x.Status == "Present")
                    .Select(x => x.Total)
                    .FirstOrDefault();

                ViewBag.AbsentCount = attendanceData
                    .Where(x => x.Status == "Absent")
                    .Select(x => x.Total)
                    .FirstOrDefault();

                ViewBag.TotalCategories = attendanceData.Count;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading PresentVsAbsent metrics dashboard.");
                TempData["Error"] = $"Error generating charts dashboard: {ex.Message}";
                return View();
            }
        }

        public IActionResult DailyAttendanceTrend()
        {
            try
            {
                var dailyData = _context.Attendance
                    .Where(x => x.Status == "Present")
                    .AsEnumerable()
                    .GroupBy(x => x.AttendanceDate.Date)
                    .Select(g => new
                    {
                        AttendanceDate = g.Key.ToString("dd-MMM"),
                        TotalStudents = g.Count()
                    })
                    .OrderBy(x => DateTime.ParseExact(
                        x.AttendanceDate,
                        "dd-MMM",
                        System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();

                ViewBag.DateLabels = JsonConvert.SerializeObject(dailyData.Select(x => x.AttendanceDate));
                ViewBag.AttendanceCounts = JsonConvert.SerializeObject(dailyData.Select(x => x.TotalStudents));
                ViewBag.TotalAttendance = dailyData.Sum(x => x.TotalStudents);
                ViewBag.TotalDays = dailyData.Count;
                ViewBag.HighestAttendance = dailyData.Any() ? dailyData.Max(x => x.TotalStudents) : 0;
                ViewBag.AverageAttendance = dailyData.Any() ? Math.Round(dailyData.Average(x => x.TotalStudents), 2) : 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating DailyAttendanceTrend data dashboards.");
                TempData["Error"] = $"Error processing trends analysis data: {ex.Message}";
                return View();
            }
        }

        public IActionResult DailyPresentVsAbsent()
        {
            try
            {
                var attendanceData = _context.Attendance
                    .GroupBy(x => x.AttendanceDate)
                    .Select(g => new
                    {
                        AttendanceDate = g.Key,
                        PresentCount = g.Count(x => x.Status == "Present"),
                        AbsentCount = g.Count(x => x.Status == "Absent")
                    })
                    .OrderBy(x => x.AttendanceDate)
                    .ToList();

                ViewBag.DateLabels = JsonConvert.SerializeObject(
                    attendanceData.Select(x => x.AttendanceDate.ToString("dd-MMM")));

                ViewBag.PresentCounts = JsonConvert.SerializeObject(attendanceData.Select(x => x.PresentCount));
                ViewBag.AbsentCounts = JsonConvert.SerializeObject(attendanceData.Select(x => x.AbsentCount));
                ViewBag.TotalPresent = attendanceData.Sum(x => x.PresentCount);
                ViewBag.TotalAbsent = attendanceData.Sum(x => x.AbsentCount);
                ViewBag.TotalDays = attendanceData.Count;
                ViewBag.AveragePresent = attendanceData.Any() ? Math.Round(attendanceData.Average(x => x.PresentCount), 2) : 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while parsing DailyPresentVsAbsent historical data metrics charts.");
                TempData["Error"] = $"Error retrieving comparative analytics datasets: {ex.Message}";
                return View();
            }
        }

        public IActionResult TopStudentsByAttendance()
        {
            try
            {
                var data = _context.Attendance
                    .Where(x => x.Status == "Present")
                    .Join(
                        _context.Students,
                        a => a.StudentId,
                        s => s.Id,
                        (a, s) => new { s.StudentName })
                    .GroupBy(x => x.StudentName)
                    .Select(g => new
                    {
                        StudentName = g.Key,
                        PresentCount = g.Count()
                    })
                    .OrderByDescending(x => x.PresentCount)
                    .Take(10)
                    .ToList();

                ViewBag.StudentLabels = JsonConvert.SerializeObject(data.Select(x => x.StudentName));
                ViewBag.AttendanceCounts = JsonConvert.SerializeObject(data.Select(x => x.PresentCount));
                ViewBag.TotalStudents = data.Count;
                ViewBag.HighestAttendance = data.Any() ? data.Max(x => x.PresentCount) : 0;
                ViewBag.LowestAttendance = data.Any() ? data.Min(x => x.PresentCount) : 0;
                ViewBag.AverageAttendance = data.Any() ? Math.Round(data.Average(x => x.PresentCount), 2) : 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing queries for TopStudentsByAttendance listings.");
                TempData["Error"] = $"Error processing student ranking leaderboard: {ex.Message}";
                return View();
            }
        }
    }
}