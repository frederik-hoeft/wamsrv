using System.Text.RegularExpressions;

namespace wamsrv.Email
{
    public static class EmailEssentials
    {
        private const string emailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

        public static bool IsValid(string email)
        {
            return Regex.IsMatch(email, emailRegex);
        }
    }
}