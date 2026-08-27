namespace Entities.Constants;

public static class AppConstants
{
    public const string ApiKeyHeaderName = "X-Api-Key";
    public const string HealthCheckEndpoint = "/api/health";
    public const int DefaultCacheExpirationMinutes = 10;
    public const string MongoDatabaseName = "btg_prototyping_db";
}
