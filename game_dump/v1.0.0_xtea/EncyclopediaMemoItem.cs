using UnityEngine;

public class EncyclopediaMemoItem : Selectable
{
	[SerializeField]
	private UISprite _memoIcon;

	[SerializeField]
	private UILabel _indexLabel;

	[SerializeField]
	private GameObject _newMaker;

	[SerializeField]
	private Color _selectColor;

	private Color _normalColor;

	public int Index { get; private set; }

	protected override void OnInit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_normalColor = _memoIcon.color;
	}

	protected override void Refresh(bool isSelect)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((!isSelect) ? _normalColor : _selectColor);
		_memoIcon.color = color;
		_indexLabel.color = color;
	}

	public void Set(int index)
	{
		Index = index;
		_indexLabel.text = $"#{index}";
		_newMaker.SetActive(false);
	}

	private void OnPress(bool press)
	{
		if (base.AsyncState == State.Normal)
		{
			Refresh(press || base.Select);
		}
	}
}
