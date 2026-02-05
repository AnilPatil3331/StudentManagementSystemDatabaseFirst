using System;
using System.Collections.Generic;
using System.Linq;
using StudentManagementSystem.Models;
using StudentManagementSystem.Repository;

namespace StudentManagementSystem.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repo;

        public StudentService(IStudentRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<Student> GetStudents(string search, string course)
        {
            var data = _repo.GetAll();

            // Search Filter
            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x => x.Name.Contains(search) || x.Email.Contains(search));
            }

            // Course Filter
            if (!string.IsNullOrEmpty(course) && Enum.TryParse(course, out Course selectedCourse))
            {
                data = data.Where(x => x.Course == (int)selectedCourse);
            }

            return data.ToList();
        }

        public Student Get(int id) => _repo.GetById(id);
        public void Create(Student s) { _repo.Add(s); _repo.Save(); }
        public void Edit(Student s) { _repo.Update(s); _repo.Save(); }
        public void Remove(int id) { _repo.Delete(id); _repo.Save(); }
    }
}