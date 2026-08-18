using AutoMapper;
using Contract.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Profiles
{
    public class CustomProfile : Profile
    {
        public CustomProfile()
        {
            // File mapping: DTO -> Entity
            CreateMap<FileDto, TaskAttachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // File mapping: Entity -> DTO
            CreateMap<TaskAttachment, FileDto>();

            // Task mapping: DTO -> Entity
            CreateMap<TaskDTO, TaskItem>()
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Files))
                .ForMember(dest => dest.TaskComments, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedToUser, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedToUserId, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.AssignedToUserId) ? (Guid?)null : Guid.Parse(src.AssignedToUserId)))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => Guid.Parse(src.CreatedByUserId)))
                .ForMember(dest => dest.AssignedWorkGroupId, opt => opt.MapFrom(src => src.WorkGroupId));

            // Task mapping: Entity -> DTO
            CreateMap<TaskItem, TaskDTO>()
                .ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.Attachments))
                .ForMember(dest => dest.AssignedToUserId, opt => opt.MapFrom(src => src.AssignedToUserId != null ? src.AssignedToUserId.ToString() : null))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId.ToString()))
                .ForMember(dest => dest.WorkGroupId, opt => opt.MapFrom(src => src.AssignedWorkGroupId))
                .ForMember(dest => dest.TaskCommentId, opt => opt.MapFrom(src => src.TaskComments != null
                    ? src.TaskComments.Select(c => c.Id).ToList()
                    : null));

            CreateMap<TaskComment, TaskCommentDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.UserId)));

            CreateMap<WorkGroup, WorkGroupDTO>()
                .ForMember(dest => dest.LeaderId, opt => opt.MapFrom(src => src.LeaderId.ToString()))
                .ForMember(dest => dest.UserIds, opt => opt.MapFrom(src => src.Users.Select(u => u.Id.ToString()).ToList()))
                .ForMember(dest => dest.TaskIds, opt => opt.MapFrom(src => src.Tasks != null ? src.Tasks.Select(t => new TaskDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    AssignedToUserId = t.AssignedToUserId != null ? t.AssignedToUserId.ToString() : null,
                    CreatedByUserId = t.CreatedByUserId.ToString(),
                    Deadline = t.Deadline,
                    Status = t.Status,
                    Difficulty = t.Difficulty,
                    WorkGroupId = t.AssignedWorkGroupId
                }).ToList() : new List<TaskDTO>()));

            CreateMap<WorkGroupDTO, WorkGroup>()
                .ForMember(dest => dest.Users, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore())
                .ForMember(dest => dest.LeaderId, opt => opt.MapFrom(src => Guid.Parse(src.LeaderId)));
        }
    }
}
