using System;
using System.Collections.Generic;
using System.Text;

namespace NGettext.Loaders;

public class MoFile
{
	public Version FormatRevision { get; protected set; }

	public bool BigEndian { get; protected set; }

	public Encoding Encoding { get; set; }

	public Dictionary<string, string> Headers { get; protected set; }

	public Dictionary<string, string[]> Translations { get; protected set; }

	public MoFile(Version formatRevision, Encoding encoding = null, bool bigEndian = false)
	{
		FormatRevision = formatRevision;
		BigEndian = bigEndian;
		Encoding = encoding;
		Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Translations = new Dictionary<string, string[]>();
	}
}
