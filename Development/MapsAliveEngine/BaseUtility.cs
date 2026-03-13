// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Security.Cryptography;
using System.Text;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseUtility
	{
		#region ===== Properties ========================================================
		#endregion

		#region ===== Public ============================================================

		static public string Hash(string data)
		{
			return Hash(Encoding.Unicode.GetBytes(data.ToCharArray()));
		}

		static public string Hash(byte[] data)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] result = md5.ComputeHash(data);

			// Build the final string by converting each byte
			// into hex and appending it to a StringBuilder
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < result.Length; i++)
				sb.Append(result[i].ToString("X2"));

			return sb.ToString();
		}

		public static bool SameString(string s1, string s2)
		{
			return string.Compare(s1, s2, true) == 0;
		}
		#endregion

		#region ===== Protected =========================================================
		#endregion

		#region ===== Private ===========================================================
		#endregion
	}
}
