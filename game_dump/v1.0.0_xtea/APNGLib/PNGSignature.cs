using System;

namespace APNGLib;

internal static class PNGSignature
{
	public static byte[] Signature = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

	public static void Compare(byte[] sig)
	{
		if (sig.Length == Signature.Length)
		{
			for (int i = 0; i < Signature.Length; i++)
			{
				if (Signature[i] != sig[i])
				{
					throw new ApplicationException("APNG signature not found.");
				}
			}
			return;
		}
		throw new ApplicationException("APNG signature not found.");
	}
}
