using System.Security.Cryptography;
using System.Text;

namespace Nusharp.Core.Utils
{
	public class SHAHelper{
		public static string CalculateSHA512Hash(string input)
		{
			// step 1, calculate MD5 hash from input
			SHA512 sha = SHA512.Create();
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
			byte[] hash = sha.ComputeHash(inputBytes);

			// step 2, convert byte array to hex string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}
