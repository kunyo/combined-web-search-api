namespace Bds.TechTest.Configuration
{
    public interface IApplicationSettings
    {
        string TokenAudience { get; }
        string TokenIssuer { get; }
        byte[] TokenSigningKey { get; }
    }
}