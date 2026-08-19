using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PlayGuide;
using UnityEngine;

public class ToDoListGroup : UIBase
{
	public const int Width = 100;

	[SerializeField]
	private UIWidget _handle;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UIWidget _vertical;

	[SerializeField]
	private ListObjectPool _nodes;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private AudioClipType _radioSignalAudio;

	[SerializeField]
	private ToDoDetailWidget _detailWidget;

	[SerializeField]
	private GameObject _closeBtn;

	private Vector3 _nodeBeginPos;

	private int _handleLeftAnchor;

	private float _hideIconsTime;

	private float _audioPlayTime;

	private float _iconTweenDuration;

	private float _lastNodeAlpha = 1f;

	public event Action<float> WidthRatioChanged;

	private void Awake()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		base.Flag |= UIFlag.HideToCombat;
		GameSystem<ToDoListSystem>.Instance().Added += ToDoListSystem_Added;
		GameSystem<ToDoListSystem>.Instance().Removed += ToDoListSystem_Removed;
		GameSystem<ToDoListSystem>.Instance().ListUpdated += ToDoListSystem_ListUpdated;
		GameSystem<ToDoListSystem>.Instance().ContextUpdated += ToDoListSystem_ContextUpdated;
		((Component)_vertical).gameObject.SetActive(false);
		_detailWidget.Initialize();
		((Component)_detailWidget).gameObject.SetActive(false);
		_nodeBeginPos = _nodes.BaseObject.transform.localPosition;
		_handleLeftAnchor = _handle.leftAnchor.absolute;
		SoundManager.Cache(_radioSignalAudio);
		UIEventListener.Get(((Component)_handle).gameObject).onClick = delegate
		{
			ShowIcons(visible: true, 0f);
		};
		_scrollView.onDragStarted = delegate
		{
			((Component)_detailWidget).gameObject.SetActive(false);
			_hideIconsTime = 0f;
		};
		UIEventListener.Get(_closeBtn).onClick = delegate
		{
			ShowIcons(visible: false, 0f);
		};
		ToDoListSystem_ListUpdated();
	}

	private void LateUpdate()
	{
		ProcessIconsTween();
		if (_hideIconsTime > 0f && _hideIconsTime <= Time.time)
		{
			ShowIcons(visible: false, 0f);
			_hideIconsTime = 0f;
		}
		ProcessNodeTween();
		bool flag = IsIconsVisible() || _nodes.Count >= 1;
		float num = base.Alpha + ((!flag) ? (0f - Time.deltaTime) : 1f);
		base.Alpha = Mathf.Clamp01(num);
	}

	private void ProcessIconsTween()
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.deltaTime * 4f;
		float num2;
		if (_iconTweenDuration > 0f)
		{
			_iconTweenDuration -= num;
			_iconTweenDuration = Mathf.Max(_iconTweenDuration, 0f);
			num2 = _iconTweenDuration;
		}
		else
		{
			if (!(_iconTweenDuration < 0f))
			{
				return;
			}
			_iconTweenDuration += num;
			_iconTweenDuration = Mathf.Min(_iconTweenDuration, 0f);
			num2 = 1f + _iconTweenDuration;
		}
		int num3 = (int)(100f * num2);
		_vertical.leftAnchor.absolute = num3;
		_vertical.rightAnchor.absolute = num3;
		_vertical.UpdateAnchors();
		_handle.leftAnchor.absolute = _handleLeftAnchor - 100 + num3;
		_handle.rightAnchor.absolute = -100 + num3;
		_handle.UpdateAnchors();
		((Component)_handle).transform.localScale = Vector3.one * num2;
		((Component)_handle).gameObject.SetActive(num2 > 0f);
		((Component)_vertical).gameObject.SetActive(num2 < 1f);
		_detailWidget.Alpha = (1f - num2) * _lastNodeAlpha;
		if (this.WidthRatioChanged != null)
		{
			this.WidthRatioChanged(num2);
		}
	}

	private void ProcessNodeTween()
	{
		if (!IsIconsVisible())
		{
			return;
		}
		_lastNodeAlpha = 1f;
		List<ToDoCollection> collections = GameSystem<ToDoListSystem>.Instance().Collections;
		int count = collections.Count;
		for (int i = 0; i < count; i++)
		{
			ToDoCollection toDoCollection = collections[i];
			float tweenRatio = toDoCollection.TweenRatio;
			if (tweenRatio < 1f)
			{
				ToDoIconNode toDoIconNode = ((ListObjectPoolBase<GameObject>)_nodes).Get<ToDoIconNode>(i);
				toDoIconNode.Alpha = tweenRatio;
				_lastNodeAlpha = tweenRatio;
				if (_detailWidget.Collection == toDoCollection)
				{
					_detailWidget.Alpha = toDoIconNode.Alpha;
				}
			}
		}
	}

	public void ShowIcons(bool visible, float hideTime = 0f)
	{
		if (!visible || base.Visible)
		{
			bool flag = IsIconsVisible();
			if (!flag && visible && _iconTweenDuration <= 0f)
			{
				_iconTweenDuration += 1f;
			}
			else if (flag && !visible && _iconTweenDuration >= 0f)
			{
				_iconTweenDuration -= 1f;
			}
			if ((!flag && hideTime > 0f) || _hideIconsTime > 0f)
			{
				_hideIconsTime = Time.time + hideTime;
			}
		}
	}

	private bool IsIconsVisible()
	{
		return ((Component)_vertical).gameObject.activeSelf && _iconTweenDuration >= 0f;
	}

	[UsedImplicitly]
	private void OnPortraitMode(bool isPortrait)
	{
		if (isPortrait)
		{
			base.Flag |= UIFlag.CoveredByClosable;
		}
		else
		{
			base.Flag &= ~UIFlag.CoveredByClosable;
		}
	}

	private void ToDoListSystem_Added(ToDoCollection collection, bool immediately)
	{
		if (!immediately)
		{
			ShowIcons(visible: true, 4f);
			if (Time.time > _audioPlayTime)
			{
				SoundManager.Play((string)_radioSignalAudio, loop: false, default(SoundManager.PitchRange));
				_audioPlayTime = Time.time + 1f;
			}
			if (!collection.IsMessageOnly())
			{
				SelectNodeByCollection(collection);
			}
		}
	}

	private void ToDoListSystem_Removed(ToDoCollection collection, bool immediately)
	{
		if (!collection.IsMessageOnly() && !immediately)
		{
			ShowIcons(visible: true, 2.5f);
			SelectNodeByCollection(collection);
			UpdateCountLabel();
		}
	}

	private void ToDoListSystem_ListUpdated()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		List<ToDoCollection> collections = GameSystem<ToDoListSystem>.Instance().Collections;
		int count = collections.Count;
		_nodes.Set(count);
		SelectNode(-1);
		Vector3 nodeBeginPos = _nodeBeginPos;
		for (int i = 0; i < count; i++)
		{
			ToDoCollection collection = collections[i];
			ToDoIconNode toDoIconNode = ((ListObjectPoolBase<GameObject>)_nodes).Get<ToDoIconNode>(i);
			toDoIconNode.Alpha = 1f;
			toDoIconNode.Collection = collection;
			((Component)toDoIconNode).transform.localPosition = nodeBeginPos;
			nodeBeginPos.y -= (float)toDoIconNode.Height;
			nodeBeginPos.y -= 30f;
			UIEventListener.Get(((Component)toDoIconNode).gameObject).onClick = Node_OnClick;
		}
		UpdateCountLabel();
		_scrollView.ResetPosition();
		if (count == 0)
		{
			ShowIcons(visible: false, 0f);
		}
	}

	private void UpdateCountLabel()
	{
		List<ToDoCollection> collections = GameSystem<ToDoListSystem>.Instance().Collections;
		int num = 0;
		for (int i = 0; i < collections.Count; i++)
		{
			ToDoCollection toDoCollection = collections[i];
			if (!toDoCollection.WillBeRemoved)
			{
				num++;
			}
		}
		((Component)_countLabel).gameObject.SetActive(num > 0);
		_countLabel.text = num.ToString();
	}

	private void ToDoListSystem_ContextUpdated(ToDoCollection collection, ToDoBase todo, bool textOnly)
	{
		if (textOnly)
		{
			if (_detailWidget.Collection == collection)
			{
				_detailWidget.Set(collection);
			}
		}
		else
		{
			ShowIcons(visible: true, 2f);
			SelectNodeByCollection(collection);
			_detailWidget.ShowUpdatedFeedBack(todo);
		}
	}

	private void Node_OnClick(GameObject go)
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			ToDoIconNode toDoIconNode = ((ListObjectPoolBase<GameObject>)_nodes).Get<ToDoIconNode>(i);
			toDoIconNode.Selected = false;
		}
		int index = _nodes.IndexOf(go);
		ToDoCollection toDoCollection = SelectNode(index);
		if (toDoCollection != null && toDoCollection.Clicked != null)
		{
			toDoCollection.Clicked();
		}
		_hideIconsTime = 0f;
	}

	private ToDoCollection SelectNode(int index)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _nodes.Count; i++)
		{
			ToDoIconNode toDoIconNode = ((ListObjectPoolBase<GameObject>)_nodes).Get<ToDoIconNode>(i);
			toDoIconNode.Selected = false;
		}
		if (index < 0 || index >= _nodes.Count)
		{
			((Component)_detailWidget).gameObject.SetActive(false);
			return null;
		}
		ToDoIconNode toDoIconNode2 = ((ListObjectPoolBase<GameObject>)_nodes).Get<ToDoIconNode>(index);
		toDoIconNode2.Selected = true;
		_detailWidget.Alpha = 1f;
		((Component)_detailWidget).gameObject.SetActive(true);
		_detailWidget.Set(toDoIconNode2.Collection);
		Vector3 val = ((Component)_detailWidget).transform.parent.InverseTransformPoint(((Component)toDoIconNode2).transform.position);
		int num = -(UIManager.ScreenHeight - _detailWidget.Height + -10);
		int num2 = (int)val.y + 20;
		Vector3 localPosition = ((Component)_detailWidget).transform.localPosition;
		localPosition.y = Mathf.Clamp(num2, num, -110);
		((Component)_detailWidget).transform.localPosition = localPosition;
		_detailWidget.SetTailOffset((float)num2 - localPosition.y);
		return toDoIconNode2.Collection;
	}

	private void SelectNodeByCollection(ToDoCollection collection)
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			ToDoIconNode toDoIconNode = ((ListObjectPoolBase<GameObject>)_nodes).Get<ToDoIconNode>(i);
			if (toDoIconNode.Collection == collection)
			{
				SelectNode(i);
				break;
			}
		}
	}

	[ExposedInEditor(null)]
	public void HideToDoList()
	{
		TweenAlpha.Begin(((Component)this).gameObject, 0.2f, 0f);
	}

	[ExposedInEditor(null)]
	public void RestoreToDoList()
	{
		TweenAlpha.Begin(((Component)this).gameObject, 0.2f, 1f);
	}

	[ExposedInEditor(null)]
	private void UpdateTweenTest()
	{
		foreach (ToDoCollection collection in GameSystem<ToDoListSystem>.Instance().Collections)
		{
			if (collection.ToDoList == null)
			{
				continue;
			}
			using IEnumerator<ToDoBase> enumerator2 = collection.ToDoList.GetEnumerator();
			if (enumerator2.MoveNext())
			{
				ToDoBase current2 = enumerator2.Current;
				current2.CallComplete();
				GameSystem<ToDoListSystem>.Instance().Remove(collection);
				break;
			}
		}
	}

	[ExposedInEditor(null)]
	private void AddCollectionByNPCType()
	{
		GameSystem<ToDoListSystem>.Instance().RemoveAll();
		int num = Enum.GetNames(typeof(NPCType)).Length;
		for (int i = 0; i < num; i++)
		{
			ToDoCollection toDoCollection = new ToDoCollection();
			NPCType nPCType = (toDoCollection.NPCType = (NPCType)i);
			toDoCollection.Title = "테스트용 할 일: " + i;
			toDoCollection.Key = nPCType.ToString();
			toDoCollection.ToDoList = new List<ToDoBase>();
			int num2 = Math.Min(i, 4);
			for (int j = 0; j < num2; j++)
			{
				GatherItemToDo gatherItemToDo = new GatherItemToDo(1);
				gatherItemToDo.LocalText = "아이템 찾아주기 하하하 " + j;
				gatherItemToDo.Key = string.Concat(nPCType, ".gather_", j);
				GatherItemToDo item = gatherItemToDo;
				toDoCollection.ToDoList.Add(item);
			}
			GameSystem<ToDoListSystem>.Instance().Add(toDoCollection);
		}
	}

	[ExposedInEditor(null)]
	private void AddCollectionByNPCType2()
	{
		for (int i = 0; i < 1; i++)
		{
			ToDoCollection toDoCollection = new ToDoCollection();
			NPCType nPCType = (toDoCollection.NPCType = (NPCType)i);
			toDoCollection.Title = "테스트용 할 일: " + i;
			toDoCollection.Key = nPCType.ToString();
			toDoCollection.ToDoList = new List<ToDoBase>();
			int num = Math.Min(i, 4);
			for (int j = 0; j < num; j++)
			{
				GatherItemToDo gatherItemToDo = new GatherItemToDo(1);
				gatherItemToDo.LocalText = "아이템 찾아주기 하하하 " + j;
				gatherItemToDo.Key = string.Concat(nPCType, ".gather_", j);
				GatherItemToDo item = gatherItemToDo;
				toDoCollection.ToDoList.Add(item);
			}
			GameSystem<ToDoListSystem>.Instance().Add(toDoCollection);
		}
	}
}
