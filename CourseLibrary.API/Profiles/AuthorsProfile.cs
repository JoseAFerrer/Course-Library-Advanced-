using AutoMapper;
using CourseLibrary.API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Profiles
{
    public class AuthorsProfile : Profile
    {
        public AuthorsProfile()
        {
            CreateMap<Entities.Author, Models.AuthorDto>()
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(
                    dest => dest.Age,
                    opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge(src.DateOfDeath)));

            CreateMap<Models.AuthorForCreationDto, Entities.Author>();

            CreateMap<Models.AuthorForCreationWithDateOfDeathDto, Entities.Author>();

            CreateMap<Entities.Author, Models.AuthorFullDto>();

            //And now, let's get to the maps that convert from entities (courses) to persistence models.

            CreateMap<Entities.Author, Persistence.PersistenceModels.AuthorDocument>()
                .ForMember(destinationMember => destinationMember.Id,
                opts => opts.MapFrom(source => source.Id.ToString()))
                .ForMember(destinationMember => destinationMember.Courses,
                opts => opts.MapFrom(source => source.Courses)); //¿Habría que invocar el otro mapping, el de cursos? Dudas, grandes dudas.

            CreateMap<Persistence.PersistenceModels.AuthorDocument, Entities.Author>()
                .ForMember(destinationMember => destinationMember.Id,
                options => options.MapFrom(source => Guid.Parse(source.Id)))
                .ForMember(destinationMember => destinationMember.Courses,
                options => options.MapFrom(source => source.Courses)); //¿El Parse está bien puesto o sobra?;
        }
    }
}
