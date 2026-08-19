using System;
using System.Collections.Generic;
using System.Text;

namespace BestHTTP.Forms;

public class HTTPFormBase
{
	private const int LongLength = 256;

	public List<HTTPFieldData> Fields { get; set; }

	public bool IsEmpty => Fields == null || Fields.Count == 0;

	public bool IsChanged { get; protected set; }

	public bool HasBinary { get; protected set; }

	public bool HasLongValue { get; protected set; }

	public void AddBinaryData(string fieldName, byte[] content)
	{
		AddBinaryData(fieldName, content, null, null);
	}

	public void AddBinaryData(string fieldName, byte[] content, string fileName)
	{
		AddBinaryData(fieldName, content, fileName, null);
	}

	public void AddBinaryData(string fieldName, byte[] content, string fileName, string mimeType)
	{
		if (Fields == null)
		{
			Fields = new List<HTTPFieldData>();
		}
		HTTPFieldData hTTPFieldData = new HTTPFieldData();
		hTTPFieldData.Name = fieldName;
		if (fileName == null)
		{
			hTTPFieldData.FileName = fieldName + ".dat";
		}
		else
		{
			hTTPFieldData.FileName = fileName;
		}
		if (mimeType == null)
		{
			hTTPFieldData.MimeType = "application/octet-stream";
		}
		else
		{
			hTTPFieldData.MimeType = mimeType;
		}
		hTTPFieldData.Binary = content;
		Fields.Add(hTTPFieldData);
		bool hasBinary = (IsChanged = true);
		HasBinary = hasBinary;
	}

	public void AddField(string fieldName, string value)
	{
		AddField(fieldName, value, Encoding.UTF8);
	}

	public void AddField(string fieldName, string value, Encoding e)
	{
		if (Fields == null)
		{
			Fields = new List<HTTPFieldData>();
		}
		HTTPFieldData hTTPFieldData = new HTTPFieldData();
		hTTPFieldData.Name = fieldName;
		hTTPFieldData.FileName = null;
		hTTPFieldData.MimeType = "text/plain; charset=\"" + e.WebName + "\"";
		hTTPFieldData.Text = value;
		hTTPFieldData.Encoding = e;
		Fields.Add(hTTPFieldData);
		IsChanged = true;
		HasLongValue |= value.Length > 256;
	}

	public virtual void CopyFrom(HTTPFormBase fields)
	{
		Fields = new List<HTTPFieldData>(fields.Fields);
		IsChanged = true;
		HasBinary = fields.HasBinary;
		HasLongValue = fields.HasLongValue;
	}

	public virtual void PrepareRequest(HTTPRequest request)
	{
		throw new NotImplementedException();
	}

	public virtual byte[] GetData()
	{
		throw new NotImplementedException();
	}
}
