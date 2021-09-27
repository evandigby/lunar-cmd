namespace api.Auth
{
    public static class StandardUsers
    {
        public static readonly string[] Contributor = new[] { AuthClaims.ReadLogEntires, AuthClaims.WriteLogEntires };
    }
}
