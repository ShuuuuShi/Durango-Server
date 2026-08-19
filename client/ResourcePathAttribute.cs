using System;

public class ResourcePathAttribute : Attribute
{
	public string Path { get; private set; }

	public ResourcePathAttribute(string path)
	{
		Path = path;
	}
}
