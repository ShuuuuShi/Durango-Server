using UnityEngine;

public class LocalizeKey : MonoBehaviour
{
	[SerializeField]
	private string _key;

	private UISpriteLabel _spriteLabel;

	private UILabel _label;

	public string Key
	{
		get
		{
			return _key;
		}
		set
		{
			_key = value;
			if (NGUITools.GetActive((Behaviour)(object)this))
			{
				OnLocalize();
			}
		}
	}

	private UISpriteLabel SpriteLabel
	{
		get
		{
			if ((Object)(object)_spriteLabel == (Object)null)
			{
				_spriteLabel = ((Component)this).GetComponent<UISpriteLabel>();
			}
			return _spriteLabel;
		}
	}

	private UILabel Label
	{
		get
		{
			if ((Object)(object)_label == (Object)null)
			{
				_label = ((Component)this).GetComponent<UILabel>();
			}
			return _label;
		}
	}

	private void OnEnable()
	{
		OnLocalize();
	}

	public void OnLocalize()
	{
		if (!string.IsNullOrEmpty(_key))
		{
			string text = LocalizeSystem.Get(_key);
			if ((Object)(object)SpriteLabel != (Object)null)
			{
				SpriteLabel.text = text;
			}
			else if ((Object)(object)Label != (Object)null)
			{
				Label.text = text;
			}
		}
	}

	public static void Set(GameObject go, string key)
	{
		LocalizeKey localizeKey = go.GetComponent<LocalizeKey>();
		if ((Object)(object)localizeKey == (Object)null)
		{
			localizeKey = go.AddComponent<LocalizeKey>();
		}
		localizeKey.Key = key;
	}
}
