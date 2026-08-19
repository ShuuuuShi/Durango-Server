using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class InteractionButtonGroup : UIBase
{
	private const float InteractionTargetSearchPeriod = 2f;

	public Func<KeyValuePair<IList<GameObject>, float>> InteractionSearchOverride;

	[SerializeField]
	private InteractionButtonControl _buttonControl;

	private bool _interactionButtonHideFlag;

	private readonly List<string> _interactionButtonHideList = new List<string>();

	private InteractionObject _selectedObject;

	private readonly List<InteractionObject> _objects = new List<InteractionObject>();

	private readonly List<GameObject> _searchObjectBuffer = new List<GameObject>();

	private float _interactionTargetSearchAt;

	public static bool IsProhibitInteraction;

	public static bool IgnoreAnimals;

	private void Start()
	{
		IsProhibitInteraction = false;
		base.OnVisible += delegate(bool visible)
		{
			ShowInteractionButton("Visible", visible);
		};
		_buttonControl.InteractionClicked += OnTouchInteractionObject;
		KSingleton<PlayerController>.Instance().MoveStarted += OnStartMove;
		KSingleton<PlayerController>.Instance().MoveEnded += OnEndMove;
		GameSystem<InteractionSystem>.Instance().InteractionTargetSelected += OnTargetSelected;
		KSingleton<BlurController>.Instance().BlurStateChanged += OnBlurStateChanged;
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

	private void OnTouchInteractionObject(InteractionObject obj)
	{
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget((_selectedObject != obj) ? obj : null);
	}

	private void OnTargetSelected(InteractionObject obj)
	{
		_selectedObject = obj;
		_buttonControl.UnselectAnimation();
		_buttonControl.SelectAnimation(_selectedObject);
	}

	private void SetInteractionButtons(IList<InteractionObject> list)
	{
		_buttonControl.SetInteractionButtons(list);
	}

	private void OnStartMove()
	{
		if (!IsProhibitInteraction)
		{
			ShowInteractionButton("Moving", show: false);
		}
	}

	private void OnEndMove()
	{
		if (!IsProhibitInteraction)
		{
			RefreshInteractions();
			ShowInteractionButton("Moving", show: true);
		}
	}

	private void OnBlurStateChanged(BlurController.Mask mask)
	{
		ShowInteractionButton("Blur", mask == BlurController.Mask.None);
	}

	private int InteractionObjectIndexOf(GameObject obj)
	{
		int result = -1;
		int i = 0;
		for (int count = _objects.Count; i < count; i++)
		{
			if ((Object)(object)_objects[i].Target == (Object)(object)obj)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void AddInteractionObject(GameObject target, float limitDistance = 0f)
	{
		float distance = InteractionObject.GetDistance(target);
		if ((!(limitDistance > 0f) || !(distance > limitDistance)) && InteractionObjectIndexOf(target) == -1)
		{
			InteractionObject interactionObject = new InteractionObject(target);
			interactionObject.LimitDistance = limitDistance;
			_objects.Add(interactionObject);
		}
	}

	private void SearchInteractionObjects()
	{
		_interactionTargetSearchAt = Time.time + 2f;
		if (InteractionSearchOverride != null)
		{
			KeyValuePair<IList<GameObject>, float> keyValuePair = InteractionSearchOverride();
			CheckNearInteractionObject(keyValuePair.Key, keyValuePair.Value);
		}
		else if (PlayerBehavior.LocalPlayer.IsAlive)
		{
			RefreshPropObjects();
			if (!IgnoreAnimals)
			{
				RefreshMovableObjects();
			}
		}
		else
		{
			_objects.Clear();
		}
		SetInteractionButtons(_objects);
	}

	private void RefreshPropObjects()
	{
		_searchObjectBuffer.Clear();
		InteractionSystem.SearchPropObjects(_searchObjectBuffer);
		CheckNearInteractionObject(_searchObjectBuffer, 800f);
	}

	private void RefreshMovableObjects()
	{
		_searchObjectBuffer.Clear();
		InteractionSystem.SearchMovableObjects(_searchObjectBuffer);
		CheckNearInteractionObject(_searchObjectBuffer, 2000f);
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
		InteractionButtonGroup interactionButtonGroup = UIManager.FindScript<InteractionButtonGroup>();
		if ((Object)(object)interactionButtonGroup == (Object)null)
		{
			return;
		}
		if (reset)
		{
			interactionButtonGroup._objects.Clear();
		}
		else
		{
			for (int num = interactionButtonGroup._objects.Count - 1; num >= 0; num--)
			{
				InteractionObject interactionObject = interactionButtonGroup._objects[num];
				if (!interactionObject.IsValid() || !(interactionObject.Distance <= interactionObject.LimitDistance))
				{
					interactionButtonGroup._objects.RemoveAt(num);
				}
			}
		}
		interactionButtonGroup.SearchInteractionObjects();
	}

	public static void ClearInteractions()
	{
		InteractionButtonGroup interactionButtonGroup = UIManager.FindScript<InteractionButtonGroup>();
		if (!((Object)(object)interactionButtonGroup == (Object)null))
		{
			interactionButtonGroup._interactionTargetSearchAt = Time.time + 2f;
			interactionButtonGroup._objects.Clear();
			interactionButtonGroup.SetInteractionButtons(interactionButtonGroup._objects);
		}
	}

	public static void HideInteractionButton()
	{
		InteractionButtonGroup interactionButtonGroup = UIManager.FindScript<InteractionButtonGroup>();
		if (!((Object)(object)interactionButtonGroup == (Object)null))
		{
			interactionButtonGroup._interactionButtonHideFlag = true;
		}
	}

	public static void ShowInteractionButton(string key, bool show)
	{
		InteractionButtonGroup interactionButtonGroup = UIManager.FindScript<InteractionButtonGroup>();
		if ((Object)(object)interactionButtonGroup == (Object)null)
		{
			return;
		}
		bool flag = interactionButtonGroup.IsInteractionButtonVisible();
		int num = interactionButtonGroup._interactionButtonHideList.IndexOf(key);
		if (num == -1)
		{
			if (!show)
			{
				interactionButtonGroup._interactionButtonHideList.Add(key);
			}
		}
		else if (show)
		{
			interactionButtonGroup._interactionButtonHideList.RemoveAt(num);
		}
		if (show)
		{
			interactionButtonGroup._interactionButtonHideFlag = false;
		}
		bool flag2 = interactionButtonGroup.IsInteractionButtonVisible();
		if (flag != flag2)
		{
			if (flag2)
			{
				interactionButtonGroup.SearchInteractionObjects();
				return;
			}
			ClearInteractions();
			interactionButtonGroup._interactionTargetSearchAt = 0f;
		}
	}
}
