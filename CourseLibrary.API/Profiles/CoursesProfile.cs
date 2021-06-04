using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Profiles
{
    public class CoursesProfile : Profile
    {
        public CoursesProfile()
        {
            CreateMap<Entities.Course, Models.CourseDto>();
            CreateMap<Models.CourseForCreationDto, Entities.Course>();
            CreateMap<Models.CourseForUpdateDto, Entities.Course>();
            CreateMap<Entities.Course, Models.CourseForUpdateDto >();

            //And now, let's get to the maps that convert from entities (courses) to persistence models
            //and the other way around.

            CreateMap<Entities.Course, Persistence.PersistenceModels.CourseDocument>()
                .ForMember(destinationMember => destinationMember.Id,
                opts => opts.MapFrom(source => source.Id.ToString())) 
                .ForMember(destinationMember => destinationMember.AuthorId,
                opts => opts.MapFrom(source => source.AuthorId.ToString())); 
            
            CreateMap<Persistence.PersistenceModels.CourseDocument, Entities.Course>()
                .ForMember(destinationMember => destinationMember.Id,
                options => options.MapFrom(source => Guid.Parse(source.Id)))
                .ForMember(destinationMember => destinationMember.AuthorId,
                options => options.MapFrom(source => Guid.Parse(source.AuthorId))); //Importante e interesante:
                                                                                    // aquí se guarda el id del autor,
                                                                                    // si la VISTA requiere que esté el autor ya se encargarán
                                                                                    // los niveles superiores de hacerlo.


        }
    }
}
