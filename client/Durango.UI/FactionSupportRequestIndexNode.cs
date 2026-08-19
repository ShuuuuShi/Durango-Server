using System;
using UnityEngine;

namespace Durango.UI;

public class FactionSupportRequestIndexNode : UIWidget
{
	public Action<FactionSupportRequestIndexNode> Clicked;

	[SerializeField]
	private UIWidget _mainObject;

	[SerializeField]
	private UIWidget _selectorObject;

	[SerializeField]
	private GameObject _lockedObject;

	[SerializeField]
	private UILabel _indexLabel;

	private bool _isLocked;

	public int Level { get; private set; }

	public bool Locked
	{
		get
		{
			return _isLocked;
		}
		set
		{
			_isLocked = value;
			_mainObject.alpha = ((!_isLocked) ? 1f : 0.5f);
			_lockedObject.gameObject.SetActive(_isLocked);
		}
	}

	public void Set(int level)
	{
		Level = level;
		_indexLabel.text = level.ToString();
	}

	public void SetSelectRatio(float ratio)
	{
		float t = ratio * ratio;
		_selectorObject.alpha = t;
		_indexLabel.color = Color.Lerp(Color.white, PresetColor.UIYellow, t);
	}

	private void OnClick()
	{
		if (Clicked != null)
		{
			Clicked(this);
		}
	}
}
