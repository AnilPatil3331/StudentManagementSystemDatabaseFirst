using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Services
{
    public interface IStudentService
    {
        IEnumerable<Student> GetStudents(string search, string course);
        Student Get(int id);
        void Create(Student student);
        void Edit(Student student);
        void Remove(int id);
    }
}
