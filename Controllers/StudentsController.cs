using ClosedXML.Excel;
using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.DependencyResolver;
using System.IO;
using X.PagedList;
using X.PagedList.Extensions;

namespace EntityFrameworkCore.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ILogger<StudentsController> _logger;
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context, ILogger<StudentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(int? page)
        {
            try
            {
                _logger.LogInformation("Loading students index page. Page number: {PageNumber}", page);

                int pageSize = 10;
                int pageNumber = page ?? 1;

                var students = _context.Students
                                       .OrderBy(x => x.Id)
                                       .ToPagedList(pageNumber, pageSize);

                return View(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rendering the students index list.");
                TempData["Error"] = "Unable to load student data records at this moment.";
                return View(new List<Student>().ToPagedList(1, 10));
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
                _logger.LogError(ex, "An error occurred while loading the Student Create view.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(student);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Student created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to create student. Please verify form details.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while committing a new student record to the database.");
                ModelState.AddModelError(string.Empty, "An unexpected system issue occurred while saving. Please try again.");
                TempData["Error"] = "An internal error occurred while trying to save the student.";
            }

            return View(student);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var student = await _context.Students.FindAsync(id);

                if (student == null)
                    return NotFound();

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching student data for editing. Student ID: {Id}", id);
                TempData["Error"] = "Could not retrieve student details for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Student updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to update student. Validation errors found.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating data for Student ID: {Id}", id);
                ModelState.AddModelError(string.Empty, "Unable to save adjustments to the database entry.");
                TempData["Error"] = "An internal error occurred while performing the update operation.";
            }

            return View(student);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (student == null)
                    return NotFound();

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the deletion review layout for Student ID: {Id}", id);
                TempData["Error"] = "Could not parse individual evaluation targets layout.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);

                if (student != null)
                {
                    _context.Students.Remove(student);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Student deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Student record could not be found to delete.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while processing deletion routine execution workflows for student ID: {Id}", id);
                TempData["Error"] = "Database processing disruption prevented record removal.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (student == null)
                    return NotFound();

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving detailed metrics fields for student identity confirmation: {Id}", id);
                TempData["Error"] = "Unable to fetch demographic metrics sheets for this profile segment.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            try
            {
                _logger.LogInformation("Student Excel Export started.");

                var students = _context.Students.ToList();

                _logger.LogInformation("Total Students fetched for export: {Count}", students.Count);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Students");

                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Student Name";
                    worksheet.Cell(1, 3).Value = "Gender";
                    worksheet.Cell(1, 4).Value = "Date Of Birth";
                    worksheet.Cell(1, 5).Value = "Mobile No";
                    worksheet.Cell(1, 6).Value = "Address";
                    worksheet.Cell(1, 7).Value = "Admission Date";
                    worksheet.Cell(1, 8).Value = "Class Id";

                    int row = 2;

                    foreach (var item in students)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.StudentName;
                        worksheet.Cell(row, 3).Value = item.Gender;
                        worksheet.Cell(row, 4).Value = item.DateOfBirth;
                        worksheet.Cell(row, 5).Value = item.MobileNo;
                        worksheet.Cell(row, 6).Value = item.Address;
                        worksheet.Cell(row, 7).Value = item.AdmissionDate;
                        worksheet.Cell(row, 8).Value = item.ClassId;

                        row++;
                    }

                    // Format Date Columns
                    worksheet.Column(4).Style.DateFormat.Format = "dd-MMM-yyyy";
                    worksheet.Column(7).Style.DateFormat.Format = "dd-MMM-yyyy";

                    // Auto Fit Columns
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        _logger.LogInformation("Student Excel Export completed successfully.");

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Students_{DateTime.Now:MMddHH}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting Students to Excel.");
                TempData["Error"] = "Error occurred while exporting Students to Excel.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult StudentsByGender()
        {
            try
            {
                var genderData = _context.Students
                    .GroupBy(x => x.Gender)
                    .Select(g => new
                    {
                        Gender = g.Key,
                        TotalStudents = g.Count()
                    })
                    .ToList();

                ViewBag.ChartData = JsonConvert.SerializeObject(genderData);
                ViewBag.TotalStudents = genderData.Sum(x => x.TotalStudents);
                ViewBag.TotalCategories = genderData.Count;
                ViewBag.HighestCount = genderData.Any() ? genderData.Max(x => x.TotalStudents) : 0;
                ViewBag.LowestCount = genderData.Any() ? genderData.Min(x => x.TotalStudents) : 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while compiling dynamic gender chart distribution sets metrics summaries.");
                TempData["Error"] = "An error occurred while preparing descriptive metrics reports assets visualizations charts.";
                return View();
            }
        }

        public IActionResult MonthlyAdmissionsTrend()
        {
            try
            {
                var monthlyData = _context.Students
                    .AsEnumerable()
                    .GroupBy(x => new
                    {
                        x.AdmissionDate.Year,
                        x.AdmissionDate.Month
                    })
                    .Select(g => new
                    {
                        MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM-yyyy"),
                        TotalStudents = g.Count()
                    })
                    .OrderBy(x => DateTime.ParseExact(
                        x.MonthYear,
                        "MMM-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();

                ViewBag.MonthLabels = JsonConvert.SerializeObject(monthlyData.Select(x => x.MonthYear));
                ViewBag.AdmissionCounts = JsonConvert.SerializeObject(monthlyData.Select(x => x.TotalStudents));
                ViewBag.TotalAdmissions = monthlyData.Sum(x => x.TotalStudents);
                ViewBag.TotalMonths = monthlyData.Count;
                ViewBag.HighestAdmission = monthlyData.Any() ? monthlyData.Max(x => x.TotalStudents) : 0;
                ViewBag.AverageAdmission = monthlyData.Any() ? Math.Round(monthlyData.Average(x => x.TotalStudents), 2) : 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while extracting historical timeline intervals arrays on line charts pipelines calculations entries.");
                TempData["Error"] = "Unable to compute structured admission metrics matrix vectors charts layout assets.";
                return View();
            }
        }
    }
}