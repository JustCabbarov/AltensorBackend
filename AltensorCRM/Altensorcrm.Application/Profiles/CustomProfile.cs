using AutoMapper;
using Altensorcrm.Contract.DTOs.CallLog;
using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Contact;
using Altensorcrm.Contract.DTOs.Deal;
using Altensorcrm.Contract.DTOs.Lead;
using Altensorcrm.Contract.DTOs.Note;
using Altensorcrm.Contract.DTOs.Organization;
using Altensorcrm.Contract.DTOs.Task;
using Altensorcrm.Domain.Entity;

namespace Altensorcrm.Application.Profiles;

public class CustomProfile : Profile
{
    public CustomProfile()
    {
        CreateMap<Address, AddressDto>().ReverseMap();
        CreateMap<CreateAddressDto, Address>();

        CreateMap<Territory, TerritoryDto>()
            .ForMember(dest => dest.TerritoryManagerName, opt => opt.MapFrom(src =>
                src.TerritoryManager != null ? $"{src.TerritoryManager.FirstName} {src.TerritoryManager.LastName}" : string.Empty))
            .ForMember(dest => dest.ParentTerritoryName, opt => opt.MapFrom(src =>
                src.ParentTerritory != null ? src.ParentTerritory.TerritoryName : string.Empty));
        CreateMap<CreateTerritoryDto, Territory>();

        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : string.Empty));
        CreateMap<CreateCommentDto, Comment>();

        CreateMap<Attachment, AttachmentDto>().ReverseMap();

        CreateMap<TaskChecklist, TaskChecklistDto>().ReverseMap();
        CreateMap<CreateTaskChecklistDto, TaskChecklist>();

        CreateMap<TaskItem, TaskDetailDto>()
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src =>
                src.AssignedUser != null ? $"{src.AssignedUser.FirstName} {src.AssignedUser.LastName}" : string.Empty));
        CreateMap<TaskItem, TaskListDto>()
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src =>
                src.AssignedUser != null ? $"{src.AssignedUser.FirstName} {src.AssignedUser.LastName}" : string.Empty));
        CreateMap<CreateTaskDto, TaskItem>();
        CreateMap<UpdateTaskDto, TaskItem>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Note, NoteDetailDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                src.CreatedBy != null ? $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}" : string.Empty));
        CreateMap<Note, NoteListDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                src.CreatedBy != null ? $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}" : string.Empty));
        CreateMap<CreateNoteDto, Note>();
        CreateMap<UpdateNoteDto, Note>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<CallLog, CallLogDetailDto>()
            .ForMember(dest => dest.FormattedDuration, opt => opt.MapFrom(src => $"{src.DurationInSeconds}s"))
            .ForMember(dest => dest.CallReceivedByName, opt => opt.MapFrom(src =>
                src.CallReceivedBy != null ? $"{src.CallReceivedBy.FirstName} {src.CallReceivedBy.LastName}" : string.Empty))
            .ForMember(dest => dest.CallerUserName, opt => opt.MapFrom(src =>
                src.CallerUser != null ? $"{src.CallerUser.FirstName} {src.CallerUser.LastName}" : string.Empty));
        CreateMap<CallLog, CallLogListDto>()
            .ForMember(dest => dest.FormattedDuration, opt => opt.MapFrom(src => $"{src.DurationInSeconds}s"));
        CreateMap<CreateCallLogDto, CallLog>();

        CreateMap<Contact, ContactDetailDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src =>
                src.Organization != null ? src.Organization.OrganizationName : string.Empty))
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src =>
                src.AssignedUser != null ? $"{src.AssignedUser.FirstName} {src.AssignedUser.LastName}" : string.Empty));
        CreateMap<Contact, ContactListDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src =>
                src.Organization != null ? src.Organization.OrganizationName : string.Empty))
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src =>
                src.AssignedUser != null ? $"{src.AssignedUser.FirstName} {src.AssignedUser.LastName}" : string.Empty));
        CreateMap<CreateContactDto, Contact>();
        CreateMap<UpdateContactDto, Contact>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Organization, OrganizationDetailDto>()
            .ForMember(dest => dest.TerritoryName, opt => opt.MapFrom(src =>
                src.Territory != null ? src.Territory.TerritoryName : string.Empty));
        CreateMap<Organization, OrganizationListDto>()
            .ForMember(dest => dest.TerritoryName, opt => opt.MapFrom(src =>
                src.Territory != null ? src.Territory.TerritoryName : string.Empty));
        CreateMap<CreateOrganizationDto, Organization>();
        CreateMap<UpdateOrganizationDto, Organization>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Lead, LeadDetailDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IndustryName, opt => opt.MapFrom(src => src.Industry.HasValue ? src.Industry.Value.ToString() : null))
            .ForMember(dest => dest.TerritoryName, opt => opt.MapFrom(src =>
                src.Territory != null ? src.Territory.TerritoryName : string.Empty))
            .ForMember(dest => dest.LeadOwnerName, opt => opt.MapFrom(src =>
                src.LeadOwner != null ? $"{src.LeadOwner.FirstName} {src.LeadOwner.LastName}" : string.Empty));
        CreateMap<Lead, LeadListDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.LeadOwnerName, opt => opt.MapFrom(src =>
                src.LeadOwner != null ? $"{src.LeadOwner.FirstName} {src.LeadOwner.LastName}" : string.Empty));
        CreateMap<CreateLeadDto, Lead>();
        CreateMap<UpdateLeadDto, Lead>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Deal, DealDetailDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IndustryName, opt => opt.MapFrom(src => src.Industry.HasValue ? src.Industry.Value.ToString() : null))
            .ForMember(dest => dest.TerritoryName, opt => opt.MapFrom(src =>
                src.Territory != null ? src.Territory.TerritoryName : string.Empty))
            .ForMember(dest => dest.DealOwnerName, opt => opt.MapFrom(src =>
                src.DealOwner != null ? $"{src.DealOwner.FirstName} {src.DealOwner.LastName}" : string.Empty))
            .ForMember(dest => dest.DetailOrganizationName, opt => opt.MapFrom(src =>
                src.Organization != null ? src.Organization.OrganizationName : src.OrganizationName))
            .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src =>
                src.Contact != null ? $"{src.Contact.FirstName} {src.Contact.LastName}" : string.Empty));
        CreateMap<Deal, DealListDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DealOwnerName, opt => opt.MapFrom(src =>
                src.DealOwner != null ? $"{src.DealOwner.FirstName} {src.DealOwner.LastName}" : string.Empty));
        CreateMap<CreateDealDto, Deal>();
        CreateMap<UpdateDealDto, Deal>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Product Mappings
        CreateMap<Domain.Entity.Product, Contract.DTOs.Product.ProductDetailDto>().ReverseMap();
        CreateMap<Contract.DTOs.Product.CreateProductDto, Domain.Entity.Product>();
        CreateMap<Contract.DTOs.Product.UpdateProductDto, Domain.Entity.Product>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // EmailTemplate Mappings
        CreateMap<Domain.Entity.EmailTemplate, Contract.DTOs.EmailTemplate.EmailTemplateDetailDto>().ReverseMap();
        CreateMap<Contract.DTOs.EmailTemplate.CreateEmailTemplateDto, Domain.Entity.EmailTemplate>();
        CreateMap<Contract.DTOs.EmailTemplate.UpdateEmailTemplateDto, Domain.Entity.EmailTemplate>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // EmailLog Mappings
        CreateMap<Domain.Entity.EmailLog, Contract.DTOs.Email.EmailLogDetailDto>().ReverseMap();
    }
}
