using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class AlarmMessageQueue : MonoBehaviour, AlarmRewardQueue.IMessageGroup
{
	private struct MessageSet
	{
		public string Key;

		public string Text;

		public float Duration;

		public float Scale;
	}

	[SerializeField]
	private int _maxMessageCount;

	[SerializeField]
	private AlarmMessageWidget _messageComponentBase;

	[SerializeField]
	private int _margin;

	[SerializeField]
	private float _animSpeed;

	private readonly Queue<MessageSet> _messageQueue = new Queue<MessageSet>();

	private readonly Stack<AlarmMessageWidget> _msgLabelPool = new Stack<AlarmMessageWidget>();

	private readonly List<AlarmMessageWidget> _msgLabels = new List<AlarmMessageWidget>();

	private bool _isPause;

	private bool _needRefreshPosition;

	private void Awake()
	{
		_messageComponentBase.gameObject.SetActive(value: false);
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
		if (_msgLabels.Count == 0 && _messageQueue.Count == 0)
		{
			base.enabled = false;
		}
	}

	private void CheckMessageQueue()
	{
		if (!UIManager.IsLoadingCurtain && !_isPause && _messageQueue.Count > 0 && _msgLabels.Count < _maxMessageCount)
		{
			AddMessage(_messageQueue.Dequeue());
		}
	}

	private void UpdatePosition()
	{
		int i = 0;
		for (int count = _msgLabels.Count; i < count; i++)
		{
			AlarmMessageWidget alarmMessageWidget = _msgLabels[i];
			alarmMessageWidget.UpdatePosition(_animSpeed * Time.deltaTime);
		}
	}

	private void LateRefreshPosition()
	{
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
			AlarmMessageWidget alarmMessageWidget = _msgLabels[i];
			Transform transform = alarmMessageWidget.transform;
			Vector3 vector2 = (alarmMessageWidget.TargetPosition = Vector3.down * num);
			if (alarmMessageWidget.Index == -1)
			{
				if (i < count - 1 || count == 1)
				{
					transform.localPosition = vector2;
					alarmMessageWidget.TargetPosition = vector2;
				}
				else
				{
					transform.localPosition = vector2 + Vector3.down * 10f;
				}
			}
			num += (float)(alarmMessageWidget.Widget.height + _margin);
			alarmMessageWidget.Index = num2;
			num2++;
		}
	}

	private void UpdateMessageState()
	{
		int i = 0;
		for (int num = _msgLabels.Count; i < num; i++)
		{
			AlarmMessageWidget alarmMessageWidget = _msgLabels[i];
			float num2 = alarmMessageWidget.Until - Time.time;
			if (num2 < 0f)
			{
				MsgLabel_Push(alarmMessageWidget);
				_msgLabels.RemoveAt(i);
				i--;
				num--;
				RefreshPosition();
				continue;
			}
			float num3 = Time.time - alarmMessageWidget.Since;
			if (num3 < 0.3f)
			{
				alarmMessageWidget.Widget.alpha = Mathf.Clamp01(num3 / 0.3f);
			}
			else if (num2 < 0.3f)
			{
				alarmMessageWidget.Widget.alpha = Mathf.Clamp01(num2 / 0.3f);
			}
			else
			{
				alarmMessageWidget.Widget.alpha = 1f;
			}
		}
	}

	private void RefreshPosition()
	{
		_needRefreshPosition = true;
	}

	private void AddMessage(MessageSet msg)
	{
		AlarmMessageWidget alarmMessageWidget = MsgLabel_Pop();
		alarmMessageWidget.Set(msg.Key, msg.Text, msg.Duration, msg.Scale <= 0f ? 1f : msg.Scale);
		_msgLabels.Add(alarmMessageWidget);
		RefreshPosition();
	}

	[ExposedInEditor(null)]
	public void PushMessage(string key, string message, float duration, float scale = 1f)
	{
		if (string.IsNullOrEmpty(message))
		{
			return;
		}
		int num = -1;
		if (!string.IsNullOrEmpty(key))
		{
			for (int i = 0; i < _msgLabels.Count; i++)
			{
				if (_msgLabels[i].Key == key)
				{
					num = i;
					break;
				}
			}
		}
		if (num == -1)
		{
			AlarmMessageWidget alarmMessageWidget = ((_msgLabels.Count <= 0) ? null : _msgLabels[_msgLabels.Count - 1]);
			if (alarmMessageWidget != null && alarmMessageWidget.Text == message)
			{
				num = _msgLabels.Count - 1;
			}
		}
		if (num == -1)
		{
			MessageSet item = default(MessageSet);
			item.Key = key;
			item.Text = message;
			item.Duration = duration;
			item.Scale = scale;
			_messageQueue.Enqueue(item);
			base.enabled = true;
		}
		else
		{
			_msgLabels[num].Set(key, message, duration, scale <= 0f ? 1f : scale);
			RefreshPosition();
		}
	}

	private AlarmMessageWidget MsgLabel_Pop()
	{
		AlarmMessageWidget alarmMessageWidget = ((_msgLabelPool.Count <= 0) ? _messageComponentBase.transform.parent.gameObject.AddChild(_messageComponentBase.gameObject).GetComponent<AlarmMessageWidget>() : _msgLabelPool.Pop());
		alarmMessageWidget.gameObject.SetActive(value: true);
		alarmMessageWidget.Widget.alpha = 0f;
		return alarmMessageWidget;
	}

	private void MsgLabel_Push(AlarmMessageWidget label)
	{
		label.gameObject.SetActive(value: false);
		_msgLabelPool.Push(label);
	}

	public bool IsPlaying()
	{
		return _msgLabels.Count > 0;
	}

	public void PauseToNext()
	{
		if (!_isPause)
		{
			_isPause = true;
		}
	}

	public void Resume()
	{
		if (_isPause)
		{
			_isPause = false;
		}
	}
}
