using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    public static class PasswordService
    {
        #region Public Methods

        public static string GetRandomPwdString(PasswordRequirement request)
        {
            var pwdCharacter = new StringBuilder("abcdefghijklmnopqrstuvwxyz");

            if (request.ContainUppercase)
                pwdCharacter.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            if (request.ContainNumber)
                pwdCharacter.Append("0123456789");

            if (request.ContainSpecialCharacter)
                pwdCharacter.Append("~!@#$%^*()_+{}|:<>?-=[]\\;'./");

            return GetRandomString(request.PasswordLength, pwdCharacter.ToString());
        }

        public static string GetRandomPassCodeString(int minLength)
        {
            var pwdCharacter = "0123456789";

            return GetRandomString((minLength < 4 ? 4 : minLength), pwdCharacter);
        }

        #endregion


        #region Private Methods

        private static string GetRandomString(int length, IEnumerable<char> characterSet)
        {
            if (length < 0)
                throw new ArgumentException("length must not be negative", nameof(length));
            if (length > int.MaxValue / 8) // 250 million chars ought to be enough for anybody
                throw new ArgumentException("length is too big", nameof(length));
            ArgumentNullException.ThrowIfNull(characterSet);
            var characterArray = characterSet.Distinct().ToArray();
            if (characterArray.Length == 0)
                throw new ArgumentException("characterSet must not be empty", nameof(characterSet));

            var bytes = new byte[length * 8];
            RandomNumberGenerator.Create().GetBytes(bytes);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                ulong value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = characterArray[value % (uint)characterArray.Length];
            }
            return new string(result);
        }

        #endregion
        public class PasswordRequirement
        {
            public int PasswordLength { get; set; }
            public bool ContainNumber { get; set; }
            public bool ContainUppercase { get; set; }
            public bool ContainSpecialCharacter { get; set; }
        }
    }
}