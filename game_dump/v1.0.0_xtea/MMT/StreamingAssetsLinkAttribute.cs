using System;
using UnityEngine;

namespace MMT;

public class StreamingAssetsLinkAttribute : PropertyAttribute
{
	public Type LinkType { get; private set; }

	public string Label { get; private set; }

	public StreamingAssetsLinkAttribute(Type a_type, string a_label)
	{
		LinkType = a_type;
		Label = a_label;
	}
}
