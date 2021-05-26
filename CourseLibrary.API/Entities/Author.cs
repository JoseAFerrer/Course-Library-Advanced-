using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Entities
{
    public class Author
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public DateTimeOffset DateOfBirth { get; set; } 
        public DateTimeOffset? DateOfDeath { get; set;}

        [Required]
        [MaxLength(50)]
        public string MainCategory { get; set; }

        public List<Course> Courses { get; set; }
    }
}
