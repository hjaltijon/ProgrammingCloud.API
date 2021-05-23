using AutoMapper;
using ProgrammingCloud.API.Models.DataEntities;
using ProgrammingCloud.API.Models.DTOs;
using ProgrammingCloud.API.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<UserForUpdateDTO, User>();
            //CreateMap<User, UserForUpdateDTO>();

            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();

            CreateMap<UserEntity, User>();
            CreateMap<User, UserEntity>();
            //--
            CreateMap<ClassroomEntity, Classroom>();
            CreateMap<Classroom, ClassroomEntity>();

            CreateMap<ClassroomDTO, Classroom>();
            CreateMap<Classroom, ClassroomDTO>();

            CreateMap<ClassroomForCreationDTO, Classroom>();
            CreateMap<Classroom, ClassroomForCreationDTO>();
            //--
            CreateMap<UserClassroomRelation, UserClassroomRelationDTO>();
            CreateMap<UserClassroomRelationDTO, UserClassroomRelation>();
            //--
            CreateMap<ProblemEntity, Problem>();
            CreateMap<Problem, ProblemEntity>();

            CreateMap<ProblemDTO, Problem>();
            CreateMap<Problem, ProblemDTO>();

            CreateMap<ProblemForCreationDTO, Problem>();
            CreateMap<Problem, ProblemForCreationDTO>();

            CreateMap<ProblemForUpdateDTO, Problem>();
            CreateMap<Problem, ProblemForUpdateDTO>();
            //--
            CreateMap<ProblemClassroomRelationEntity, ProblemClassroomRelation>();
            CreateMap<ProblemClassroomRelation, ProblemClassroomRelationEntity>();

            CreateMap<ProblemClassroomRelationDTO, ProblemClassroomRelation>();
            CreateMap<ProblemClassroomRelation, ProblemClassroomRelationDTO>();

            //--
            CreateMap<ClassroomInvite, ClassroomInviteForCreationDTO>();
            CreateMap<ClassroomInviteForCreationDTO, ClassroomInvite>();

            CreateMap<ClassroomInviteEntity, ClassroomInvite>();
            CreateMap<ClassroomInvite, ClassroomInviteEntity>();

            CreateMap<ClassroomInviteDTO, ClassroomInvite>();
            CreateMap<ClassroomInvite, ClassroomInviteDTO>();

            
        }
    }
}
