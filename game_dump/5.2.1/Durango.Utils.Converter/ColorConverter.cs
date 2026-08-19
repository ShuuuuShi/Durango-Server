using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Durango.Utils.Converter;

public class ColorConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is Color c)
		{
			writer.WriteValue(NGUIText.EncodeColor(c));
		}
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		switch (reader.TokenType)
		{
		case JsonToken.Integer:
		{
			int num = (int)reader.Value;
			byte b = (byte)((uint)num & 0xFFu);
			byte g = (byte)((uint)(num >> 8) & 0xFFu);
			return (Color)new Color32((byte)((uint)(num >> 16) & 0xFFu), g, b, byte.MaxValue);
		}
		case JsonToken.String:
			return NGUIText.ParseColor((string)reader.Value);
		default:
			return default(Color);
		}
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Color);
	}
}
