using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Persistence.PersistenceModels
{
    public class AuthorDocument
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public DateTimeOffset? DateOfDeath { get; set; }
        public string MainCategory { get; set; }
        public ICollection<string> CoursesIds { get; set; }
            = new List<string>();

        [JsonIgnore]
        public ICollection<Entities.Course> Courses { get; set; }
     = new List<Entities.Course>(); //Todo: ver si esto es necesario cuando cree el segundo mapping.
    }
}
