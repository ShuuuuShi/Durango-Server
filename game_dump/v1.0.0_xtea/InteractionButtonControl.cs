using System;
using System.Collections.Generic;
using InteractionData;
using UnityEngine;

public class InteractionButtonControl : MonoBehaviour
{
	[SerializeField]
	private InteractionButton _interactionButton;

	[SerializeField]
	private InteractionIconMetaList _interactionIconMetaList;

	private readonly List<InteractionButton> _interactionButtons = new List<InteractionButton>();

	private readonly List<InteractionButton> _waitRemoveObject = new List<InteractionButton>();

	private readonly Queue<InteractionButton> _interactionButtonPool = new Queue<InteractionButton>();

	private InteractionButton _selectedButton;

	private bool _isInit;

	public event Action<InteractionObject> InteractionClicked;

	private void LateUpdate()
	{
		if ((Object)(object)_selectedButton != (Object)null)
		{
			_selectedButton.UpdateIconGradation();
		}
		DrawInteraction();
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			((Component)_interactionButton).gameObject.SetActive(false);
		}
	}

	private InteractionButton InteractionButton_Pop()
	{
		InteractionButton interactionButton;
		if (_interactionButtonPool.Count == 0)
		{
			interactionButton = ((Component)_interactionButton.Transform.parent).gameObject.AddChild(((Component)_interactionButton).gameObject).GetComponent<InteractionButton>();
			UIEventListener uIEventListener = UIEventListener.Get(((Component)interactionButton).gameObject);
			uIEventListener.onClick = ClickInteractionButton;
			uIEventListener.onDrag = UIManager.IgnoreUIDrag;
		}
		else
		{
			interactionButton = _interactionButtonPool.Dequeue();
		}
		interactionButton.ClearConflict();
		((Component)interactionButton).gameObject.SetActive(true);
		return interactionButton;
	}

	private void InteractionButton_Push(InteractionButton btn)
	{
		btn.ClearConflict();
		btn.ResetIconGradation();
		((Component)btn).gameObject.SetActive(false);
		if ((Object)(object)btn == (Object)(object)_selectedButton)
		{
			_selectedButton = null;
		}
		_interactionButtonPool.Enqueue(btn);
	}

	public void SetInteractionButtons(IList<InteractionObject> list)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Init();
		int i = 0;
		for (int count = _interactionButtons.Count; i < count; i++)
		{
			_interactionButtons[i].Valid = false;
		}
		int j = 0;
		for (int size = KUtility.GetSize(list); j < size; j++)
		{
			InteractionObject interactionObject = list[j];
			InteractionButton interactionButton = FindInteractionButton(interactionObject);
			bool flag = false;
			if ((Object)(object)interactionButton == (Object)null)
			{
				interactionButton = InteractionButton_Pop();
				_interactionButtons.Add(interactionButton);
				flag = true;
			}
			interactionButton.Valid = true;
			interactionButton.TouchFlag = false;
			if (flag)
			{
				interactionButton.Set(interactionObject);
				InteractionIconMeta interactionIconMeta = _interactionIconMetaList.Get(interactionButton.Type);
				interactionButton.Color = interactionIconMeta.Color;
				interactionButton.Icon.spriteName = interactionIconMeta.Icon.sprite;
				interactionButton.Icon.depth = interactionIconMeta.Depth;
				interactionButton.Icon.width = (int)((float)_interactionButton.Icon.width * interactionIconMeta.Scale);
				interactionButton.Icon.height = (int)((float)_interactionButton.Icon.height * interactionIconMeta.Scale);
				float duration = 0.2f + interactionObject.DistanceRatio * 3f;
				interactionButton.TweenAlpha(0f, 1f, duration);
			}
			interactionButton.Icon.UpdateAnchors();
		}
		for (int num = _interactionButtons.Count - 1; num >= 0; num--)
		{
			if (!_interactionButtons[num].Valid)
			{
				_waitRemoveObject.Add(_interactionButtons[num]);
				_interactionButtons.RemoveAt(num);
			}
		}
		RemoveInteractionButton();
	}

	private void DrawInteraction()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		int num = _interactionButtons.Count + _waitRemoveObject.Count;
		for (int i = 0; i < num; i++)
		{
			InteractionButton interactionBtn = GetInteractionBtn(i);
			Vector3 position = MainCamera.WorldToNGUIPos(interactionBtn.InteractionTarget.Position);
			interactionBtn.SetPosition(position);
		}
		for (int j = 0; j < num; j++)
		{
			for (int k = j + 1; k < num; k++)
			{
				InteractionButton interactionBtn2 = GetInteractionBtn(j);
				InteractionButton interactionBtn3 = GetInteractionBtn(k);
				Vector3 posDiff = interactionBtn2.PosDiff;
				Vector3 posDiff2 = interactionBtn3.PosDiff;
				if (posDiff != posDiff2 && interactionBtn2.Type == interactionBtn3.Type)
				{
					HideConflictArea(interactionBtn2, interactionBtn3);
				}
			}
		}
		for (int l = 0; l < num; l++)
		{
			InteractionButton interactionBtn4 = GetInteractionBtn(l);
			interactionBtn4.SliceSprite.Refresh(forceRefresh: false);
		}
	}

	private void HideConflictArea(InteractionButton btn1, InteractionButton btn2)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)btn1 == (Object)(object)btn2))
		{
			Vector3 localPosition = btn1.Transform.localPosition;
			Vector3 localPosition2 = btn2.Transform.localPosition;
			Vector3 val = localPosition2 - localPosition;
			Vector3 val2 = localPosition + val * 0.5f;
			if ((Object)(object)btn1 != (Object)(object)_selectedButton)
			{
				btn1.AddConflict(btn2, val2 - localPosition);
			}
			if ((Object)(object)btn2 != (Object)(object)_selectedButton)
			{
				btn2.AddConflict(btn1, val2 - localPosition2);
			}
		}
	}

	private void RemoveInteractionButton()
	{
		int i = 0;
		for (int count = _waitRemoveObject.Count; i < count; i++)
		{
			InteractionButton interactionButton = _waitRemoveObject[i];
			((Component)interactionButton).gameObject.SetActive(true);
			interactionButton.TweenAlpha(interactionButton.Icon.alpha, 0f, 0.5f);
		}
		((MonoBehaviour)this).Invoke("RemoveFinished", 0.6f);
	}

	private void RemoveFinished()
	{
		int i = 0;
		for (int count = _waitRemoveObject.Count; i < count; i++)
		{
			InteractionButton_Push(_waitRemoveObject[i]);
		}
		_waitRemoveObject.Clear();
	}

	private int InteractionButtonIndexOf(InteractionObject obj)
	{
		int i = 0;
		for (int count = _interactionButtons.Count; i < count; i++)
		{
			if (_interactionButtons[i].InteractionTarget == obj)
			{
				return i;
			}
		}
		return -1;
	}

	public InteractionButton FindInteractionButton(InteractionObject obj)
	{
		int num = InteractionButtonIndexOf(obj);
		return (num == -1) ? null : _interactionButtons[num];
	}

	public void SelectAnimation(InteractionObject target)
	{
		if (target != null)
		{
			InteractionButton interactionButton = FindInteractionButton(target);
			if (!((Object)(object)interactionButton == (Object)null))
			{
				interactionButton.Icon.alpha = 1f;
				((Behaviour)interactionButton.AlphaTweener).enabled = false;
				interactionButton.ClearConflict();
				_selectedButton = interactionButton;
			}
		}
	}

	public void UnselectAnimation()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_selectedButton == (Object)null))
		{
			InteractionIconMeta interactionIconMeta = _interactionIconMetaList.Get(_selectedButton.Type);
			_selectedButton.Color = interactionIconMeta.Color;
			_selectedButton.ResetIconGradation();
			_selectedButton = null;
		}
	}

	private void ClickInteractionButton(GameObject go)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(NGUIMath.ScreenToParentPixels(UICamera.currentTouch.pos, go.transform.parent));
		int num = -1;
		float num2 = _interactionButton.Widget.width * _interactionButton.Widget.width;
		float num3 = num2;
		bool flag = true;
		int i = 0;
		for (int count = _interactionButtons.Count; i < count; i++)
		{
			InteractionButton interactionButton = _interactionButtons[i];
			Transform transform = ((Component)interactionButton).transform;
			Vector3 val2 = val - transform.localPosition;
			float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
			if (flag)
			{
				if (interactionButton.TouchFlag)
				{
					if (sqrMagnitude < num3)
					{
						num3 = sqrMagnitude;
						num = i;
					}
				}
				else if (sqrMagnitude < num2)
				{
					flag = false;
					num3 = sqrMagnitude;
					num = i;
				}
			}
			else if (!interactionButton.TouchFlag && sqrMagnitude < num3)
			{
				num3 = sqrMagnitude;
				num = i;
			}
		}
		if (num == -1)
		{
			return;
		}
		if (_interactionButtons[num].TouchFlag)
		{
			for (int j = 0; j < _interactionButtons.Count; j++)
			{
				_interactionButtons[j].TouchFlag = false;
			}
		}
		else
		{
			_interactionButtons[num].TouchFlag = true;
		}
		if (this.InteractionClicked != null)
		{
			this.InteractionClicked(_interactionButtons[num].InteractionTarget);
		}
	}

	private InteractionButton GetInteractionBtn(int index)
	{
		if (index < 0 || index >= _interactionButtons.Count + _waitRemoveObject.Count)
		{
			return null;
		}
		if (index < _interactionButtons.Count)
		{
			return _interactionButtons[index];
		}
		index -= _interactionButtons.Count;
		return _waitRemoveObject[index];
	}
}
