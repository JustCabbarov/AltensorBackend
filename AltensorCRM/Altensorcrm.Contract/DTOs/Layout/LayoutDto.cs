namespace Altensorcrm.Contract.DTOs.Layout;

public class LayoutDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string LayoutJson { get; set; } = string.Empty;
}

public class UpdateLayoutDto
{
    public string LayoutJson { get; set; } = string.Empty;
}
