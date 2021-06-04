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
using Raven.Client.Documents.Linq;

namespace CourseLibrary.API.Services
{
    public class RavenDbCoursesRepository : ICourseLibraryRepository
    {
        private readonly IDocumentStore _documentStore;
        private readonly IMapper _mapper;
        //private readonly IPropertyMappingService _propertyMappingService;

        public RavenDbCoursesRepository(IDocumentStore documentStore,
            IMapper mapper,
            IPropertyMappingService propertyMappingService)
        {
            _documentStore = documentStore;
            _mapper= mapper;
            //_propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
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

            authorCourses = _mapper.Map<List<Course>>(authorCoursesFromDb);

            return authorCourses;
        }

        public async Task<Course> GetCourse(Guid authorId, Guid courseId)
        {
            using var session = _documentStore.OpenAsyncSession();

            var authorCourseFromDB = await session
                                            .LoadAsync<CourseDocument>(courseId.ToString(),
                                            builder => builder
                                            .IncludeDocuments(authorId.ToString())); //Precargamos el autor a través de su id.

            var authorCourse = _mapper.Map<Course>(authorCourseFromDB); //Este mapeo deja al autor vacío. Ahora lo rellenaremos.

            var authorFromDB = await session.LoadAsync<AuthorDocument>(authorId.ToString());

            authorCourse.Author = _mapper.Map<Author>(authorFromDB); //Note that this particular author has an empty list of courses BECAUSE YOU DON'T NEED TO GET TO THE SECOND LEVEL.

            return authorCourse;
        }
        public async Task<Course> GetCourse(Guid courseId)
        {
            using var session = _documentStore.OpenAsyncSession();

            var authorCourseFromDB = await session
                .LoadAsync<CourseDocument>(courseId.ToString(),
                builder => builder
                .IncludeDocuments(x =>x.AuthorId)); //Esto precarga el autor desde su id.

            var authorCourse = _mapper.Map<Course>(authorCourseFromDB);

            var authorFromDB = await session.LoadAsync<AuthorDocument>(authorCourse.AuthorId.ToString());

            authorCourse.Author = _mapper.Map<Author>(authorFromDB);

            return authorCourse;
        }

        public async Task<Course> AddCourse(Guid authorId, Course course)
        {
            course.Id = Guid.NewGuid(); //Este id se pasará luego a string dentro del mapeo.
            course.AuthorId = authorId;

            var courseToDB = _mapper.Map<CourseDocument>(course);

            using var session = _documentStore.OpenAsyncSession();

            await session.StoreAsync(courseToDB);
            await session.SaveChangesAsync();

            return course;
        }

        public async void UpdateCourse(Course course) //El peligro de hacer Upsert es que se sobreescribe el objeto entero,se haya pasado toda la información o no.
                                                      //Por eso lo hacemos de la siguiente manera:
        {
            var courseToDB = _mapper.Map<CourseDocument>(course);

            using var session = _documentStore.OpenAsyncSession();

            var courseFromDB = await session.LoadAsync<CourseDocument>(courseToDB.Id); 

            #region Update each non-empty property.
            if (courseToDB.Title != null)
            {
                courseFromDB.Title = courseToDB.Title;
            }
            if (courseToDB.Description != null)
            {
                courseFromDB.Description = courseToDB.Description;
            }
            if (courseToDB.AuthorId != null)
            {
                courseFromDB.AuthorId = courseToDB.AuthorId;
            }
            #endregion

            await session.SaveChangesAsync(); //La base de datos está escuchando para ver qué cambios le haces al objeto que has cargado.
        }

        public async void DeleteCourse(Course course)
        {
            var courseToDB = _mapper.Map<CourseDocument>(course);

            using var session = _documentStore.OpenAsyncSession();

            session.Delete<CourseDocument>(courseToDB); 

            await session.SaveChangesAsync();
        }

        //Este método nunca se llama desde el controlador: lo que ocurre es que se le llama desde el paginador
        // para que haga el trabajo sucio de base de datos y todo eso.
        public async Task<IEnumerable<Author>> GetAuthors(Raven.Client.Documents.Session.IAsyncDocumentSession session)
        {
            var authorsFromDb = await session
                                        .Query<AuthorDocument>()
                                        .OfType<AuthorDocument>()
                                        .Include(x => x.CoursesIds) //Include: cláusula de la query. Precárgame este Id y luego dámelo
                                                                    //(así te ahorras un viaje a base de datos).
                                        .ToListAsync();

            var authors = new List<Author>();

            foreach (AuthorDocument authorDB in authorsFromDb) //Iterate through all authors.
            {
                Author convertedAuthor = await ComplexMapFromAuthorDBToAuthor(authorDB, session); //Note this loads all courses for every author, but the author of those courses is empty.

                authors.Add(convertedAuthor);
            }

            return authors;
        } 

        public async Task<PagedList<Author>> GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            if (authorsResourceParameters == null)
            {
                throw new ArgumentNullException(nameof(authorsResourceParameters));
            }

