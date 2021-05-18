using CourseLibrary.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Course.Finance.Entities
{
    public class FinanceCourse // Este es un Course más sencillo y ligero. Si alguien quiere más información se hace fetch del curso entero.
    {
        public Guid Id { get; set; } 
        public string Title { get; set; }
        public double Price = 12.5;
    }
}
