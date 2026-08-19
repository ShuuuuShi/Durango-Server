using Crafting;
using UnityEngine;

public class CategoryControl : SelectableWidget
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _newChecker;

	public RecipeSystem.RecipeType Type { get; set; }

	public string Id => Category.Id;

	public Category Category { get; private set; }

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.localPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Component)this).transform.localPosition = value;
		}
	}

	public int NewCount
	{
		set
		{
			((Component)_newChecker).gameObject.SetActive(value > 0);
			_newChecker.text = value.ToString();
		}
	}

	public void SetCategory(Category category, RecipeSystem.RecipeType type)
	{
		if (Category != null)
		{
			EventDelegate.Remove(Category.NewChecker.OnChangeList, OnChangeNewState);
		}
		Category = category;
		Type = type;
		_textLabel.text = category.Name;
		_iconSprite.spriteName = IconMap.Get(category.Id, "icon_question");
		category.NewChecker.RegisterCallback(OnChangeNewState);
		OnChangeNewState();
	}

	private void OnChangeNewState()
	{
		NewCount = Category.NewChecker.Count;
	}
}
