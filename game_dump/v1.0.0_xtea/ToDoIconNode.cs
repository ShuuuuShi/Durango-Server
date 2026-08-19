using PlayGuide;
using UnityEngine;

public class ToDoIconNode : MonoBehaviour
{
	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private UISprite _portrait;

	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private GameObject _messageOnly;

	private bool _selected;

	private ToDoCollection _collection;

	public ToDoCollection Collection
	{
		get
		{
			return _collection;
		}
		set
		{
			_collection = value;
			_portrait.spriteName = "todo_icon_npc_" + _collection.NPCType;
			bool flag = _collection.IsMessageOnly();
			_messageOnly.SetActive(flag);
			_portrait.alpha = ((!flag) ? 1f : 0.6f);
			_border.alpha = 1f;
		}
	}

	public bool Selected
	{
		get
		{
			return _selected;
		}
		set
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			_selected = value;
			_border.color = ((!_selected) ? PresetColor.UIWhite : PresetColor.UIYellow);
			_border.alpha = ((!_selected) ? 0.7f : 1f);
			_portrait.alpha = ((!_selected) ? 0.7f : 1f);
		}
	}

	public int Height => _portrait.height;

	public float Alpha
	{
		get
		{
			return _widget.alpha;
		}
		set
		{
			_widget.alpha = value;
		}
	}
}
