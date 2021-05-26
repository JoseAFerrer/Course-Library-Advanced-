﻿using System;
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
        public AuthorDocument Author { get; set; } //Aquí sería el id del autor.
        public string AuthorId { get; set; }
    }
}