            using var session = _documentStore.OpenAsyncSession();

            var collection = session
                                        .Query<AuthorDocument>()
                                        .OfType<AuthorDocument>()
                                        .Include(x => x.CoursesIds); //Include: cláusula de la query. Precárgame este Id y luego dámelo

            #region Filtering by MainCategory and SearchQuery 
            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.MainCategory))
            {
                var mainCategory = authorsResourceParameters.MainCategory.Trim();
                collection = collection.Where<AuthorDocument>(a => a.MainCategory == mainCategory);

            }

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.SearchQuery))
            {
                var searchQuery = authorsResourceParameters.SearchQuery.Trim();
                collection = collection.Where(a => searchQuery.In(a.MainCategory)
                       || searchQuery.In(a.FirstName)
                       || searchQuery.In(a.LastName));
            }
            #endregion

            var recollection = await collection.ToListAsync();
            var preauthors = new List<Author>();

            foreach (AuthorDocument authorDB in recollection) //Iterate through all authors. IF YOU ITERATE, YOU ARE FORCING THE QUERY TO HAPPEN.
            {
                Author convertedAuthor = await ComplexMapFromAuthorDBToAuthor(authorDB, session); //Note this loads all courses for every author, but the author of those courses is empty.

                preauthors.Add(convertedAuthor);
            }

            var authors = preauthors.AsQueryable<Author>();

            return PagedList<Author>.Create(authors,
                authorsResourceParameters.PageNumber,
                authorsResourceParameters.PageSize);

            ////////////////////////////////////
            //if (!string.IsNullOrWhiteSpace(authorsResourceParameters.OrderBy))
            //{
            //    //Get property mapping dictionary
            //    var authorPropertyMappingDictionary =
            //        _propertyMappingService.GetPropertyMapping<AuthorDto, Author>(); //Todo: fix something about this,
            //                                                                         //I think the problem is the new format I'm feeding the program.

            //    authors = authors.ApplySort(authorsResourceParameters.OrderBy,
            //       authorPropertyMappingDictionary);
            //}
            //Todo: reentender bien cómo funciona el ApplySort e implementarlo.


            
        }

        public async Task<Author> GetAuthor(Guid authorId)
        {
            var id = authorId.ToString();

            using var session = _documentStore.OpenAsyncSession();

            var authorFromDB = await session
                                .LoadAsync<AuthorDocument>(id, builder => builder
                                .IncludeDocuments<AuthorDocument>(x =>
                                    x.CoursesIds));

            var author = await ComplexMapFromAuthorDBToAuthor(authorFromDB, session);

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
                .Include(x => x.CoursesIds)
                .OfType<AuthorDocument>()
                .ToListAsync();

            var authors = new List<Author>();
            foreach (AuthorDocument authorDB in authorsFromDb)
            {
                authors.Add(await ComplexMapFromAuthorDBToAuthor(authorDB, session));
            }
            return authors;
        }

        public async void AddAuthor(Author author)
        {
            author.Id = Guid.NewGuid();

            var coursesToDB = new List<CourseDocument>();
            foreach (var course in author.Courses)
            {
                course.Id = Guid.NewGuid();
                course.AuthorId = author.Id;
                coursesToDB.Add(_mapper.Map<CourseDocument>(course));
            }

            var authorToDB = _mapper.Map<AuthorDocument>(author);

            using var session = _documentStore.OpenAsyncSession();
            await session.StoreAsync(authorToDB);

            foreach (var course in coursesToDB) //En caso de verme obligado a hacerlo así, que parece que sí, me tengo que comer el límite de 32 operaciones...
            {
                await session.StoreAsync(course);
            }
            // await session.StoreAsync(coursesToDB); //Todo: Joao: ¿al guardar una lista se guarda la lista o los elemntos? Esto me da una excepción, for some reason.
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

        private async Task<Author> ComplexMapFromAuthorDBToAuthor(AuthorDocument authorFromDB,
            Raven.Client.Documents.Session.IAsyncDocumentSession session)              //Estamos usando, al pasar la sesión abierta, los datos que hemos precargado en memoria.
                                                                                       //El límite de operaciones por sesión a base de datos ya no nos afecta (!)
        {
            var convertedAuthor = _mapper.Map<Author>(authorFromDB); //For each author, recover the mapping.
                                                                     //
            // var coursesFromDB = await session.LoadAsync<CourseDocument>(authorFromDB.CoursesIds.ToArray()); 
            //¡Aquí se carga un diccionario porque también van los ids! Confirmar.
            
            var coursesFromDB = await session.LoadAsync<CourseDocument>(authorFromDB.CoursesIds.ToArray());

            var x = coursesFromDB.Values.ToList<CourseDocument>();

            var convertedCourses = _mapper.Map<List<Course>>(x);

            convertedAuthor.Courses = convertedCourses;

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
    }
}