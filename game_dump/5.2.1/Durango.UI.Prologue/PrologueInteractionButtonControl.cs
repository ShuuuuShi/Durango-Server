using System;
using System.Collections.Generic;
using Durango.Render.Camera;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueInteractionButtonControl : MonoBehaviour
{
	public enum InteractionIconType
	{
		Default,
		Animal,
		Prop,
		PrologueCharacter,
		Special
	}

	[Serializable]
	public struct InteractionIconMeta
	{
		public SpriteData Icon;

		public Color Color;

		public int Depth;

		public float Scale;
	}

	[Serializable]
	[EnumType(typeof(InteractionIconType))]
	public class InteractionIconMetaList : EnumKeyList
	{
		[SerializeField]
		private List<InteractionIconMeta> _values;

		public InteractionIconMeta Get(InteractionIconType type)
		{
			int num = IndexOf((int)type);
			if (num != -1)
			{
				return _values[num];
			}
			return default(InteractionIconMeta);
		}
	}

	[SerializeField]
	private PrologueInteractionButton _interactionButton;

	[SerializeField]
	private InteractionIconMetaList _interactionIconMetaList;

	private readonly List<PrologueInteractionButton> _interactionButtons = new List<PrologueInteractionButton>();

	private readonly List<PrologueInteractionButton> _waitRemoveObject = new List<PrologueInteractionButton>();

	private readonly Queue<PrologueInteractionButton> _interactionButtonPool = new Queue<PrologueInteractionButton>();

	private PrologueInteractionButton _selectedButton;

	private bool _isInit;

	public event Action<InteractionObject> InteractionClicked;

	private void LateUpdate()
	{
		if (_selectedButton != null)
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
			_interactionButton.gameObject.SetActive(value: false);
		}
	}

	private PrologueInteractionButton InteractionButton_Pop()
	{
		PrologueInteractionButton prologueInteractionButton;
		if (_interactionButtonPool.Count == 0)
		{
			prologueInteractionButton = _interactionButton.Transform.parent.gameObject.AddChild(_interactionButton.gameObject).GetComponent<PrologueInteractionButton>();
			UIEventListener uIEventListener = UIEventListener.Get(prologueInteractionButton.gameObject);
			uIEventListener.onClick = ClickInteractionButton;
			uIEventListener.onDrag = UIManager.IgnoreUIDrag;
		}
		else
		{
			prologueInteractionButton = _interactionButtonPool.Dequeue();
		}
		prologueInteractionButton.ClearConflict();
		prologueInteractionButton.gameObject.SetActive(value: true);
		return prologueInteractionButton;
	}

	private void InteractionButton_Push(PrologueInteractionButton btn)
	{
		btn.ClearConflict();
		btn.ResetIconGradation();
		btn.gameObject.SetActive(value: false);
		if (btn == _selectedButton)
		{
			_selectedButton = null;
		}
		_interactionButtonPool.Enqueue(btn);
	}

	public void SetInteractionButtons(IList<InteractionObject> list)
	{
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
			PrologueInteractionButton prologueInteractionButton = FindInteractionButton(interactionObject);
			bool flag = false;
			if (prologueInteractionButton == null)
			{
				prologueInteractionButton = InteractionButton_Pop();
				_interactionButtons.Add(prologueInteractionButton);
				flag = true;
			}
			prologueInteractionButton.Valid = true;
			prologueInteractionButton.TouchFlag = false;
			if (flag)
			{
				prologueInteractionButton.Set(interactionObject);
				InteractionIconMeta interactionIconMeta = _interactionIconMetaList.Get(prologueInteractionButton.Type);
				prologueInteractionButton.Color = interactionIconMeta.Color;
				prologueInteractionButton.Icon.spriteName = interactionIconMeta.Icon.sprite;
				prologueInteractionButton.Icon.depth = interactionIconMeta.Depth;
				prologueInteractionButton.Icon.width = (int)((float)_interactionButton.Icon.width * interactionIconMeta.Scale);
				prologueInteractionButton.Icon.height = (int)((float)_interactionButton.Icon.height * interactionIconMeta.Scale);
				float duration = 0.2f + interactionObject.DistanceRatio * 3f;
				prologueInteractionButton.TweenAlpha(0f, 1f, duration);
			}
			prologueInteractionButton.Icon.UpdateAnchors();
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
		int num = _interactionButtons.Count + _waitRemoveObject.Count;
		for (int i = 0; i < num; i++)
		{
			PrologueInteractionButton interactionBtn = GetInteractionBtn(i);
			Vector3 position = MainCamera.WorldToNGUIPos(interactionBtn.InteractionTarget.Position);
			interactionBtn.SetPosition(position);
		}
		for (int j = 0; j < num; j++)
		{
			for (int k = j + 1; k < num; k++)
			{
				PrologueInteractionButton interactionBtn2 = GetInteractionBtn(j);
				PrologueInteractionButton interactionBtn3 = GetInteractionBtn(k);
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
			GetInteractionBtn(l).SliceSprite.Refresh(forceRefresh: false);
		}
	}

	private void HideConflictArea(PrologueInteractionButton btn1, PrologueInteractionButton btn2)
	{
		if (!(btn1 == btn2))
		{
			Vector3 localPosition = btn1.Transform.localPosition;
			Vector3 localPosition2 = btn2.Transform.localPosition;
			Vector3 vector = localPosition2 - localPosition;
			Vector3 vector2 = localPosition + vector * 0.5f;
			if (btn1 != _selectedButton)
			{
				btn1.AddConflict(btn2, vector2 - localPosition);
			}
			if (btn2 != _selectedButton)
			{
				btn2.AddConflict(btn1, vector2 - localPosition2);
			}
		}
	}

	private void RemoveInteractionButton()
	{
		int i = 0;
		for (int count = _waitRemoveObject.Count; i < count; i++)
		{
			PrologueInteractionButton prologueInteractionButton = _waitRemoveObject[i];
			prologueInteractionButton.gameObject.SetActive(value: true);
			prologueInteractionButton.TweenAlpha(prologueInteractionButton.Icon.alpha, 0f, 0.5f);
		}
		Invoke("RemoveFinished", 0.6f);
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

	public PrologueInteractionButton FindInteractionButton(InteractionObject obj)
	{
		int num = InteractionButtonIndexOf(obj);
		if (num != -1)
		{
			return _interactionButtons[num];
		}
		return null;
	}

	public void SelectAnimation(InteractionObject target)
	{
		if (target != null)
		{
			PrologueInteractionButton prologueInteractionButton = FindInteractionButton(target);
			if (!(prologueInteractionButton == null))
			{
				prologueInteractionButton.Icon.alpha = 1f;
				prologueInteractionButton.AlphaTweener.enabled = false;
				prologueInteractionButton.ClearConflict();
				_selectedButton = prologueInteractionButton;
			}
		}
	}

	public void UnselectAnimation()
	{
		if (!(_selectedButton == null))
		{
			InteractionIconMeta interactionIconMeta = _interactionIconMetaList.Get(_selectedButton.Type);
			_selectedButton.Color = interactionIconMeta.Color;
			_selectedButton.ResetIconGradation();
			_selectedButton = null;
		}
	}

	private void ClickInteractionButton(GameObject go)
	{
		Vector3 vector = NGUIMath.ScreenToParentPixels(UICamera.currentTouch.pos, go.transform.parent);
		int num = -1;
		float num2 = _interactionButton.Widget.width * _interactionButton.Widget.width;
		float num3 = num2;
		bool flag = true;
		int i = 0;
		for (int count = _interactionButtons.Count; i < count; i++)
		{
			PrologueInteractionButton prologueInteractionButton = _interactionButtons[i];
			Transform transform = prologueInteractionButton.transform;
			float sqrMagnitude = (vector - transform.localPosition).sqrMagnitude;
			if (flag)
			{
				if (prologueInteractionButton.TouchFlag)
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
			else if (!prologueInteractionButton.TouchFlag && sqrMagnitude < num3)
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

	private PrologueInteractionButton GetInteractionBtn(int index)
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
