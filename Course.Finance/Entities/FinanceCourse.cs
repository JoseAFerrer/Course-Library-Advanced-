using CourseLibrary.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Course.Finance.Entities
{
    public class FinanceCourse : CourseDto
    {
        //public Guid Id { get; set; } //All of this is inherited from CourseDto.
        //public string Title { get; set; }
        //public string Description { get; set; }
        //public Guid AuthorId { get; set; }
        public double Price = 12.5;
    }
}
