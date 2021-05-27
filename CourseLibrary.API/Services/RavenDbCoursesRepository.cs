using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
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

            var authorCourseFromDB = await session.LoadAsync<CourseDocument>(courseId.ToString());
            var authorCourse = _mapper.Map<Course>(authorCourseFromDB);
            return authorCourse;
        }
        public async Task<Course> GetCourse(Guid courseId)
        {
            using var session = _documentStore.OpenAsyncSession();

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

        public async void UpdateCourse(Course course) //El peligro de hacer Upsert es que se sobreescribe el objeto entero,
                                                      //se haya pasado toda la información o no.
                                                      //Por eso lo hacemos de la siguiente manera:
        {
            var courseToDB = _mapper.Map<CourseDocument>(course);

            using var session = _documentStore.OpenAsyncSession();

            var courseFromDB = await session.LoadAsync<CourseDocument>(courseToDB.Id); 

            #region Save each property if it is not empty.
            if (courseToDB.Title != null)
            {
                courseFromDB.Title = courseToDB.Title;
            }
            if (courseToDB.Description != null)
            {
                courseFromDB.Description = courseToDB.Description;
            }
            #endregion

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

            var authorsFromDb = await session
                                        .Query<AuthorDocument>()
                                        .OfType<AuthorDocument>()
                                        .ToListAsync();

            var authors = new List<Author>(); //Todo: Joao: ¿es demasiado trabajo para este método?
                                              //¿Debería extraer todo este proceso a un nuevo método
                                              //o este es el lugar adecuado para hacerlo?

            foreach (AuthorDocument authorDB in authorsFromDb) //Iterate through all authors.
            {
                Author convertedAuthor = await ComplexMapFromAuthorDBToAuthor(authorDB);

                authors.Add(convertedAuthor);
            }


            return authors;
        }

        //Not implemented
        //Todo: Do it, using the method before.
        public async Task<PagedList<Author>> GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            if (authorsResourceParameters == null)
            {
                throw new ArgumentNullException(nameof(authorsResourceParameters));
            }

            using var session = _documentStore.OpenAsyncSession();

            var collection = await GetAuthors() as IQueryable<Author>;

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.MainCategory))
            {
                var mainCategory = authorsResourceParameters.MainCategory.Trim();
                collection = collection.Where(a => a.MainCategory == mainCategory);
            }

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.SearchQuery))
            {
                var searchQuery = authorsResourceParameters.SearchQuery.Trim();
                collection = collection.Where(a => a.MainCategory.Contains(searchQuery)
                       || a.FirstName.Contains(searchQuery)
                       || a.LastName.Contains(searchQuery));
            }

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.OrderBy))
            {
                //Get property mapping dictionary
                var authorPropertyMappingDictionary =
                    _propertyMappingService.GetPropertyMapping<AuthorDto, Author>();


                collection = collection.ApplySort(authorsResourceParameters.OrderBy,
                   authorPropertyMappingDictionary);
            }

            return PagedList<Author>.Create(collection,
                authorsResourceParameters.PageNumber,
                authorsResourceParameters.PageSize); //We are using deferred execution inside the method: the query doesn't execute until we get here
        }

        public async Task<Author> GetAuthor(Guid authorId)
        {
            var id = authorId.ToString();

            using var session = _documentStore.OpenAsyncSession();

            var authorFromDB = await session.LoadAsync<AuthorDocument>(id);

            var author = await ComplexMapFromAuthorDBToAuthor(authorFromDB);

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
            {//Todo: Joao: ¿se puede hacer un await dentro de otro método? Wow.
                //var workingAuthor = await ComplexMapFromAuthorDBToAuthor(authorDB);
                authors.Add(await ComplexMapFromAuthorDBToAuthor(authorDB));
            }
            return authors;
        }

        public async void AddAuthor(Author author)
        {
            author.Id = Guid.NewGuid();

            var authorToDB = _mapper.Map<AuthorDocument>(author);

            using var session = _documentStore.OpenAsyncSession();
            await session.StoreAsync(authorToDB);
            await session.SaveChangesAsync();
        }

        public async void DeleteAuthor(Author author)
        {
            author.Id = Guid.NewGuid();

            var authorToDB = _mapper.Map<AuthorDocument>(author);

            using var session = _documentStore.OpenAsyncSession();
            session.Delete(authorToDB);
            await session.SaveChangesAsync();
        }

        //Todo: checkear todos los usos de CourseDocument por si he puesto eso en alguno de los autores (se rompería todo).
        public async void UpdateAuthor(Author author)
        {
            var authorToDB = _mapper.Map<AuthorDocument>(author);

            using var session = _documentStore.OpenAsyncSession();
            var authorFromDB = await session.LoadAsync<AuthorDocument>(authorToDB.Id);

            #region Saving the changes to then update (not upsert) the author.
            if (authorToDB.FirstName != null)
            {
                authorFromDB.FirstName = authorToDB.FirstName;
            }
            if (authorToDB.LastName != null)
            {
                authorFromDB.LastName = authorToDB.LastName;
            }
            if (authorToDB.DateOfBirth != null)
            {
                authorFromDB.DateOfBirth = authorToDB.DateOfBirth;
            }
            if (authorToDB.MainCategory != null)
            {
                authorFromDB.MainCategory = authorToDB.MainCategory;
            }
            if (authorToDB.DateOfDeath != null)
            {
                authorFromDB.DateOfDeath = authorToDB.DateOfDeath;
            }
            #endregion

            await session.SaveChangesAsync();
        }

        private async Task<Author> ComplexMapFromAuthorDBToAuthor(AuthorDocument authorFromDB)
        {
            var convertedAuthor = _mapper.Map<Author>(authorFromDB); //For each author, recover the mapping.
            var workingAuthor = convertedAuthor;
            convertedAuthor.Courses.Clear(); //Erase the empty courses to refill the list with the full courses.
            foreach (var emptyCourse in workingAuthor.Courses)
            {
                var fullCourse = await GetCourse(emptyCourse.Id);
                convertedAuthor.Courses.Add(fullCourse);
            }

            return convertedAuthor;
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