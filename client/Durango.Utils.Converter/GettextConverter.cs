using System;
using System.Collections.Generic;
using System.IO;
using L10N;
using Newtonsoft.Json;

namespace Durango.Utils.Converter;

public class GettextConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is Gettext gettext)
		{
			writer.WriteValue(gettext.ToString());
		}
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		return reader.TokenType switch
		{
			JsonToken.StartObject => new Gettext(ParseObject(reader, serializer)), 
			JsonToken.String => new Gettext((string)reader.Value), 
			_ => throw new IOException($"Gettext type expects Map or String but got {reader.TokenType}"), 
		};
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Gettext);
	}

	private static string ParseObject(JsonReader reader, JsonSerializer serializer)
	{
		reader.Read();
		string msgid = reader.Value as string;
		reader.Read();
		string result;
		if (reader.TokenType == JsonToken.StartObject)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			while (reader.Read() && reader.TokenType != JsonToken.EndObject)
			{
				string key = reader.Value as string;
				reader.Read();
				object value = ParseValue(reader, serializer);
				dictionary.Add(key, value);
			}
			result = T.ParseMsgIdAndGetString(msgid, dictionary);
		}
		else
		{
			result = T.ParseMsgIdAndGetString(msgid);
		}
		reader.Read();
		return result;
	}

	private static object ParseArray(JsonReader reader, JsonSerializer serializer)
	{
		List<object> list = new List<object>();
		while (reader.Read() && reader.TokenType != JsonToken.EndArray)
		{
			object item = ParseValue(reader, serializer);
			list.Add(item);
		}
		return list;
	}

	private static object ParseValue(JsonReader reader, JsonSerializer serializer)
	{
		return reader.TokenType switch
		{
			JsonToken.StartObject => ParseObject(reader, serializer), 
			JsonToken.StartArray => ParseArray(reader, serializer), 
			_ => reader.Value, 
		};
	}
}
