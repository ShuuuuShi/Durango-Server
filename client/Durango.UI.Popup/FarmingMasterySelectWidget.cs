using System;
using System.Collections.Generic;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class FarmingMasterySelectWidget : MonoBehaviour
{
	public enum State
	{
		Acquired,
		Unselected,
		Selectable,
		Locked
	}

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _effectLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private TweenAlpha _bgSprite;

	[SerializeField]
	private UISprite _arrowSprite;

	[SerializeField]
	private GameObject[] _iconEmphasisObjects;

	private int _index;

	public event Action<int> Clicked;

	public void Set(int index, KeyValuePair<string, float> modifier)
	{
		_index = index;
		EncyclopediaModifiers encyclopediaModifiers = ((!string.IsNullOrEmpty(modifier.Key)) ? SingletonDict<string, EncyclopediaModifiers>.Get(modifier.Key) : null);
		if (encyclopediaModifiers == null)
		{
			_nameLabel.text = modifier.Key;
			_effectLabel.text = null;
			_iconSprite.spriteName = null;
		}
		else
		{
			_nameLabel.text = encyclopediaModifiers.Name;
			_effectLabel.text = encyclopediaModifiers.GetValueString(modifier.Value, null, "[icon=img_pet_arrow_up] {0}", "[icon=img_pet_arrow_down] {0}");
			_iconSprite.spriteName = encyclopediaModifiers.Icon;
		}
	}

	public void SetState(State state)
	{
		Color color = Color.white;
		switch (state)
		{
		case State.Acquired:
			color = PresetColor.UIYellow;
			_bgSprite.gameObject.SetActive(value: true);
			_bgSprite.enabled = false;
			_bgSprite.Sample(1f, isFinished: true);
			_arrowSprite.alpha = 1f;
			break;
		case State.Unselected:
			_bgSprite.gameObject.SetActive(value: false);
			_arrowSprite.alpha = 0f;
			color.a = 0.3f;
			break;
		case State.Selectable:
			color = PresetColor.UIYellow;
			_bgSprite.gameObject.SetActive(value: true);
			_bgSprite.enabled = true;
			_bgSprite.tweenFactor = 0f;
			_arrowSprite.alpha = 1f;
			break;
		case State.Locked:
			_bgSprite.gameObject.SetActive(value: false);
			_arrowSprite.alpha = 0f;
			color.a = 0.5f;
			break;
		}
		_nameLabel.color = color;
		_effectLabel.color = color;
		if (_iconEmphasisObjects != null)
		{
			GameObject[] iconEmphasisObjects = _iconEmphasisObjects;
			foreach (GameObject gameObject in iconEmphasisObjects)
			{
				gameObject.gameObject.SetActive(state == State.Acquired);
			}
		}
	}

	private void OnClick()
	{
		if (this.Clicked != null)
		{
			this.Clicked(_index);
		}
	}
}
