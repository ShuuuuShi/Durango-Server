namespace Crafting;

public abstract class CategoryItem : INewCheckerable
{
	public string Id;

	public string Name;

	public string Category;

	public string Icon;

	private NewCheckerNode _newChecker;

	public bool Available { get; set; }

	public virtual string LocalizedName => Name;

	public NewChecker NewChecker
	{
		get
		{
			if (_newChecker == null)
			{
				_newChecker = new NewCheckerNode();
				_newChecker.Key = $"{GetType().Name}:{Id}";
			}
			return _newChecker;
		}
	}

	public string LocalizedNameWithIcon()
	{
		return $"[{Icon}] {LocalizedName}";
	}
}
