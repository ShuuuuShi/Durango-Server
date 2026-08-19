namespace Crafting;

public abstract class Category : INewCheckerable
{
	public string Id;

	public string Name;

	private NewCheckerContainer _newChecker;

	public abstract CategoryItem[] Items { get; }

	public NewChecker NewChecker
	{
		get
		{
			if (_newChecker == null)
			{
				_newChecker = new NewCheckerContainer();
			}
			return _newChecker;
		}
	}

	public void OnInit()
	{
		CategoryItem[] items = Items;
		NewChecker.ClearChild();
		int i = 0;
		for (int num = ((items != null) ? items.Length : 0); i < num; i++)
		{
			NewChecker.AddChild(items[i]);
		}
	}
}
