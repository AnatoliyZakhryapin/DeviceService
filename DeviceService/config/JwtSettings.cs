namespace DeviceService.config
{
    public class JwtSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
        public int TokenExpiryInMinutes { get; set; }
    }
}
