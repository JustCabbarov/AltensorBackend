using System;

namespace Altensorcrm.Contract.DTOs.CustomView;

public class CustomViewDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ViewName { get; set; } = string.Empty;
    public string ViewType { get; set; } = "List";
    public string ConfigJson { get; set; } = "{}";
}

public class CreateCustomViewDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string ViewName { get; set; } = string.Empty;
    public string ViewType { get; set; } = "List";
    public string ConfigJson { get; set; } = "{}";
}

