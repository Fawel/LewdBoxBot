using System;

namespace ImageDanbooruPuller
{
    public class DanbooruAuthenticationSettings
    {
        public readonly string Login;
        public readonly string ApiKey;

        public DanbooruAuthenticationSettings(string login, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentException("Необходим логин для данбору");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Необходим ключ апи для профиля данбору");
            }

            Login = login;
            ApiKey = apiKey;
        }
    }
}
