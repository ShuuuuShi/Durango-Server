using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class InteractionHelperList : MonoBehaviour, IUIInitializable
{
	protected ListObjectPool<InteractionHelperLabel> Helpers;

	[SerializeField]
	private InteractionHelperLabel _baseHelper;

	[SerializeField]
	protected float RefreshPeriod;

	protected readonly List<GameObject> ObjectBuffer = new List<GameObject>();

	private AnimationWidget _animWidget;

	private InteractionObject _selectedObject;

	protected float RefreshAt;

	public bool IsShow { get; private set; }

	public event Action ShowStateChanged;

	public virtual void Init()
	{
		Helpers = new ListObjectPool<InteractionHelperLabel>();
		Helpers.BaseObject = _baseHelper;
		Helpers.UseBase = true;
		Helpers.Init(OnInitHelperLabel);
		base.gameObject.SetActive(value: false);
		_animWidget = GetComponent<AnimationWidget>();
		GameSystem<InteractionSystem>.Instance().InteractionTargetSelected += OnSelectInteractionTarget;
		GameSystem<PartySystem>.Instance().MembersUpdated += PartySystem_MembersUpdated;
	}

	protected virtual void LateUpdate()
	{
		if (IsShow && RefreshAt < Time.time)
		{
			RefreshHelpers();
		}
		UpdateLabels();
	}

	private void OnInitHelperLabel(InteractionHelperLabel lb)
	{
		lb.Clicked = delegate
		{
			if (!InputSystem.IsMouseButtonReversed)
			{
				OnClickHelperLabel();
			}
		};
		lb.RightClicked = delegate
		{
			if (InputSystem.IsMouseButtonReversed)
			{
				OnClickHelperLabel();
			}
		};
		InteractionHelperLabel interactionHelperLabel = lb;
		interactionHelperLabel.OnHovered = (Action<bool>)Delegate.Combine(interactionHelperLabel.OnHovered, (Action<bool>)delegate(bool isHovered)
		{
			GameCursorUtil.ChangeGameCursor(lb.Target, isHovered);
		});
	}

	protected virtual void OnClickHelperLabel()
	{
		Vector3 vector = NGUIMath.ScreenToParentPixels(UICamera.currentTouch.pos, _baseHelper.transform);
		InteractionHelperLabel interactionHelperLabel = null;
		float num = float.MaxValue;
		for (int i = 0; i < Helpers.Count; i++)
		{
			Vector3 localPosition = Helpers[i].transform.localPosition;
			float sqrMagnitude = (localPosition - vector).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				interactionHelperLabel = Helpers[i];
				num = sqrMagnitude;
			}
		}
		if (!(interactionHelperLabel == null) && !(interactionHelperLabel.Target == null))
		{
			Singleton<PlayerController>.Instance().StopMove();
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(new InteractionObject(interactionHelperLabel.Target));
		}
	}

	public void Show()
	{
		if (!IsShow)
		{
			IsShow = true;
			Helpers.Clear();
			base.gameObject.SetActive(value: true);
			_animWidget.SetAlpha(1f, useTween: false);
			if (this.ShowStateChanged != null)
			{
				this.ShowStateChanged();
			}
		}
		RefreshHelpers();
	}

	public void Hide()
	{
		if (IsShow)
		{
			IsShow = false;
			_animWidget.Alpha = 0f;
			if (this.ShowStateChanged != null)
			{
				this.ShowStateChanged();
			}
		}
	}

	protected virtual void RefreshHelpers()
	{
		RefreshAt = Time.time + RefreshPeriod;
		List<GameObject> list = UpdateObjectBuffer();
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = list[i];
			int num = -1;
			for (int j = 0; j < Helpers.Count; j++)
			{
				if (Helpers[j].Target == gameObject)
				{
					num = j;
					break;
				}
			}
			if (num == -1)
			{
				InteractionHelperLabel interactionHelperLabel = Helpers.Add();
				interactionHelperLabel.gameObject.SetActive(value: true);
				interactionHelperLabel.enabled = true;
				interactionHelperLabel.Widget.alpha = 0f;
				float magnitude = (PlayerBehavior.LocalPlayer.CurrentPosition - list[i].transform.position).magnitude;
				interactionHelperLabel.TweenAlphaDelta = ((!(magnitude > 0f)) ? 0f : (800f / magnitude));
				interactionHelperLabel.Set(list[i]);
				interactionHelperLabel.IsShow = true;
			}
			else
			{
				Helpers[num].IsShow = true;
			}
		}
	}

	protected List<GameObject> UpdateObjectBuffer()
	{
		List<GameObject> objectBuffer = ObjectBuffer;
		objectBuffer.Clear();
		InteractionSystem.SearchMovableObjects(objectBuffer);
		InteractionSystem.SearchPropObjects(objectBuffer);
		for (int i = 0; i < Helpers.Count; i++)
		{
			Helpers[i].IsShow = false;
		}
		Driver driver = PlayerBehavior.LocalPlayer.Driver;
		GameObject gameObject = ((!driver.IsRiding || !(driver.Vehicle != null)) ? null : driver.Vehicle.gameObject);
		GameObject gameObject2 = ((_selectedObject == null) ? null : _selectedObject.Target);
		int num = (byte)PlayerBehavior.LocalPlayer.Floor;
		for (int num2 = objectBuffer.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject3 = objectBuffer[num2];
			if (gameObject3 == gameObject2 || gameObject3 == gameObject)
			{
				objectBuffer.RemoveAt(num2);
			}
			else
			{
				ImmovableBase component = gameObject3.GetComponent<ImmovableBase>();
				if (component != null && num != component.Floor.GetValueOrDefault())
				{
					objectBuffer.RemoveAt(num2);
				}
			}
		}
		return objectBuffer;
	}

	private void UpdateLabels()
	{
		for (int i = 0; i < Helpers.Count; i++)
		{
			InteractionHelperLabel interactionHelperLabel = Helpers[i];
			interactionHelperLabel.UpdatePosition();
			float alpha = interactionHelperLabel.Widget.alpha;
			if (interactionHelperLabel.IsShow)
			{
				if (alpha < 1f)
				{
					float num = interactionHelperLabel.TweenAlphaDelta * Time.deltaTime;
					interactionHelperLabel.Widget.alpha = ((!(num > 0f)) ? 1f : Mathf.Clamp01(alpha + num));
				}
			}
			else if (alpha > 0f)
			{
				float num2 = Time.deltaTime / _animWidget.Duration;
				interactionHelperLabel.Widget.alpha = Mathf.Clamp01(alpha - num2);
			}
			else
			{
				int num3 = Helpers.Count - 1;
				Helpers.Swap(num3, i);
				Helpers.Set(num3);
				i--;
			}
		}
	}

	private void OnSelectInteractionTarget(InteractionObject obj)
	{
		_selectedObject = obj;
		if (IsShow)
		{
			RefreshHelpers();
		}
	}

	private void PartySystem_MembersUpdated()
	{
		if (IsShow)
		{
			for (int i = 0; i < Helpers.Count; i++)
			{
				InteractionHelperLabel interactionHelperLabel = Helpers[i];
				interactionHelperLabel.UpdateContents();
			}
		}
	}
}
