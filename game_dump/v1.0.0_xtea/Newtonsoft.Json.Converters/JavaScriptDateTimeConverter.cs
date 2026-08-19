using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

public class JavaScriptDateTimeConverter : DateTimeConverterBase
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		long value2;
		if (value is DateTime dateTime)
		{
			DateTime dateTime2 = dateTime.ToUniversalTime();
			value2 = JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTime2);
		}
		else
		{
			if (!(value is DateTimeOffset dateTimeOffset))
			{
				throw new Exception("Expected date object value.");
			}
			value2 = JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTimeOffset.ToUniversalTime().UtcDateTime);
		}
		writer.WriteStartConstructor("Date");
		writer.WriteValue(value2);
		writer.WriteEndConstructor();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Type type = ((!ReflectionUtils.IsNullableType(objectType)) ? objectType : Nullable.GetUnderlyingType(objectType));
		if (reader.TokenType == JsonToken.Null)
		{
			if (!ReflectionUtils.IsNullableType(objectType))
			{
				throw new Exception("Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		if (reader.TokenType != JsonToken.StartConstructor || string.Compare(reader.Value.ToString(), "Date", StringComparison.Ordinal) != 0)
		{
			throw new Exception("Unexpected token or value when parsing date. Token: {0}, Value: {1}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType, reader.Value));
		}
		reader.Read();
		if (reader.TokenType != JsonToken.Integer)
		{
			throw new Exception("Unexpected token parsing date. Expected Integer, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		long javaScriptTicks = (long)reader.Value;
		DateTime dateTime = JsonConvert.ConvertJavaScriptTicksToDateTime(javaScriptTicks);
		reader.Read();
		if (reader.TokenType != JsonToken.EndConstructor)
		{
			throw new Exception("Unexpected token parsing date. Expected EndConstructor, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		if ((object)type == typeof(DateTimeOffset))
		{
			return new DateTimeOffset(dateTime);
		}
		return dateTime;
	}
}
