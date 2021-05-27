using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.ResourceParameters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseLibrary.API.Services
{
    public interface ICourseLibraryRepository
    {    
        Task<IEnumerable<Course>> GetCourses(Guid authorId);
        Task<Course> GetCourse(Guid authorId, Guid courseId);
        Task<Course> AddCourse(Guid authorId, Course course);
        void UpdateCourse(Course course);
        void DeleteCourse(Course course);
        Task<IEnumerable<Author>> GetAuthors();
        Task<PagedList<Author>> GetAuthors(AuthorsResourceParameters authorsResourceParameters); //Filtering by categories! And paginating
        Task<Author> GetAuthor(Guid authorId);
        Task<IEnumerable<Author>> GetAuthors(IEnumerable<Guid> authorIds);
        void AddAuthor(Author author);
        void DeleteAuthor(Author author);
        void UpdateAuthor(Author author);
        bool AuthorExists(Guid authorId);
        bool Save();
    }
}
