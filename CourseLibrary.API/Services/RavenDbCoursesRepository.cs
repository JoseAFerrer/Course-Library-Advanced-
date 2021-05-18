using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.ResourceParameters;
using Raven.Client.Documents;

namespace CourseLibrary.API.Services
{
    public class RavenDbCoursesRepository : ICourseLibraryRepository
    {
        private readonly IDocumentStore _documentStore;

        public RavenDbCoursesRepository(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public IEnumerable<Course> GetCourses(Guid authorId)
        {
            throw new NotImplementedException();
        }

        public Course GetCourse(Guid authorId, Guid courseId)
        {
            throw new NotImplementedException();
        }

        public async Task AddCourse(Guid authorId, Course course)
        {
            using var session = _documentStore.OpenAsyncSession();
            

            //Crear el Id del curso
            course.AuthorId = authorId;
            await session.StoreAsync(course);
            await session.SaveChangesAsync();
        }

        public void UpdateCourse(Course course)
        {
            throw new NotImplementedException();
        }

        public void DeleteCourse(Course course)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Author> GetAuthors()
        {
            throw new NotImplementedException();
        }

        public PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            throw new NotImplementedException();
        }

        public Author GetAuthor(Guid authorId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            throw new NotImplementedException();
        }

        public void AddAuthor(Author author)
        {
            throw new NotImplementedException();
        }

        public void DeleteAuthor(Author author)
        {
            throw new NotImplementedException();
        }

        public void UpdateAuthor(Author author)
        {
            throw new NotImplementedException();
        }

        public bool AuthorExists(Guid authorId)
        {
            throw new NotImplementedException();
        }

        public bool Save()
        {
            throw new NotImplementedException();
        }
    }
}