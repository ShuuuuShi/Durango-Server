using System;

namespace Durango.Logic.Item;

public abstract class TagFilterBase
{
	public struct Tag : IComparable<Tag>
	{
		public string Id;

		public int Level;

		public Tag(string id, int value)
		{
			Id = id;
			Level = value;
		}

		public string GetName()
		{
			return TagData.GetTagName(Id);
		}

		public int CompareTo(Tag y)
		{
			return string.CompareOrdinal(Id, y.Id);
		}
	}

	public abstract int Count { get; }

	public abstract string GetName();

	public abstract int RequiredLevel();

	public abstract string[] GetIdArray();

	public abstract string FirstElementId();
}
