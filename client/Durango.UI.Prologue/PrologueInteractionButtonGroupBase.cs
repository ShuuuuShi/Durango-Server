using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Prologue;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Prologue;

public abstract class PrologueInteractionButtonGroupBase : UIBase
{
	private const float InteractionTargetSearchPeriod = 2f;

	[SerializeField]
	private float _interactionSearchDistance;

	private bool _interactionButtonHideFlag;

	private readonly List<string> _interactionButtonHideList = new List<string>();

	private InteractionObject _selectedObject;

	private readonly List<InteractionObject> _objects = new List<InteractionObject>();

	private float _interactionTargetSearchAt;

	public bool CanInteraction { get; set; }

	protected virtual void Start()
	{
		base.VisibleController.Changed += delegate(bool visible)
		{
			ShowInteractionButton("Visible", visible);
		};
		GameSystem<InteractionSystem>.Instance().InteractionTargetSelected += OnTargetSelected;
		Singleton<BlurController>.Instance().BlurStateChanged += OnBlurStateChanged;
		ShowInteractionButton("Loading", show: false);
		UIManager.OnLoadingCurtainHidden(delegate
		{
			ShowInteractionButton("Loading", show: true);
		});
	}

	private void Update()
	{
		if (0f < _interactionTargetSearchAt && _interactionTargetSearchAt < Time.time)
		{
			SearchInteractionObjects();
		}
	}

	private bool IsInteractionButtonVisible()
	{
		return !_interactionButtonHideFlag && _interactionButtonHideList.Count == 0;
	}

	protected void OnTouchInteractionObject(InteractionObject obj)
	{
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget((_selectedObject != obj) ? obj : null);
	}

	protected virtual void OnTargetSelected(InteractionObject obj)
	{
		_selectedObject = obj;
	}

	protected abstract void SetInteractionButtons(IList<InteractionObject> list);

	private void OnBlurStateChanged(BlurController.Mask mask)
	{
		ShowInteractionButton("Blur", mask == BlurController.Mask.None);
	}

	private void AddInteractionObject(GameObject target, float limitDistance = 0f)
	{
		float distance = InteractionObject.GetDistance(target);
		if ((!(limitDistance > 0f) || !(distance > limitDistance)) && !_objects.Any((InteractionObject x) => x.Target == target))
		{
			InteractionObject interactionObject = new InteractionObject(target);
			interactionObject.LimitDistance = limitDistance;
			_objects.Add(interactionObject);
		}
	}

	private void SearchInteractionObjects()
	{
		_interactionTargetSearchAt = Time.time + 2f;
		List<GameObject> list = new List<GameObject>();
		float limitDistance = 0f;
		if (CanInteraction)
		{
			SearchInteractionsNearbyPrologue(list, SelectableObject.FindSelectable);
			limitDistance = _interactionSearchDistance;
		}
		else if (Singleton<PrologueManager>.Instance().CurrentState == PrologueManager.State.CharacterSelect)
		{
			SearchInteractionsNearbyPrologue(list, (GameObject o) => (!(o.GetComponent<TriggerPrologueSelectCharacter>() != null)) ? null : o);
			limitDistance = _interactionSearchDistance;
		}
		CheckNearInteractionObject(list, limitDistance);
		SetInteractionButtons(_objects);
	}

	private static void SearchInteractionsNearbyPrologue(List<GameObject> list, Func<GameObject, GameObject> filter)
	{
		InteractionSystem.GetNearObjectsInternal(list, LayerHelper.DefaultMask, 800f, filter);
	}

	private void CheckNearInteractionObject([NotNull] IList<GameObject> objects, float limitDistance)
	{
		int i = 0;
		for (int count = objects.Count; i < count; i++)
		{
			AddInteractionObject(objects[i], limitDistance);
		}
	}

	public static void RefreshInteractions(bool reset = false)
	{
		PrologueInteractionButtonGroupBase prologueInteractionButtonGroupBase = UIManager.FindScript<PrologueInteractionButtonGroupBase>();
		if (prologueInteractionButtonGroupBase == null)
		{
			return;
		}
		if (reset)
		{
			prologueInteractionButtonGroupBase._objects.Clear();
		}
		else
		{
			for (int num = prologueInteractionButtonGroupBase._objects.Count - 1; num >= 0; num--)
			{
				InteractionObject interactionObject = prologueInteractionButtonGroupBase._objects[num];
				if (!interactionObject.IsValid() || !(interactionObject.Distance <= interactionObject.LimitDistance))
				{
					prologueInteractionButtonGroupBase._objects.RemoveAt(num);
				}
			}
		}
		prologueInteractionButtonGroupBase.SearchInteractionObjects();
	}

	public static void ClearInteractions()
	{
		PrologueInteractionButtonGroupBase prologueInteractionButtonGroupBase = UIManager.FindScript<PrologueInteractionButtonGroupBase>();
		if (!(prologueInteractionButtonGroupBase == null))
		{
			prologueInteractionButtonGroupBase._interactionTargetSearchAt = Time.time + 2f;
			prologueInteractionButtonGroupBase._objects.Clear();
			prologueInteractionButtonGroupBase.SetInteractionButtons(prologueInteractionButtonGroupBase._objects);
		}
	}

	public static void HideInteractionButton()
	{
		PrologueInteractionButtonGroupBase prologueInteractionButtonGroupBase = UIManager.FindScript<PrologueInteractionButtonGroupBase>();
		if (!(prologueInteractionButtonGroupBase == null))
		{
			prologueInteractionButtonGroupBase._interactionButtonHideFlag = true;
		}
	}

	public static void ShowInteractionButton(string key, bool show)
	{
		PrologueInteractionButtonGroupBase prologueInteractionButtonGroupBase = UIManager.FindScript<PrologueInteractionButtonGroupBase>();
		if (prologueInteractionButtonGroupBase == null)
		{
			return;
		}
		bool flag = prologueInteractionButtonGroupBase.IsInteractionButtonVisible();
		int num = prologueInteractionButtonGroupBase._interactionButtonHideList.IndexOf(key);
		if (num == -1)
		{
			if (!show)
			{
				prologueInteractionButtonGroupBase._interactionButtonHideList.Add(key);
			}
		}
		else if (show)
		{
			prologueInteractionButtonGroupBase._interactionButtonHideList.RemoveAt(num);
		}
		if (show)
		{
			prologueInteractionButtonGroupBase._interactionButtonHideFlag = false;
		}
		bool flag2 = prologueInteractionButtonGroupBase.IsInteractionButtonVisible();
		if (flag != flag2)
		{
			if (flag2)
			{
				prologueInteractionButtonGroupBase.SearchInteractionObjects();
				return;
			}
			ClearInteractions();
			prologueInteractionButtonGroupBase._interactionTargetSearchAt = 0f;
		}
	}
}
