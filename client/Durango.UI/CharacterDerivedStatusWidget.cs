using Durango.UI.Control;
using L10N;
using Shared.Ability;
using UnityEngine;

namespace Durango.UI;

public class CharacterDerivedStatusWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KeyValueLabel _baesStatWidget;

	private UIWidget _widget;

	private ListObjectPool<KeyValueLabel> _stats;

	private Derived[] _abilities;

	private bool _isInit;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_stats = new ListObjectPool<KeyValueLabel>();
			_stats.BaseObject = _baesStatWidget;
			_stats.UseBase = true;
		}
	}

	public void Init(string title, Derived[] abilities)
	{
		Init();
		_titleLabel.text = T._(title);
		_abilities = abilities;
		_baesStatWidget.Widget.SetPosition(_titleWidget.GetPosition(0.5f, 0f), 0.5f, 1f);
		_stats.Set(KUtility.GetSize(abilities));
		for (int i = 0; i < _stats.Count; i++)
		{
			_stats[i].SetKey(abilities[i].GetName());
		}
		float num = _stats.Reposition(Vector3.down);
		Widget.height = _titleWidget.height + (int)num;
	}

	public void Refresh()
	{
		for (int i = 0; i < _stats.Count; i++)
		{
			int num = (int)GameSystem<StatisticsSystem>.Instance().GetDeriveds(_abilities[i]);
			_stats[i].SetValue(num.ToString());
		}
	}

	public void UpdateLayout(int width)
	{
		Widget.width = width;
		for (int i = 0; i < _stats.Count; i++)
		{
			_stats[i].Widget.width = width;
		}
		UIUtility.UpdateAnchors(base.transform);
	}
}
