using ClosedXML.Excel;
using EntityFrameworkCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using X.PagedList;
using X.PagedList.Extensions;

namespace EntityFrameworkCore.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ILogger<TeacherController> _logger;
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context, ILogger<TeacherController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page)
        {
            try
            {
                _logger.LogInformation("Teacher Index page requested. Page: {Page}", page);

                await Task.Delay(100); // Retained from your original codebase
                int pageSize = 10;
                int pageNumber = page ?? 1;

                var teachers = _context.Teachers
                    .OrderBy(x => x.Id)
                    .ToPagedList(pageNumber, pageSize);

                _logger.LogInformation("Teacher records loaded successfully. Total Records: {Count}", teachers.TotalItemCount);

                ViewData["IsLoading"] = false;
                return View(teachers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Teacher Index page");
                TempData["Error"] = "An error occurred while loading the teacher list.";

                // Return an empty paged list to avoid null reference exceptions in the View
                return View(new List<Teacher>().ToPagedList(1, 10));
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("Teacher Create page opened");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while opening Teacher Create page");
                TempData["Error"] = "An error occurred while loading the creation page.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Teacher teacher)
        {
            try
            {
                _logger.LogInformation("Teacher Create initiated for Name: {Name}", teacher?.Name);

                if (ModelState.IsValid)
                {
                    if (teacher.ImageFile != null)
                    {
                        string fileName = teacher.ImageFile.FileName;
                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/TeacherImages");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            teacher.ImageFile.CopyTo(stream);
                        }

                        teacher.ImageUrl = "/TeacherImages/" + fileName;
                        _logger.LogInformation("Teacher image uploaded successfully. Image: {ImageUrl}", teacher.ImageUrl);
                    }

                    _context.Teachers.Add(teacher);
                    _context.SaveChanges();

                    _logger.LogInformation("Teacher created successfully. Id: {Id}, Name: {Name}", teacher.Id, teacher.Name);
                    TempData["Success"] = "Teacher Created Successfully";

                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("Teacher creation failed due to invalid model state");
                TempData["Error"] = "Please correct the validation errors and try again.";
                return View(teacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating Teacher. Name: {Name}", teacher?.Name);
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                TempData["Error"] = "Internal system error saving the teacher record.";
                return View(teacher);
            }
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            try
            {
                _logger.LogInformation("Teacher Update page requested for Id: {Id}", id);

                var teacher = _context.Teachers.FirstOrDefault(a => a.Id == id);

                if (teacher == null)
                {
                    _logger.LogWarning("Teacher not found for Update. Id: {Id}", id);
                    TempData["Error"] = "Teacher not found";
                    return RedirectToAction(nameof(Index));
                }

                return View(teacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Update page for Teacher Id: {Id}", id);
                TempData["Error"] = "An error occurred while trying to load the teacher record.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ActionName("Update")]
        [ValidateAntiForgeryToken]
        public IActionResult Update_Post(Teacher teacher)
        {
            try
            {
                _logger.LogInformation("Teacher Update initiated. Id: {Id}, Name: {Name}", teacher?.Id, teacher?.Name);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Teacher Update failed due to invalid model state. Id: {Id}", teacher?.Id);
                    TempData["Error"] = "Invalid Teacher Data";
                    return View("Update", teacher);
                }

                if (teacher.ImageFile != null)
                {
                    string fileName = teacher.ImageFile.FileName;
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/TeacherImages", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        teacher.ImageFile.CopyTo(stream);
                    }

                    teacher.ImageUrl = "/TeacherImages/" + fileName;
                    _logger.LogInformation("Teacher image uploaded successfully. Id: {Id}, ImageUrl: {ImageUrl}", teacher.Id, teacher.ImageUrl);
                }
                else
                {
                    // Tracked entity state isolation fix using AsNoTracking
                    var existingTeacher = _context.Teachers
                                                  .AsNoTracking()
                                                  .FirstOrDefault(x => x.Id == teacher.Id);

                    if (existingTeacher != null)
                    {
                        teacher.ImageUrl = existingTeacher.ImageUrl;
                    }
                }

                _context.Teachers.Update(teacher);
                _context.SaveChanges();

                _logger.LogInformation("Teacher updated successfully. Id: {Id}, Name: {Name}", teacher.Id, teacher.Name);
                TempData["Success"] = "Teacher Updated Successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating Teacher. Id: {Id}, Name: {Name}", teacher?.Id, teacher?.Name);
                ModelState.AddModelError(string.Empty, "Database error during data modification routine.");
                TempData["Error"] = "An error occurred while saving modifications.";
                return View("Update", teacher);
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                _logger.LogInformation("Teacher Delete page requested. Id: {Id}", id);

                var teacher = _context.Teachers.Find(id);

                if (teacher == null)
                {
                    _logger.LogWarning("Teacher not found for Delete. Id: {Id}", id);
                    TempData["Error"] = "Teacher not found";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Teacher found for Delete. Id: {Id}, Name: {Name}", teacher.Id, teacher.Name);
                return View(teacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while opening Delete page for Teacher. Id: {Id}", id);
                TempData["Error"] = "An error occurred while looking for deletion records.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Teacher teacher)
        {
            try
            {
                _logger.LogInformation("Teacher Delete initiated. Id: {Id}", teacher?.Id);

                var data = _context.Teachers.Find(teacher.Id);

                if (data != null)
                {
                    _context.Teachers.Remove(data);
                    _context.SaveChanges();

                    _logger.LogInformation("Teacher deleted successfully. Id: {Id}, Name: {Name}", data.Id, data.Name);
                    TempData["Success"] = "Teacher Deleted Successfully";
                }
                else
                {
                    _logger.LogWarning("Teacher not found during delete operation. Id: {Id}", teacher?.Id);
                    TempData["Error"] = "Teacher Not Found";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting Teacher. Id: {Id}", teacher?.Id);
                TempData["Error"] = "Database processing error occurred during deletion workflow.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                _logger.LogInformation("Teacher Details requested. Id: {Id}", id);

                var teacher = _context.Teachers.Find(id);

                if (teacher == null)
                {
                    _logger.LogWarning("Teacher not found for Details. Id: {Id}", id);
                    TempData["Error"] = "Teacher not found";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Teacher Details loaded successfully. Id: {Id}, Name: {Name}", teacher.Id, teacher.Name);
                return View(teacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Teacher Details. Id: {Id}", id);
                TempData["Error"] = "An exception occurred while gathering targeted teacher records.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult ExportToExcelMessage()
        {
            try
            {
                TempData["Success"] = "Teacher Excel export started.";
                return RedirectToAction(nameof(ExportToExcel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inside redirection routing for ExportToExcelMessage.");
                return RedirectToAction(nameof(ExportToExcel));
            }
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            try
            {
                _logger.LogInformation("Teacher Excel Export started.");

                var teachers = _context.Teachers.ToList();

                _logger.LogInformation("Total Teachers fetched for export: {Count}", teachers.Count);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Teachers");

                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Name";
                    worksheet.Cell(1, 3).Value = "Skills";
                    worksheet.Cell(1, 4).Value = "Total Students";
                    worksheet.Cell(1, 5).Value = "Salary";
                    worksheet.Cell(1, 6).Value = "ImageUrl";

                    int row = 2;
                    foreach (var item in teachers)
                    {
                        worksheet.Cell(row, 1).Value = item.Id;
                        worksheet.Cell(row, 2).Value = item.Name;
                        worksheet.Cell(row, 3).Value = item.Skills;
                        worksheet.Cell(row, 4).Value = item.TotalStudents;
                        worksheet.Cell(row, 5).Value = item.Salary;
                        worksheet.Cell(row, 6).Value = item.ImageUrl;
                        row++;
                    }

                    // Layout improvement: Auto-fit contents cleanly
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        _logger.LogInformation("Teacher Excel Export completed successfully.");

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Teachers_{DateTime.Now:MMddHH}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting Teachers to Excel.");
                TempData["Error"] = "Failed to export data to Excel format.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult TeacherPieChart()
        {
            try
            {
                _logger.LogInformation("Teacher Pie Chart requested.");

                var chartData = _context.Teachers
                    .Select(x => new TeacherChartViewModel
                    {
                        TeacherName = x.Name,
                        TotalStudents = x.TotalStudents
                    })
                    .ToList();

                _logger.LogInformation("Teacher Pie Chart data loaded successfully. Teacher Count: {Count}", chartData.Count);

                ViewBag.ChartData = JsonConvert.SerializeObject(chartData);
                ViewBag.TotalTeachers = _context.Teachers.Count();
                ViewBag.TotalStudents = _context.Teachers.Sum(x => x.TotalStudents);
                ViewBag.HighestStudentCount = _context.Teachers.Max(x => x.TotalStudents);

                var anyTeachers = _context.Teachers.Any();
                ViewBag.AverageStudents = anyTeachers
                    ? Math.Round(_context.Teachers.Average(x => x.TotalStudents), 2)
                    : 0;

                _logger.LogInformation("Teacher Pie Chart KPIs loaded successfully.");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Teacher Pie Chart.");
                TempData["Error"] = "Unable to load Teacher Pie Chart dashboard visuals.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult SkillBarChart()
        {
            try
            {
                _logger.LogInformation("Skill Bar Chart requested.");

                var data = _context.Teachers
                    .GroupBy(x => x.Skills)
                    .Select(g => new SkillChartViewModel
                    {
                        Skill = g.Key,
                        TotalStudents = g.Sum(x => x.TotalStudents)
                    })
                    .OrderBy(x => x.Skill)
                    .ToList();

                _logger.LogInformation("Skill Bar Chart data loaded successfully. Skill Count: {Count}", data.Count);
                ViewBag.ChartData = JsonConvert.SerializeObject(data);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Skill Bar Chart.");
                TempData["Error"] = "Unable to load Skill Bar Chart metrics.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult TeacherSalaryChart()
        {
            try
            {
                _logger.LogInformation("Teacher Salary Chart requested.");

                var chartData = _context.Teachers
                    .Select(t => new
                    {
                        TeacherName = t.Name,
                        Salary = t.Salary
                    })
                    .ToList();

                _logger.LogInformation("Teacher Salary Chart data loaded successfully. Teacher Count: {Count}", chartData.Count);
                ViewBag.ChartData = JsonConvert.SerializeObject(chartData);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading Teacher Salary Chart.");
                TempData["Error"] = "Unable to load Teacher Salary configuration breakdown.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}