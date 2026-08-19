using System;
using System.Text;
using Converter;
using Durango.Utils.Converter;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Durango.Utils;

public static class Json
{
	private static JsonSerializerSettings _writeSettings;

	private static JsonSerializerSettings WriteSettings
	{
		get
		{
			if (_writeSettings == null)
			{
				_writeSettings = new JsonSerializerSettings();
				_writeSettings.Converters.Add(new PairConverter());
				_writeSettings.Converters.Add(new GettextConverter());
				_writeSettings.Converters.Add(new ColorConverter());
				_writeSettings.Converters.Add(new GaugeConverter());
			}
			return _writeSettings;
		}
	}

	[CanBeNull]
	public static T Read<T>(string json, bool logException = false)
	{
		if (string.IsNullOrEmpty(json))
		{
			return default(T);
		}
		try
		{
			return JsonConvert.DeserializeObject<T>(json, Converters.Setting);
		}
		catch (Exception exception)
		{
			if (logException)
			{
				Debug.LogException(exception);
			}
		}
		return default(T);
	}

	[CanBeNull]
	public static T Read<T>(byte[] data, bool logException = false)
	{
		if (data == null || data.Length == 0)
		{
			return default(T);
		}
		return Read<T>(Encoding.UTF8.GetString(data), logException);
	}

	[CanBeNull]
	public static T Read<T>(JToken jToken)
	{
		if (jToken == null)
		{
			return default(T);
		}
		T result;
		using (JTokenReader reader = new JTokenReader(jToken))
		{
			JsonSerializer jsonSerializer = JsonSerializer.Create(Converters.Setting);
			try
			{
				result = jsonSerializer.Deserialize<T>(reader);
				return result;
			}
			catch (JsonSerializationException ex)
			{
				result = default(T);
				Debug.LogError("Json Parse Error\n" + ex.Message + jToken);
			}
		}
		return result;
	}

	[CanBeNull]
	public static T ReadFromFile<T>(string fileName)
	{
		TextAsset textAsset = Resources.Load(fileName) as TextAsset;
		if (textAsset == null)
		{
			Debug.LogError("Cannot load json file - " + fileName);
			return default(T);
		}
		return Read<T>(textAsset.text);
	}

	public static string Write<T>(T data, bool indented = false, JsonSerializerSettings settings = null)
	{
		string result = string.Empty;
		try
		{
			result = JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None, (settings == null) ? WriteSettings : settings);
		}
		catch (JsonSerializationException ex)
		{
			string message = ex.Message;
			T val = data;
			Debug.LogError("Json Serialize Error\n" + message + val);
		}
		return result;
	}

	public static byte[] WriteToBytes<T>(T data, bool indented = false, JsonSerializerSettings settings = null)
	{
		string s = Write(data, indented, settings);
		return Encoding.UTF8.GetBytes(s);
	}
}
