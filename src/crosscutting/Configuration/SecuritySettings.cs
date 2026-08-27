namespace Crosscutting.Configuration;

public class SecuritySettings
{
    public string ApiKey { get; set; } = "BTG_PROTOTYPING_SECRET_KEY_12345";
    public bool EnableApiKeyAuth { get; set; } = true;
}
