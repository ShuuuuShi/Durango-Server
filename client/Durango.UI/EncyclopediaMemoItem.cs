using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EncyclopediaMemoItem : SelectableWidget
{
	[SerializeField]
	private UISprite _memoIcon;

	[SerializeField]
	private UILabel _indexLabel;

	[SerializeField]
	private GameObject _newMaker;

	public int Index { get; private set; }

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}

	public void Set(int index)
	{
		Index = index;
		_indexLabel.text = $"#{index}";
		_newMaker.SetActive(value: false);
	}
}
