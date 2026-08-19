using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Bson;

namespace Newtonsoft.Json.Converters;

public class RegexConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Regex regex = (Regex)value;
		if (writer is BsonWriter writer2)
		{
			WriteBson(writer2, regex);
		}
		else
		{
			WriteJson(writer, regex);
		}
	}

	private bool HasFlag(RegexOptions options, RegexOptions flag)
	{
		return (options & flag) == flag;
	}

	private void WriteBson(BsonWriter writer, Regex regex)
	{
		string text = null;
		if (HasFlag(regex.Options, RegexOptions.IgnoreCase))
		{
			text += "i";
		}
		if (HasFlag(regex.Options, RegexOptions.Multiline))
		{
			text += "m";
		}
		if (HasFlag(regex.Options, RegexOptions.Singleline))
		{
			text += "s";
		}
		text += "u";
		if (HasFlag(regex.Options, RegexOptions.ExplicitCapture))
		{
			text += "x";
		}
		writer.WriteRegex(regex.ToString(), text);
	}

	private void WriteJson(JsonWriter writer, Regex regex)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("Pattern");
		writer.WriteValue(regex.ToString());
		writer.WritePropertyName("Options");
		writer.WriteValue(regex.Options);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader is BsonReader reader2)
		{
			return ReadBson(reader2);
		}
		return ReadJson(reader);
	}

	private object ReadBson(BsonReader reader)
	{
		string text = (string)reader.Value;
		int num = text.LastIndexOf("/");
		string pattern = text.Substring(1, num - 1);
		string text2 = text.Substring(num + 1);
		RegexOptions regexOptions = RegexOptions.None;
		string text3 = text2;
		for (int i = 0; i < text3.Length; i++)
		{
			switch (text3[i])
			{
			case 'i':
				regexOptions |= RegexOptions.IgnoreCase;
				break;
			case 'm':
				regexOptions |= RegexOptions.Multiline;
				break;
			case 's':
				regexOptions |= RegexOptions.Singleline;
				break;
			case 'x':
				regexOptions |= RegexOptions.ExplicitCapture;
				break;
			}
		}
		return new Regex(pattern, regexOptions);
	}

	private Regex ReadJson(JsonReader reader)
	{
		reader.Read();
		reader.Read();
		string pattern = (string)reader.Value;
		reader.Read();
		reader.Read();
		int options = Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture);
		reader.Read();
		return new Regex(pattern, (RegexOptions)options);
	}

	public override bool CanConvert(Type objectType)
	{
		return (object)objectType == typeof(Regex);
	}
}
