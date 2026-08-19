using System.IO;
using BestHTTP.Extensions;

namespace BestHTTP.Forms;

public sealed class HTTPMultiPartForm : HTTPFormBase
{
	private string Boundary;

	private byte[] CachedData;

	public HTTPMultiPartForm()
	{
		Boundary = GetHashCode().ToString("X");
	}

	public override void PrepareRequest(HTTPRequest request)
	{
		request.SetHeader("Content-Type", "multipart/form-data; boundary=\"" + Boundary + "\"");
	}

	public override byte[] GetData()
	{
		if (CachedData != null)
		{
			return CachedData;
		}
		using MemoryStream memoryStream = new MemoryStream();
		for (int i = 0; i < base.Fields.Count; i++)
		{
			HTTPFieldData hTTPFieldData = base.Fields[i];
			memoryStream.WriteLine("--" + Boundary);
			memoryStream.WriteLine("Content-Disposition: form-data; name=\"" + hTTPFieldData.Name + "\"" + (string.IsNullOrEmpty(hTTPFieldData.FileName) ? string.Empty : ("; filename=\"" + hTTPFieldData.FileName + "\"")));
			if (!string.IsNullOrEmpty(hTTPFieldData.MimeType))
			{
				memoryStream.WriteLine("Content-Type: " + hTTPFieldData.MimeType);
			}
			memoryStream.WriteLine("Content-Length: " + hTTPFieldData.Payload.Length);
			memoryStream.WriteLine();
			memoryStream.Write(hTTPFieldData.Payload, 0, hTTPFieldData.Payload.Length);
			memoryStream.Write(HTTPRequest.EOL, 0, HTTPRequest.EOL.Length);
		}
		memoryStream.WriteLine("--" + Boundary + "--");
		base.IsChanged = false;
		return CachedData = memoryStream.ToArray();
	}
}
