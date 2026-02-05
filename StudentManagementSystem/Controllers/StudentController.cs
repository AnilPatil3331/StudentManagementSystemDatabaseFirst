using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Web;

namespace StudentManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // ------------------- INDEX -------------------
        public ActionResult Index(string search, string course, string sortOrder, int page = 1)
        {
            int pageSize = 5;

       
            var students = _studentService.GetStudents(search, course).AsQueryable();

            ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.DateSortParam = sortOrder == "Date" ? "date_desc" : "Date";

            switch (sortOrder)
            {
                case "name_asc": students = students.OrderBy(s => s.Name);break;
                case "name_desc": students = students.OrderByDescending(s => s.Name); break;
                case "Date": students = students.OrderBy(s => s.DateOfBirth); break;
                case "date_desc": students = students.OrderByDescending(s => s.DateOfBirth); break;
                default: students = students.OrderBy(s => s.Id); break;
            }

            var studentList = students.ToList();
            int totalRecords = students.Count();

            var pagedData = studentList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.Search = search;
            ViewBag.Course = course;
            ViewBag.SortOrder = sortOrder;

          
            ViewBag.Courses = Enum.GetValues(typeof(Course)).Cast<Course>()
                .Select(c => new SelectListItem
                {
                    Value = ((int)c).ToString(), 
                    Text = c.ToString(),
                    Selected = (course == ((int)c).ToString())
                }).ToList();

            return View(pagedData);
        }

        // ------------------- CREATE -------------------
        public ActionResult Create()
        {
            SetCourseViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student student)
        {
           
            if (_studentService.GetStudents(null, null).Any(x => x.Email == student.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (ModelState.IsValid)
            {
                _studentService.Create(student);
                TempData["Success"] = "Student created successfully!";
                return RedirectToAction("Index");
            }

            SetCourseViewBag();
            return View(student);
        }

        // ------------------- EDIT -------------------
        public ActionResult Edit(int id)
        {
            var student = _studentService.Get(id);
            if (student == null) return HttpNotFound();

            SetCourseViewBag(student.Course); 
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Student student)
        {
            if (_studentService.GetStudents(null, null)
                               .Any(x => x.Email == student.Email && x.Id != student.Id))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (ModelState.IsValid)
            {
                _studentService.Edit(student);
                TempData["Success"] = "Student updated successfully!";
                return RedirectToAction("Index");
            }

            SetCourseViewBag(student.Course);
            return View(student);
        }

        // ------------------- DELETE -------------------
        public ActionResult Delete(int id)
        {
            var student = _studentService.Get(id);
            if (student != null)
            {
                _studentService.Remove(id);
                TempData["Success"] = "Student deleted successfully!";
            }
            return RedirectToAction("Index");
        }

        private void SetCourseViewBag(object selectedValue = null)
        {
            ViewBag.Courses = Enum.GetValues(typeof(Course))
                .Cast<Course>()
                .Select(c => new SelectListItem
                {
                    Value = ((int)c).ToString(),
                    Text = c.ToString(),
                    Selected = selectedValue != null && (int)c == (int)selectedValue
                }).ToList();
        }
        // ------------------- EXPORT TO EXCEL -------------------
        public ActionResult Export(string search, string course, string sortOrder)
        {
            var studentsQuery = _studentService.GetStudents(search, course).AsQueryable();

            // Sorting Logic
            switch (sortOrder)
            {
                case "name_desc": studentsQuery = studentsQuery.OrderByDescending(s => s.Name); break;
                case "Date": studentsQuery = studentsQuery.OrderBy(s => s.DateOfBirth); break;
                case "date_desc": studentsQuery = studentsQuery.OrderByDescending(s => s.DateOfBirth); break;
                default: studentsQuery = studentsQuery.OrderBy(s => s.Id); break;
            }

            var students = studentsQuery.ToList();

           

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Students");

                // Headers
                string[] headers = { "Name", "Email", "Date of Birth", "Course", "Mobile Number" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[1, i + 1].Value = headers[i];
                    ws.Cells[1, i + 1].Style.Font.Bold = true;
                    ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Data Fill
                int row = 2;
                foreach (var s in students)
                {
                    ws.Cells[row, 1].Value = s.Name;
                    ws.Cells[row, 2].Value = s.Email;
                    ws.Cells[row, 3].Value = s.DateOfBirth.ToString("yyyy-MM-dd");
                    ws.Cells[row, 4].Value = ((Course)s.Course).ToString(); 
                    ws.Cells[row, 5].Value = s.MobileNumber;
                    row++;
                }

             
                var courseValidation = ws.DataValidations.AddListValidation(ws.Cells[2, 4, 1000, 4].Address);
                courseValidation.Error = "Please select a course from the list.";
                courseValidation.ErrorTitle = "Invalid Course";
                courseValidation.ShowErrorMessage = true;

                foreach (var c in Enum.GetNames(typeof(Course)))
                {
                    courseValidation.Formula.Values.Add(c);
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StudentsList.xlsx");
            }
        }

        // ------------------- IMPORT FROM EXCEL -------------------
        [HttpPost]
        
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                TempData["Error"] = "Please select a valid Excel file.";
                return RedirectToAction("Index");
            }

            try
            {
              

                using (var package = new ExcelPackage(file.InputStream))
                {
                    var ws = package.Workbook.Worksheets.FirstOrDefault();
                    if (ws == null || ws.Dimension == null)
                    {
                        TempData["Error"] = "The Excel file is empty.";
                        return RedirectToAction("Index");
                    }

                    int successCount = 0;
                    int skippedCount = 0;
                    int totalRows = ws.Dimension.End.Row;

                    for (int r = 2; r <= totalRows; r++)
                    {
                        string name = ws.Cells[r, 1].Text.Trim();
                        string email = ws.Cells[r, 2].Text.Trim();
                        string dobText = ws.Cells[r, 3].Text.Trim();
                        string courseText = ws.Cells[r, 4].Text.Trim();
                        string mobile = ws.Cells[r, 5].Text.Trim();

                     
                        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email) && string.IsNullOrEmpty(mobile))
                        {
                            continue;
                        }

                      
                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                        {
                            skippedCount++;
                            continue;
                        }

                      
                        var allStudents = _studentService.GetStudents(null, null);
                        if (allStudents.Any(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                        {
                            skippedCount++;
                            continue;
                        }

                      
                        if (!Enum.TryParse(courseText, true, out Course parsedCourse))
                        {
                            parsedCourse = Course.BSc; 
                        }

                       
                        var student = new Student
                        {
                            Name = name,
                            Email = email,
                            DateOfBirth = DateTime.TryParse(dobText, out DateTime dt) ? dt : DateTime.Now,
                            Course = (int)parsedCourse,
                            MobileNumber = mobile
                        };

                        _studentService.Create(student);
                        successCount++;
                    }

                    
                    if (successCount > 0 || skippedCount > 0)
                    {
                        TempData["Success"] = "Import Process Completed! Added: " + successCount + ", Skipped: " + skippedCount;
                    }
                    else
                    {
                        TempData["Error"] = "No valid data found to import.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Import Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}