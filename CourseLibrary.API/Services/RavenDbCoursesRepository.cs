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
        public async Task<Course> GetCourse(Guid courseId)
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
            course.Id = Guid.NewGuid(); //Pasar a .ToString();
            course.AuthorId = authorId;

            var courseToDB = _mapper.Map<CourseDocument>(course);

            using var session = _documentStore.OpenAsyncSession();

            await session.StoreAsync(courseToDB);
            await session.SaveChangesAsync();

            return course;
        }

        public async void UpdateCourse(Course course)
        {
            var courseToDB = _mapper.Map<CourseDocument>(course);

            using var session = _documentStore.OpenAsyncSession();

            await session.StoreAsync(courseToDB); //Directamente guardamos el dato nuevo. El peligro de esto es que
                                                  //se sobreescribe el objeto entero, se haya pasado toda la información o no.

            // Carga un curso y se actualiza con un foreach.
            await session.SaveChangesAsync(); //La base de datos está escuchando para ver qué cambios le haces al objeto que has cargado.
        }

        public async void DeleteCourse(Course course)
        {
            var courseToDB = _mapper.Map<CourseDocument>(course);

            using var session = _documentStore.OpenAsyncSession();

            session.Delete<CourseDocument>(courseToDB); //Cannot await void?

            await session.SaveChangesAsync();
        }

        public async Task<IEnumerable<Author>> GetAuthors()
        {
            using var session = _documentStore.OpenAsyncSession();

            var authorsFromDb = await session.Query<AuthorDocument>().OfType<AuthorDocument>().ToListAsync();

            var authors = new List<Author>();

            foreach (AuthorDocument authorDB in authorsFromDb)
            {
                authors.Add(_mapper.Map<Author>(authorDB));
            }

            return authors;
        }

        
        //Not implemented
        //Todo: Do it!
        public PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            throw new NotImplementedException();
        }

        public async Task<Author> GetAuthor(Guid authorId)
        {
            var id = authorId.ToString();

            using var session = _documentStore.OpenAsyncSession();

            var authorFromDB = await session.LoadAsync<AuthorDocument>(id);

            var author = _mapper.Map<Author>(authorFromDB);

            return author;
        }

        public async Task<IEnumerable<Author>> GetAuthors(IEnumerable<Guid> authorIds)
        {
            var ids = new List<string>();
            foreach (var id in authorIds)
            {
                ids.Add(id.ToString());
            }
            using var session = _documentStore.OpenAsyncSession();

            var authorsFromDb = await session
                .Query<AuthorDocument>()
                .Where(x => ids.Contains(x.Id))
                .OfType<AuthorDocument>()
                .ToListAsync();

            var authors = new List<Author>();

            foreach (AuthorDocument authorDB in authorsFromDb)
            {
                authors.Add(_mapper.Map<Author>(authorDB));
            }

            return authors;
        }

        public async void AddAuthor(Author author)
        {
            author.Id = Guid.NewGuid();

            var authorToDB = _mapper.Map<CourseDocument>(author);

            using var session = _documentStore.OpenAsyncSession();
            await session.StoreAsync(authorToDB);
            await session.SaveChangesAsync();
        }

        public async void DeleteAuthor(Author author)
        {
            author.Id = Guid.NewGuid();

            var authorToDB = _mapper.Map<CourseDocument>(author);

            using var session = _documentStore.OpenAsyncSession();
            session.Delete(authorToDB);
            await session.SaveChangesAsync();
        }

        public async void UpdateAuthor(Author author) //Imita los cambios del UpdateCourse
        {
            var authorToDB = _mapper.Map<CourseDocument>(author);

            using var session = _documentStore.OpenAsyncSession();
            await session.StoreAsync(authorToDB);
            await session.SaveChangesAsync();
        }

        public bool AuthorExists(Guid authorId)
        {
            var id = authorId.ToString();

            using var session = _documentStore.OpenAsyncSession();
            var authorFromDB =  session.LoadAsync<AuthorDocument>(id).Result; //Podemos hacerlo async o así. Sospecho que async es más elegante.
            if (authorFromDB != null)
            {
                return true;
            }
            return false;
        }

        public bool Save()
        {
            throw new NotImplementedException();
        }
    }
}