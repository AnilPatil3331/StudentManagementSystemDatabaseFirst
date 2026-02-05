using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using StudentManagementSystem.Models;


namespace StudentManagementSystem.Repository
{
    public interface IStudentRepository
    {
        IQueryable<Student> GetAll();
        Student GetById(int id);
        void Add(Student student);
        void Update(Student student);
        void Delete(int id);
        void Save();
    }
}
