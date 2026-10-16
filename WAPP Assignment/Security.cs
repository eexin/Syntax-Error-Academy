using System.Security.Cryptography;
using System.Text;

// Hashes passwords so plain text is never stored. Login and Sign-up both call this,
// so the same password always produces the same hash to compare against.
//
// Web Site project: place in App_Code. Web Application: include anywhere in project.
//
// Note: SHA-256 is used here for course simplicity. A production app should use a
// salted, slow hash (e.g. PBKDF2 / Rfc2898DeriveBytes or bcrypt).
public static class Security
{
    public static string Hash(string password)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password ?? ""));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
