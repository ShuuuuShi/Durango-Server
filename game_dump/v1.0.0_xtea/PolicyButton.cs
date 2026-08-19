using UnityEngine;

public class PolicyButton : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private Color _iconColorNormal;

	[SerializeField]
	private Color _iconColorPressed;

	[SerializeField]
	private Color _iconColorSelected;

	[SerializeField]
	private UISprite _buttonImage;

	[SerializeField]
	private SpriteData _buttonNormal;

	[SerializeField]
	private SpriteData _buttonPressed;

	[SerializeField]
	private SpriteData _buttonSelected;

	[SerializeField]
	private TweenScale _tweenScale;

	[SerializeField]
	private TweenScale _iconScale;

	private bool isSelected;

	public string PolicyId { get; private set; }

	public int PolicyLevel { get; private set; }

	public string PolicyName { get; private set; }

	public string Description { get; private set; }

	public bool IsSelected
	{
		get
		{
			return isSelected;
		}
		set
		{
			isSelected = value;
			RefresImageAndColor();
		}
	}

	public void Set(string id, int level, string name, string description, string iconName)
	{
		PolicyId = id;
		PolicyLevel = level;
		PolicyName = name;
		Description = description;
		IsSelected = false;
		UIUtility.SetSpriteName(_icon, iconName);
		RefresImageAndColor();
	}

	private void OnPress(bool press)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (press)
		{
			_buttonPressed.Set(_buttonImage);
			_icon.color = _iconColorPressed;
		}
		else
		{
			RefresImageAndColor();
		}
	}

	private void RefresImageAndColor()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (IsSelected)
		{
			_buttonSelected.Set(_buttonImage);
			_icon.color = _iconColorSelected;
			_tweenScale.PlayForward();
			_iconScale.PlayForward();
		}
		else
		{
			_buttonNormal.Set(_buttonImage);
			_icon.color = _iconColorNormal;
			_tweenScale.ResetToBeginning();
			_iconScale.ResetToBeginning();
		}
	}
}
