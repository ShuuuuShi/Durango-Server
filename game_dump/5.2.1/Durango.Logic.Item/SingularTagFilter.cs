namespace Durango.Logic.Item;

public class SingularTagFilter : TagFilterBase
{
	public string Id;

	private int _level;

	public override int Count => (!string.IsNullOrEmpty(Id)) ? 1 : 0;

	public SingularTagFilter(string id, int level)
	{
		Id = id;
		_level = level;
	}

	public override string GetName()
	{
		return TagData.GetTagName(Id);
	}

	public override int RequiredLevel()
	{
		return _level;
	}

	public override string[] GetIdArray()
	{
		return new string[1] { Id };
	}

	public override string FirstElementId()
	{
		return Id;
	}
}
