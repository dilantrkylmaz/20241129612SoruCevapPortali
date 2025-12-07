using System.Security.Cryptography;
using System.Text;

namespace _20241129612SoruCevapPortalı.Helpers
{
    public static class SecurityHelper
    {
        
        private static readonly string _salt = "VizeProjesi_Tuzlu_2025!*";

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return "";

            
            string saltedPassword = password + _salt;

           
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(saltedPassword);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

               
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}