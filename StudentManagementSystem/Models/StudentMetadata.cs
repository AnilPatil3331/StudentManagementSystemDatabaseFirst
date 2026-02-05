using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Models
{
    [MetadataType(typeof(StudentMetadata))]
    public partial class Student { }
    public class StudentMetadata
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be 10 digits")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Only numbers are allowed")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        public int Course { get; set; }
    }
}