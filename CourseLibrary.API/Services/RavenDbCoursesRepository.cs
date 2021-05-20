using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Persistence.PersistenceModels;
using CourseLibrary.API.ResourceParameters;
using Raven.Client.Documents;

namespace CourseLibrary.API.Services
{
    public class RavenDbCoursesRepository : ICourseLibraryRepository
    {
        private readonly IDocumentStore _documentStore;
        private readonly IMapper _mapper;

        public RavenDbCoursesRepository(IDocumentStore documentStore,
            IMapper mapper)
        {
            _documentStore = documentStore;
            _mapper= mapper;
        }

        public async Task<IEnumerable<Course>> GetCourses(Guid authorId)
        {
            using var session = _documentStore.OpenAsyncSession();

            var authorCoursesFromDb =  await session
                .Query<CourseDocument>() // Decirle a la base de datos lo que tiene que buscar.
                .Where(x => x.AuthorId == authorId.ToString())
                .OfType<CourseDocument>() //El tipo es a qué tiene que convertir para poder trabajar con él
                .ToListAsync();

            var authorCourses = new List<Course>();

            foreach (CourseDocument courseDB in authorCoursesFromDb)
            {
                authorCourses.Add(_mapper.Map<Course>(authorCoursesFromDb));
            }

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

            var authorCourseFromDB = await session.LoadAsync<CourseDocument>(courseId.ToString());
            var authorCourse = _mapper.Map<Course>(authorCourseFromDB);
            return authorCourse;
        }

        public async Task<Course> AddCourse(Guid authorId, Course course)
        {
            using var session = _documentStore.OpenAsyncSession();

            course.Id = Guid.NewGuid(); //Pasar a .ToString();
            course.AuthorId = authorId;

            var courseToDB = _mapper.Map<CourseDocument>(course);
            await session.StoreAsync(courseToDB);
            await session.SaveChangesAsync();

            return course;
        }

        public async void UpdateCourse(Course course)
        {
            var courseToDB = _mapper.Map<CourseDocument>(course);

            using var session = _documentStore.OpenAsyncSession();

            await session.StoreAsync(courseToDB); //Directamente guardamos el dato nuevo.

            await session.SaveChangesAsync();
        }

        public async void DeleteCourse(Course course)
        {
            var courseToDB = _mapper.Map<CourseDocument>(course);

            using var session = _documentStore.OpenAsyncSession();

            session.Delete<CourseDocument>(courseToDB); //Cannot await void?

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