using System;
using System.Text;

namespace BestHTTP.Forms;

public sealed class HTTPUrlEncodedForm : HTTPFormBase
{
	private byte[] CachedData;

	public override void PrepareRequest(HTTPRequest request)
	{
		request.SetHeader("Content-Type", "application/x-www-form-urlencoded");
	}

	public override byte[] GetData()
	{
		if (CachedData != null && !base.IsChanged)
		{
			return CachedData;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < base.Fields.Count; i++)
		{
			HTTPFieldData hTTPFieldData = base.Fields[i];
			if (i > 0)
			{
				stringBuilder.Append("&");
			}
			stringBuilder.Append(Uri.EscapeDataString(hTTPFieldData.Name));
			stringBuilder.Append("=");
			if (!string.IsNullOrEmpty(hTTPFieldData.Text) || hTTPFieldData.Binary == null)
			{
				stringBuilder.Append(Uri.EscapeDataString(hTTPFieldData.Text));
			}
			else
			{
				stringBuilder.Append(Uri.EscapeDataString(Encoding.UTF8.GetString(hTTPFieldData.Binary, 0, hTTPFieldData.Binary.Length)));
			}
		}
		base.IsChanged = false;
		return CachedData = Encoding.UTF8.GetBytes(stringBuilder.ToString());
	}
}
