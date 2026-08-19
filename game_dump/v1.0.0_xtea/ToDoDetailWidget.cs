using PlayGuide;
using UnityEngine;

public class ToDoDetailWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private UIWidget _bg;

	[SerializeField]
	private UIWidget _grunge;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private ListObjectPool _checkBoxPool;

	[SerializeField]
	private UISprite _tail;

	private Vector3 _beginPos;

	private Vector3 _tailBasePos;

	public ToDoCollection Collection { get; private set; }

	public int Height => _bg.height;

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

	public void Initialize()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		_beginPos = _checkBoxPool.BaseObject.transform.localPosition;
		_tailBasePos = ((Component)_tail).transform.localPosition;
	}

	public void Set(ToDoCollection collection)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Collection = collection;
		int size = KUtility.GetSize(Collection.ToDoList);
		_checkBoxPool.Set(size);
		Vector3 beginPos = _beginPos;
		for (int i = 0; i < size; i++)
		{
			ToDoBase toDo = Collection.ToDoList[i];
			ToDoCheckBoxControl toDoCheckBoxControl = ((ListObjectPoolBase<GameObject>)_checkBoxPool).Get<ToDoCheckBoxControl>(i);
			toDoCheckBoxControl.SetToDo(toDo);
			((Component)toDoCheckBoxControl).transform.localPosition = beginPos;
			beginPos.y -= (float)toDoCheckBoxControl.Height;
			beginPos.y -= 15f;
		}
		_titleLabel.text = Collection.Title;
		int num = (int)(0f - beginPos.y) + 10;
		_bg.height = num;
		_grunge.width = num;
	}

	public void SetTailOffset(float offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 tailBasePos = _tailBasePos;
		tailBasePos.y += offset;
		tailBasePos.y = Mathf.Clamp(tailBasePos.y, (float)(-Height + _tail.height), 0f);
		((Component)_tail).transform.localPosition = tailBasePos;
	}

	public void ShowUpdatedFeedBack(ToDoBase todo)
	{
		for (int i = 0; i < _checkBoxPool.Count; i++)
		{
			ToDoCheckBoxControl toDoCheckBoxControl = ((ListObjectPoolBase<GameObject>)_checkBoxPool).Get<ToDoCheckBoxControl>(i);
			if (toDoCheckBoxControl.Todo == todo)
			{
				toDoCheckBoxControl.ShowUpdatedFeedBack();
				break;
			}
		}
	}
}
