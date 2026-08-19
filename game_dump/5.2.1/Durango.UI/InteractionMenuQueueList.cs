using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using UnityEngine;

namespace Durango.UI;

public class InteractionMenuQueueList : MonoBehaviour
{
	[SerializeField]
	private GameObject _baseIcon;

	private readonly Queue<InteractionQueueIconWidget> _iconPool = new Queue<InteractionQueueIconWidget>();

	private readonly List<InteractionQueueIconWidget> _icons = new List<InteractionQueueIconWidget>();

	private int _signFactor;

	public event Action IconClicked;

	private void OnEnable()
	{
		_baseIcon.SetActive(value: false);
	}

	private void OnDisable()
	{
		Clear();
	}

	public void SetList(List<Pair<int, ItemIcon>> items, int sign)
	{
		_signFactor = sign;
		if (items == null || items.Count == 0)
		{
			Clear();
			return;
		}
		for (int i = 0; i < _icons.Count; i++)
		{
			_icons[i].Index = -1;
		}
		int count = items.Count;
		for (int j = 0; j < count; j++)
		{
			if (!Find(items[j].Item1, out var icon))
			{
				icon = PopNext();
			}
			icon.Set(items[j].Item1, j, items[j].Item2);
		}
		Reposition();
	}

	public void Clear()
	{
		for (int num = _icons.Count - 1; num >= 0; num--)
		{
			RemoveItem(_icons[num]);
		}
		Reposition();
	}

	private InteractionQueueIconWidget PopNext()
	{
		InteractionQueueIconWidget interactionQueueIconWidget;
		if (_iconPool.Count > 0)
		{
			interactionQueueIconWidget = _iconPool.Dequeue();
		}
		else
		{
			GameObject obj = base.gameObject.AddChild(_baseIcon);
			interactionQueueIconWidget = obj.GetComponent<InteractionQueueIconWidget>();
			UIEventListener.Get(obj).onClick = OnIconClicked;
		}
		interactionQueueIconWidget.Reset();
		_icons.Add(interactionQueueIconWidget);
		interactionQueueIconWidget.gameObject.SetActive(value: true);
		return interactionQueueIconWidget;
	}

	private void OnIconClicked(GameObject go)
	{
		if (this.IconClicked != null)
		{
			this.IconClicked();
		}
	}

	private bool Find(int id, out InteractionQueueIconWidget icon)
	{
		for (int i = 0; i < _icons.Count; i++)
		{
			if (_icons[i].Id == id)
			{
				icon = _icons[i];
				return true;
			}
		}
		icon = null;
		return false;
	}

	private void RemoveItem(InteractionQueueIconWidget icon)
	{
		_icons.Remove(icon);
		_iconPool.Enqueue(icon);
		icon.gameObject.SetActive(value: false);
	}

	private void Reposition()
	{
		Vector3 localPosition = _baseIcon.transform.localPosition;
		localPosition.x = Mathf.Abs(localPosition.x) * (float)_signFactor;
		int width = _baseIcon.GetComponent<UIWidget>().width;
		for (int num = _icons.Count - 1; num >= 0; num--)
		{
			if (_icons[num].Index == -1)
			{
				RemoveItem(_icons[num]);
			}
		}
		int count = _icons.Count;
		for (int i = 0; i < count; i++)
		{
			InteractionQueueIconWidget interactionQueueIconWidget = _icons[i];
			Vector3 vector = localPosition + Vector3.right * _signFactor * interactionQueueIconWidget.Index * width;
			if (interactionQueueIconWidget.Index == interactionQueueIconWidget.PrevIndex)
			{
				interactionQueueIconWidget.transform.localPosition = vector;
				continue;
			}
			if (interactionQueueIconWidget.PrevIndex != -1)
			{
				TweenPosition component = interactionQueueIconWidget.GetComponent<TweenPosition>();
				if (component != null)
				{
					component.from = interactionQueueIconWidget.transform.localPosition;
					component.to = vector;
					component.tweenFactor = 0f;
					component.PlayForward();
				}
			}
			else
			{
				TweenAlpha component2 = interactionQueueIconWidget.GetComponent<TweenAlpha>();
				if (component2 != null)
				{
					component2.tweenFactor = 0f;
					component2.PlayForward();
				}
				interactionQueueIconWidget.transform.localPosition = vector;
			}
			interactionQueueIconWidget.PrevIndex = interactionQueueIconWidget.Index;
		}
	}
}
