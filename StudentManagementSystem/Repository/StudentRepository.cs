using System.Data.Entity;
using System.Linq;
using StudentManagementSystem.Models;


namespace StudentManagementSystem.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SMSDBNEWEntities _context;

        public StudentRepository(SMSDBNEWEntities context)
        {
            _context = context;
        }

        public IQueryable<Student> GetAll() => _context.Students;

        public Student GetById(int id) => _context.Students.Find(id);

        public void Add(Student student) => _context.Students.Add(student);

        public void Update(Student student)
        {
            var existing = _context.Students.Local.FirstOrDefault(f => f.Id == student.Id);
            if (existing != null) _context.Entry(existing).State = EntityState.Detached;

            _context.Entry(student).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            var student = _context.Students.Find(id);
            if (student != null) _context.Students.Remove(student);
        }

        public void Save() => _context.SaveChanges();
    }
}