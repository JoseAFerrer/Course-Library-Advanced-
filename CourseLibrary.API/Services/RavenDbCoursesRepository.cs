using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<Course>> GetCourses(Guid authorId)
        {
            using var session = _documentStore.OpenAsyncSession();

            var authorCourses =  await session
                .Query<Course>() // Decirle a la base de datos lo que tiene que buscar.
                .Where(x => x.AuthorId == authorId)
                .OfType<Course>() //El tipo es a qué tiene que convertir para poder trabajar con él
                .ToListAsync();

            return authorCourses;
        }

        public async Task<Course> GetCourse(Guid authorId, Guid courseId)
        {
            using var session = _documentStore.OpenAsyncSession();

            //var authorCourse = await session
            //    .Query<Course>()
            //    .OfType<Course>()
            //    .Where(x => x.AuthorId == authorId)
            //    .Where(x => x.Id == courseId)
            //    .FirstOrDefaultAsync(); //Esto es innecesario cuando tienes específicamente el id de lo que estás buscando
            // No necesitas hacer preguntas porque ya conoces la respuesta.

             var authorCourse = await session.LoadAsync<Course>(courseId.ToString());
            return authorCourse;
        }

        public async Task AddCourse(Guid authorId, Course course) //Estaría bien que la Task devolviera el curso recién creado.
        {
            using var session = _documentStore.OpenAsyncSession();

            course.Id = Guid.NewGuid(); //Pasar a .ToString();
            course.AuthorId = authorId;
            await session.StoreAsync(course);
            await session.SaveChangesAsync();
        }

        public async void UpdateCourse(Course course)
        {
            using var session = _documentStore.OpenAsyncSession();

            await session.StoreAsync(course); //Directamente guardamos el dato nuevo.


            await session.SaveChangesAsync();
        }

        public async void DeleteCourse(Course course)
        {
            using var session = _documentStore.OpenAsyncSession();

            session.Delete<Course>(course); //Cannot await void?

            await session.SaveChangesAsync();
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