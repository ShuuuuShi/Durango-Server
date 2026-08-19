using System;
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

internal class LateBoundMetadataTypeAttribute : IMetadataTypeAttribute
{
	private static PropertyInfo _metadataClassTypeProperty;

	private readonly object _attribute;

	public Type MetadataClassType
	{
		get
		{
			if ((object)_metadataClassTypeProperty == null)
			{
				_metadataClassTypeProperty = _attribute.GetType().GetProperty("MetadataClassType");
			}
			return (Type)ReflectionUtils.GetMemberValue(_metadataClassTypeProperty, _attribute);
		}
	}

	public LateBoundMetadataTypeAttribute(object attribute)
	{
		_attribute = attribute;
	}
}
