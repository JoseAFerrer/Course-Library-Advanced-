using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Persistence.PersistenceModels
{
    public class CourseDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public AuthorDocument Author { get; set; } //Todo: aquí sería el id del autor. Si Joao aprueba lo que he hecho en Author para los ids de los cursos, hacer lo mismo aquí, es decir: cargarse la propiedad Autor y dejar solo su Id.
        public string AuthorId { get; set; }
    }
}
