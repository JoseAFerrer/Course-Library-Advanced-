using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Helpers
{
    public static class AuthorExtensions
    {
        public static IEnumerable<string> GetCoursesIdsAsStrings(this Entities.Author author)
        {
            var ids = new List<string>();
            foreach (var course in author.Courses)
            {
                ids.Add(course.Id.ToString());
            }
            return ids;
        }
    }
}
