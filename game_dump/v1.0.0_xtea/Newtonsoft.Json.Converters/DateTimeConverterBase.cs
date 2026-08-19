using System;

namespace Newtonsoft.Json.Converters;

public abstract class DateTimeConverterBase : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		if ((object)objectType == typeof(DateTime) || (object)objectType == typeof(DateTime?))
		{
			return true;
		}
		if ((object)objectType == typeof(DateTimeOffset) || (object)objectType == typeof(DateTimeOffset?))
		{
			return true;
		}
		return false;
	}
}
