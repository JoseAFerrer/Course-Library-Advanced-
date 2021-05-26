using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseLibrary.API.Services;
using Raven.Client.Documents;

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
        public static IEnumerable<Entities.Course> ConvertFromStringIdsToMostlyEmptyCourses(this Persistence.PersistenceModels.AuthorDocument author)
        {
            var courses = new List<Entities.Course>();
            foreach (var id in author.CoursesIds)
            {
                courses.Add(new Entities.Course { Id = Guid.Parse(id) });
            }
            return courses;
        }
    }
}
