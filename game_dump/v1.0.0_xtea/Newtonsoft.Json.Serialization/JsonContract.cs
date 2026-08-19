using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public abstract class JsonContract
{
	public Type UnderlyingType { get; private set; }

	public Type CreatedType { get; set; }

	public bool? IsReference { get; set; }

	public JsonConverter Converter { get; set; }

	internal JsonConverter InternalConverter { get; set; }

	public MethodInfo OnDeserialized { get; set; }

	public MethodInfo OnDeserializing { get; set; }

	public MethodInfo OnSerialized { get; set; }

	public MethodInfo OnSerializing { get; set; }

	public Func<object> DefaultCreator { get; set; }

	public bool DefaultCreatorNonPublic { get; set; }

	public MethodInfo OnError { get; set; }

	internal JsonContract(Type underlyingType)
	{
		ValidationUtils.ArgumentNotNull(underlyingType, "underlyingType");
		UnderlyingType = underlyingType;
		CreatedType = underlyingType;
	}

	internal void InvokeOnSerializing(object o, StreamingContext context)
	{
		if ((object)OnSerializing != null)
		{
			OnSerializing.Invoke(o, new object[1] { context });
		}
	}

	internal void InvokeOnSerialized(object o, StreamingContext context)
	{
		if ((object)OnSerialized != null)
		{
			OnSerialized.Invoke(o, new object[1] { context });
		}
	}

	internal void InvokeOnDeserializing(object o, StreamingContext context)
	{
		if ((object)OnDeserializing != null)
		{
			OnDeserializing.Invoke(o, new object[1] { context });
		}
	}

	internal void InvokeOnDeserialized(object o, StreamingContext context)
	{
		if ((object)OnDeserialized != null)
		{
			OnDeserialized.Invoke(o, new object[1] { context });
		}
	}

	internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
	{
		if ((object)OnError != null)
		{
			OnError.Invoke(o, new object[2] { context, errorContext });
		}
	}
}
