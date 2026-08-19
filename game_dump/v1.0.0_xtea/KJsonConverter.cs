using System;
using System.Collections.Generic;
using System.IO;
using L10N;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class KJsonConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if ((object)objectType == typeof(Color))
		{
			return ReadColor(reader, objectType, existingValue, serializer);
		}
		if ((object)objectType == typeof(Gettext))
		{
			return ReadGetText(reader, objectType, existingValue, serializer);
		}
		if ((object)objectType == typeof(KeyValuePair<string, string>))
		{
			return ReadKeyValuePair<string>(reader, objectType, existingValue, serializer);
		}
		if ((object)objectType == typeof(KeyValuePair<int, int>))
		{
			return ReadKeyValuePair<int>(reader, objectType, existingValue, serializer);
		}
		if ((object)objectType == typeof(KeyValuePair<double, double>))
		{
			return ReadKeyValuePair<double>(reader, objectType, existingValue, serializer);
		}
		if ((object)objectType == typeof(KeyValuePair<float, float>))
		{
			return ReadKeyValuePair<float>(reader, objectType, existingValue, serializer);
		}
		return existingValue;
	}

	public override bool CanConvert(Type objectType)
	{
		if ((object)objectType == typeof(Color))
		{
			return true;
		}
		if ((object)objectType == typeof(Enum))
		{
			return true;
		}
		if ((object)objectType == typeof(Gettext))
		{
			return true;
		}
		if ((object)objectType == typeof(KeyValuePair<string, string>))
		{
			return true;
		}
		if ((object)objectType == typeof(KeyValuePair<int, int>))
		{
			return true;
		}
		if ((object)objectType == typeof(KeyValuePair<double, double>))
		{
			return true;
		}
		if ((object)objectType == typeof(KeyValuePair<float, float>))
		{
			return true;
		}
		return false;
	}

	private object ReadColor(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		switch (reader.TokenType)
		{
		case JsonToken.Integer:
		{
			int num = (int)reader.Value;
			byte b = (byte)((uint)num & 0xFFu);
			byte b2 = (byte)((uint)(num >> 8) & 0xFFu);
			byte b3 = (byte)((uint)(num >> 16) & 0xFFu);
			return Color32.op_Implicit(new Color32(b3, b2, b, byte.MaxValue));
		}
		case JsonToken.String:
			return NGUIText.ParseColor((string)reader.Value);
		default:
			return (object)default(Color);
		}
	}

	private object ReadGetText(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		switch (reader.TokenType)
		{
		case JsonToken.StartObject:
		{
			JObject token = serializer.Deserialize<JObject>(reader);
			return new Gettext(ParseJObject(token));
		}
		case JsonToken.String:
			return new Gettext((string)reader.Value);
		default:
			throw new IOException($"Gettext type expects Map or String but got {reader.TokenType}");
		}
	}

	private string ParseJObject(JToken token)
	{
		if (!(token is JObject jObject))
		{
			return string.Empty;
		}
		IEnumerator<KeyValuePair<string, JToken>> enumerator = jObject.GetEnumerator();
		if (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			if (!(enumerator.Current.Value is JObject jObject2))
			{
				return T._(key);
			}
			enumerator = jObject2.GetEnumerator();
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JToken> current = enumerator.Current;
				dictionary.Add(current.Key, ParseJToken(current.Value));
			}
			return T._(key, dictionary);
		}
		return string.Empty;
	}

	private object ParseJArray(JToken token)
	{
		if (!(token is JArray jArray))
		{
			return null;
		}
		object[] array = new object[jArray.Count];
		for (int i = 0; i < jArray.Count; i++)
		{
			array[i] = ParseJToken(jArray[i]);
		}
		return array;
	}

	private object ParseJToken(JToken token)
	{
		return token.Type switch
		{
			JTokenType.Object => ParseJObject(token), 
			JTokenType.Array => ParseJArray(token), 
			JTokenType.Integer => token.Value<long>(), 
			JTokenType.Float => token.Value<double>(), 
			JTokenType.String => token.Value<string>(), 
			JTokenType.Boolean => token.Value<bool>(), 
			_ => null, 
		};
	}

	private object ReadKeyValuePair<T>(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType != JsonToken.StartArray)
		{
			throw new IOException($"KeyValuePair type expects Array but got {reader.TokenType}");
		}
		T[] array = serializer.Deserialize<T[]>(reader);
		return new KeyValuePair<T, T>(array[0], array[1]);
	}
}
