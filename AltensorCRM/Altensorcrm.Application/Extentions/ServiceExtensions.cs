using System;
using Altensorcrm.Application.Services.CallLog;
using Altensorcrm.Application.Services.Contact;
using Altensorcrm.Application.Services.CustomView;
using Altensorcrm.Application.Services.Dashboard;
using Altensorcrm.Application.Services.Deal;
using Altensorcrm.Application.Services.Layout;
using Altensorcrm.Application.Services.Lead;
using Altensorcrm.Application.Services.Note;
using Altensorcrm.Application.Services.Organization;
using Altensorcrm.Application.Services.Setting;
using Altensorcrm.Application.Services.Task;
using Altensorcrm.Application.Services.UserManagement;
using Altensorcrm.Contract.Services.CallLog;
using Altensorcrm.Contract.Services.Contact;
using Altensorcrm.Contract.Services.CustomView;
using Altensorcrm.Contract.Services.Dashboard;
using Altensorcrm.Contract.Services.Deal;
using Altensorcrm.Contract.Services.Email;
using Altensorcrm.Contract.Services.EmailTemplate;
using Altensorcrm.Contract.Services.Layout;
using Altensorcrm.Contract.Services.Lead;
using Altensorcrm.Contract.Services.Note;
using Altensorcrm.Contract.Services.Organization;
using Altensorcrm.Contract.Services.Product;
using Altensorcrm.Contract.Services.Setting;
using Altensorcrm.Contract.Services.Task;
using Altensorcrm.Contract.Services.UserManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Altensorcrm.Application.Extentions
{
    public static class ServiceExtensions 
    {
        public static IServiceCollection AddServiceRegistration (this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddScoped<IUserSyncService, UserSyncService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILeadService, LeadService>();
            services.AddScoped<IDealService, DealService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<ICallLogService, CallLogService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ILayoutService, LayoutService>();
            services.AddScoped<ICustomViewService, CustomViewService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IEmailService, Altensorcrm.Application.Services.Email.EmailService>();
            services.AddScoped<IProductService, Altensorcrm.Application.Services.Product.ProductService>();
            services.AddScoped<IEmailTemplateService, Altensorcrm.Application.Services.EmailTemplate.EmailTemplateService>();

            return services;
        }
    }
}
