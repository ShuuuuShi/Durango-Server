using System;
using System.IO;
using System.Text;

namespace NGettext.Loaders;

public class MoFileParser
{
	private struct StringOffsetTable
	{
		public int Length;

		public int Offset;
	}

	private const uint MO_FILE_MAGIC = 2500072158u;

	private const ushort MAX_SUPPORTED_VERSION = 1;

	public Encoding DefaultEncoding { get; set; }

	public bool AutoDetectEncoding { get; set; }

	public MoFileParser()
	{
		DefaultEncoding = Encoding.UTF8;
		AutoDetectEncoding = true;
	}

	public MoFileParser(Encoding defaultEncoding, bool autoDetectEncoding = true)
	{
		DefaultEncoding = defaultEncoding;
		AutoDetectEncoding = autoDetectEncoding;
	}

	public MoFile Parse(Stream stream)
	{
		if (stream == null || stream.Length < 20)
		{
			throw new ArgumentException("Stream can not be null of less than 20 bytes long.");
		}
		bool bigEndian = false;
		BinaryReader binaryReader = new BinaryReader(new ReadOnlyStreamWrapper(stream));
		try
		{
			uint num = binaryReader.ReadUInt32();
			if (num != 2500072158u)
			{
				if (_ReverseBytes(num) != 2500072158u)
				{
					throw new ArgumentException("Invalid stream: can not find MO file magic number.");
				}
				bigEndian = true;
				((IDisposable)binaryReader).Dispose();
				binaryReader = new BigEndianBinaryReader(new ReadOnlyStreamWrapper(stream));
			}
			int num2 = binaryReader.ReadInt32();
			MoFile moFile = new MoFile(new Version(num2 >> 16, num2 & 0xFFFF), DefaultEncoding, bigEndian);
			if (moFile.FormatRevision.Major > 1)
			{
				throw new CatalogLoadingException($"Unsupported MO file major revision: {moFile.FormatRevision.Major}.");
			}
			int num3 = binaryReader.ReadInt32();
			int num4 = binaryReader.ReadInt32();
			int num5 = binaryReader.ReadInt32();
			StringOffsetTable[] array = new StringOffsetTable[num3];
			StringOffsetTable[] array2 = new StringOffsetTable[num3];
			binaryReader.BaseStream.Seek(num4, SeekOrigin.Begin);
			for (int i = 0; i < num3; i++)
			{
				array[i].Length = binaryReader.ReadInt32();
				array[i].Offset = binaryReader.ReadInt32();
			}
			binaryReader.BaseStream.Seek(num5, SeekOrigin.Begin);
			for (int j = 0; j < num3; j++)
			{
				array2[j].Length = binaryReader.ReadInt32();
				array2[j].Offset = binaryReader.ReadInt32();
			}
			for (int k = 0; k < num3; k++)
			{
				string[] array3 = _ReadStrings(binaryReader, array[k].Offset, array[k].Length, moFile.Encoding);
				string[] array4 = _ReadStrings(binaryReader, array2[k].Offset, array2[k].Length, moFile.Encoding);
				if (array3.Length == 0 || array4.Length == 0)
				{
					continue;
				}
				if (array3[0].Length == 0)
				{
					string[] array5 = array4[0].Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string text in array5)
					{
						int num6 = text.IndexOf(':');
						if (num6 > 0)
						{
							string key = text.Substring(0, num6);
							string text2 = text.Substring(num6 + 1).Trim();
							moFile.Headers.Add(key, text2.Trim());
						}
					}
					if (AutoDetectEncoding && moFile.Headers.ContainsKey("Content-Type"))
					{
						try
						{
							ContentType contentType = new ContentType(moFile.Headers["Content-Type"]);
							if (!string.IsNullOrEmpty(contentType.CharSet))
							{
								moFile.Encoding = Encoding.GetEncoding(contentType.CharSet);
							}
						}
						catch (Exception ex)
						{
							throw new CatalogLoadingException($"Unable to change parser encoding using the Content-Type header: \"{ex.Message}\".", ex);
						}
					}
				}
				moFile.Translations.Add(array3[0], array4);
			}
			return moFile;
		}
		finally
		{
			((IDisposable)binaryReader).Dispose();
		}
	}

	private string[] _ReadStrings(BinaryReader reader, int offset, int length, Encoding encoding)
	{
		reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		byte[] array = reader.ReadBytes(length);
		return encoding.GetString(array, 0, array.Length).Split(default(char));
	}

	private static uint _ReverseBytes(uint value)
	{
		return ((value & 0xFF) << 24) | ((value & 0xFF00) << 8) | ((value & 0xFF0000) >> 8) | ((value & 0xFF000000u) >> 24);
	}
}
