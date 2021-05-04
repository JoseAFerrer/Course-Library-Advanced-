using CourseLibrary.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescription(
            ErrorMessage = "The title shouldn't be the same as the description. You are a very bad boy...")]
    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a title! Bad boy...")]
        [MaxLength(100, ErrorMessage = "The title shouldn't be so long. Bad boy...")]
        public string Title { get; set; }

        [MaxLength(1200, ErrorMessage = "The description shouldn't be so long. Bad boy...")]
        public virtual string Description { get; set; }
    }
}
