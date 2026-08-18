namespace Altensorcrm.Contract.DTOs.Setting;

public class SystemSettingDto
{
    public string Theme { get; set; } = "system";
    public string Language { get; set; } = "en";
    public string Timezone { get; set; } = "Asia/Baku";

    public bool UpdateTimestamp { get; set; } = true;
    public bool MarkRepliedOnResponse { get; set; } = false;
    public bool ReopenOnCommunication { get; set; } = false;
    public string TimelineFormat { get; set; } = "Relative";
    public string TimelineSort { get; set; } = "Oldest First";

    public bool EnableForecasting { get; set; } = false;
    public bool AutoUpdateDealValue { get; set; } = true;
    public string DashboardCurrency { get; set; } = "INR";
    public string ExchangeProvider { get; set; } = "Frankfurter";

    public string Currency { get; set; } = "USD";
    public string CurrencyPrecision { get; set; } = "3";
    public string NumberFormat { get; set; } = "#,###.##";
    public string FloatPrecision { get; set; } = "3";
    public string DateFormat { get; set; } = "dd-mm-yyyy";
    public string TimeFormat { get; set; } = "HH:mm:ss";

    public string BrandName { get; set; } = "Altensor CRM";
    public string? BrandLogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
}
