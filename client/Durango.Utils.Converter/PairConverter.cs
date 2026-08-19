using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;

namespace Durango.Utils.Converter;

public class PairConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Type type = value.GetType();
		PropertyInfo property = type.GetProperty("Item1");
		PropertyInfo property2 = type.GetProperty("Item2");
		writer.WriteStartArray();
		serializer.Serialize(writer, ReflectionUtils.GetMemberValue(property, value));
		serializer.Serialize(writer, ReflectionUtils.GetMemberValue(property2, value));
		writer.WriteEndArray();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		Type underlyingType = Nullable.GetUnderlyingType(objectType);
		if (underlyingType != null)
		{
			objectType = underlyingType;
		}
		IList<Type> genericArguments = objectType.GetGenericArguments();
		Type objectType2 = genericArguments[0];
		Type objectType3 = genericArguments[1];
		reader.Read();
		object obj = serializer.Deserialize(reader, objectType2);
		reader.Read();
		object obj2 = serializer.Deserialize(reader, objectType3);
		reader.Read();
		return ReflectionUtils.CreateInstance(objectType, obj, obj2);
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType.IsValueType && objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Pair<, >);
	}
}
