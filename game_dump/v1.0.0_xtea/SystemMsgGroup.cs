using System;
using System.Collections.Generic;
using UnityEngine;

public class SystemMsgGroup : UIBase
{
	private struct MessageSet
	{
		public string Key;

		public string Text;

		public float Duration;

		public Action OnClick;
	}

	private class MessageLabel
	{
		public MessageSet Message;

		public float StartTime;

		public float EndTime;

		public UISpriteLabel Label;

		public UISprite Bg;

		public Transform Trans;

		public Vector3 Pos;

		public int Index;

		public void Set(MessageSet set)
		{
			Message = set;
			EndTime = Time.time + set.Duration;
			Label.text = ((set.OnClick != null) ? $"{set.Text} [img_loading_unknown_question2]" : set.Text);
			Bg.UpdateAnchors();
			Trans = ((Component)Label).transform;
			((Collider)((Component)Bg).GetComponent<BoxCollider>()).enabled = set.OnClick != null;
		}
	}

	[SerializeField]
	private int _maxMessageCount;

	[SerializeField]
	private UISpriteLabel _msgLabelBase;

	[SerializeField]
	private float _fadeinTime;

	[SerializeField]
	private float _fadeoutTime;

	[SerializeField]
	private float _animSpeed;

	private readonly Queue<MessageSet> _messageQueue = new Queue<MessageSet>();

	private readonly Stack<UISpriteLabel> _msgLabelPool = new Stack<UISpriteLabel>();

	private readonly List<MessageLabel> _msgLabels = new List<MessageLabel>();

	private bool _needRefreshPosition;

	private void Awake()
	{
		((Component)_msgLabelBase).gameObject.SetActive(false);
	}

	private void Update()
	{
		UpdateMessageState();
		CheckMessageQueue();
		UpdatePosition();
	}

	private void LateUpdate()
	{
		LateRefreshPosition();
	}

	private void CheckMessageQueue()
	{
		if (!UIManager.IsLoadingCurtain && _messageQueue.Count > 0 && _msgLabels.Count < _maxMessageCount)
		{
			AddMessage(_messageQueue.Dequeue());
		}
	}

	private void UpdatePosition()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = _msgLabels.Count; i < count; i++)
		{
			MessageLabel messageLabel = _msgLabels[i];
			Vector3 val = messageLabel.Pos - messageLabel.Trans.localPosition;
			if (((Vector3)(ref val)).sqrMagnitude < 1f)
			{
				messageLabel.Trans.localPosition = messageLabel.Pos;
				continue;
			}
			Vector3 val2 = val * _animSpeed * Time.deltaTime;
			if (((Vector3)(ref val)).sqrMagnitude < ((Vector3)(ref val2)).sqrMagnitude)
			{
				messageLabel.Trans.localPosition = messageLabel.Pos;
				continue;
			}
			Transform trans = messageLabel.Trans;
			trans.localPosition += val2;
		}
	}

	private void LateRefreshPosition()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!_needRefreshPosition)
		{
			return;
		}
		_needRefreshPosition = false;
		float num = 0f;
		int num2 = 0;
		int i = 0;
		for (int count = _msgLabels.Count; i < count; i++)
		{
			MessageLabel messageLabel = _msgLabels[i];
			Transform transform = ((Component)messageLabel.Label).transform;
			Vector3 val = (messageLabel.Pos = Vector3.down * num);
			if (messageLabel.Index == -1)
			{
				if (i < count - 1 || count == 1)
				{
					transform.localPosition = val;
					messageLabel.Pos = val;
				}
				else
				{
					transform.localPosition = val + Vector3.down * 100f;
				}
			}
			num += messageLabel.Label.Label.printedSize.y + 18f;
			messageLabel.Index = num2;
			num2++;
		}
	}

	private void UpdateMessageState()
	{
		int i = 0;
		for (int num = _msgLabels.Count; i < num; i++)
		{
			MessageLabel messageLabel = _msgLabels[i];
			float num2 = messageLabel.EndTime - Time.time;
			if (num2 < 0f)
			{
				MsgLabel_Push(messageLabel.Label);
				_msgLabels.RemoveAt(i);
				i--;
				num--;
				RefreshPosition();
				continue;
			}
			float num3 = Time.time - messageLabel.StartTime;
			if (num3 < _fadeinTime)
			{
				messageLabel.Label.alpha = Mathf.Clamp01(num3 / _fadeinTime);
			}
			else if (num2 < _fadeoutTime)
			{
				messageLabel.Label.alpha = Mathf.Clamp01(num2 / _fadeoutTime);
			}
			else
			{
				messageLabel.Label.alpha = 1f;
			}
		}
	}

	private void RefreshPosition()
	{
		_needRefreshPosition = true;
	}

	private void AddMessage(MessageSet msg)
	{
		MessageLabel messageLabel = new MessageLabel();
		messageLabel.StartTime = Time.time;
		MsgLabel_Pop(out messageLabel.Label, out messageLabel.Bg);
		messageLabel.Set(msg);
		messageLabel.Index = -1;
		_msgLabels.Add(messageLabel);
		RefreshPosition();
	}

	public void PushMessage(string key, string message, float duration, Action onClick = null)
	{
		int num = -1;
		if (!string.IsNullOrEmpty(key))
		{
			for (int i = 0; i < _msgLabels.Count; i++)
			{
				if (_msgLabels[i].Message.Key == key)
				{
					num = i;
					break;
				}
			}
		}
		if (num == -1)
		{
			MessageLabel messageLabel = ((_msgLabels.Count <= 0) ? null : _msgLabels[_msgLabels.Count - 1]);
			if (messageLabel != null && string.Equals(messageLabel.Message.Text, message))
			{
				num = _msgLabels.Count - 1;
			}
		}
		if (num == -1)
		{
			MessageSet item = default(MessageSet);
			item.Key = key;
			item.Text = message;
			item.Duration = duration + _fadeinTime + _fadeoutTime;
			item.OnClick = onClick;
			_messageQueue.Enqueue(item);
		}
		else
		{
			_msgLabels[num].Set(new MessageSet
			{
				Key = key,
				Text = message,
				Duration = duration + _fadeoutTime,
				OnClick = onClick
			});
			RefreshPosition();
		}
	}

	private void MsgLabel_Pop(out UISpriteLabel label, out UISprite bg)
	{
		label = ((_msgLabelPool.Count <= 0) ? ((Component)((Component)_msgLabelBase).transform.parent).gameObject.AddChild(((Component)_msgLabelBase).gameObject).GetComponent<UISpriteLabel>() : _msgLabelPool.Pop());
		((Component)label).gameObject.SetActive(true);
		label.alpha = 0f;
		bg = ((Component)((Component)label).transform.FindChild("Bg")).GetComponent<UISprite>();
		UIEventListener uIEventListener = UIEventListener.Get(((Component)bg).gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickMessage));
	}

	private void MsgLabel_Push(UISpriteLabel label)
	{
		((Component)label).gameObject.SetActive(false);
		_msgLabelPool.Push(label);
	}

	private void OnClickMessage(GameObject obj)
	{
		int num = -1;
		for (int i = 0; i < _msgLabels.Count; i++)
		{
			if ((Object)(object)((Component)_msgLabels[i].Bg).gameObject == (Object)(object)obj)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			_msgLabels[num].EndTime = Mathf.Min(_msgLabels[num].EndTime, Time.time + _fadeoutTime);
			if (_msgLabels[num].Message.OnClick != null)
			{
				_msgLabels[num].Message.OnClick();
			}
		}
	}
}
