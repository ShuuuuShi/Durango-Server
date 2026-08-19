using System;
using Newtonsoft.Json;

namespace Durango.Utils.Converter;

public class GaugeConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is Gauge gauge)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("min");
			writer.WriteValue(gauge.Min());
			writer.WritePropertyName("max");
			writer.WriteValue(gauge.Max());
			writer.WritePropertyName("cur");
			writer.WriteValue(gauge.Get());
			writer.WriteEndObject();
		}
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		float min = 0f;
		float max = 1f;
		float value = 1f;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			switch (text)
			{
			case "min":
				min = Convert.ToSingle(reader.Value);
				break;
			case "max":
				max = Convert.ToSingle(reader.Value);
				break;
			case "cur":
				value = Convert.ToSingle(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return new Gauge(max, min, new GaugeNode[1]
		{
			new GaugeNode(0.0, value)
		});
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Gauge);
	}
}
