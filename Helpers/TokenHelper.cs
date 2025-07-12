using System;

namespace YazlabBirSonProje.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }
        public static bool IsTokenExpired(DateTime expiration)
        {
            return DateTime.Now > expiration;
        }
    }
}
