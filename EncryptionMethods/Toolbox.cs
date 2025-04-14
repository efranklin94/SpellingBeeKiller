using System.Security.Cryptography;
using System.Text;

namespace EncryptionMethods;

public class Toolbox
{
    // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
    // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
    // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
    private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89gejo340t89u2");

    // This constant is used to determine the keysize of the encryption algorithm.
    private const int keysize = 256;

    public static string RandomString(int length)
    {
        string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
        }

        if (string.IsNullOrEmpty(allowedChars))
        {
            throw new ArgumentException("allowedChars may not be empty.");
        }

        const int byteSize = 0x100;
        var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
        if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

        // Guid.NewGuid and System.Random are not particularly random. By using a
        // cryptographically-secure random number generator, the caller is always
        // protected, regardless of use.
        using (var rng = new RNGCryptoServiceProvider())
        {
            var result = new StringBuilder();
            var buf = new byte[128];
            while (result.Length < length)
            {
                rng.GetBytes(buf);
                for (var i = 0; i < buf.Length && result.Length < length; ++i)
                {
                    // Divide the byte into allowedCharSet-sized groups. If the
                    // random value falls into the last group and the last group is
                    // too small to choose from the entire allowedCharSet, ignore
                    // the value in order to avoid biasing the result.
                    var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                    if (outOfRangeStart <= buf[i]) continue;
                    result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                }
            }
            return result.ToString();
        }
    }

    public static string ComputeSha256Hash(string rawData)
    {
        // Create a SHA256   
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // ComputeHash - returns byte array  
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public static string Encrypt(string plainText, string passPhrase)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
        {
            byte[] keyBytes = password.GetBytes(keysize / 8);
            using (var aes = new AesManaged())
            {
                aes.IV = initVectorBytes;
                aes.Key = keyBytes;
                aes.Mode = CipherMode.CBC;
                var cipherBytes = aes.CreateEncryptor().TransformFinalBlock(plainTextBytes,
                    0, plainTextBytes.Length);

                return Convert.ToBase64String(cipherBytes);
            }
        }
    }

    public static string Decrypt(string cipherText, string passPhrase)
    {
        byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
        using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
        {
            byte[] keyBytes = password.GetBytes(keysize / 8);
            using (var aes = new AesManaged())
            {
                aes.IV = initVectorBytes;
                aes.Mode = CipherMode.CBC;
                aes.Key = keyBytes;
                var plainTextBytes = aes.CreateDecryptor().TransformFinalBlock(cipherTextBytes,
                    0, cipherTextBytes.Length);
                return Encoding.UTF8.GetString(plainTextBytes);
            }
        }
    }

    public static string Encrypt(string plainText, string passPhrase, int saltCharacters)
    {
        return $"{Encrypt(plainText, passPhrase)}{RandomString(saltCharacters)}";
    }

    public static string Decrypt(string cipherText, string passPhrase, int saltCharacters)
    {
        return Decrypt(cipherText.Substring(0, cipherText.Length - saltCharacters), passPhrase);
    }
}