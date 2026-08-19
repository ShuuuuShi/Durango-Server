using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class LoadingCurtainGroup : MonoBehaviour
{
	public static bool IsFirstPlayAfterCreatePlayer;

	[NonSerialized]
	public List<EventDelegate> LoadingStarted = new List<EventDelegate>();

	[NonSerialized]
	public List<EventDelegate> FadeOutStarted = new List<EventDelegate>();

	[NonSerialized]
	public List<EventDelegate> FadeOutFinished = new List<EventDelegate>();

	[SerializeField]
	private GameObject _bg;

	private readonly List<LoadingCurtainBase> _curtainList = new List<LoadingCurtainBase>();

	private bool _curtainVisible;

	public static int LoadingCount { get; private set; }

	public bool IsVisible
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		private set
		{
			base.gameObject.SetActive(value);
		}
	}

	public bool IsFadeoutStarted { get; private set; }

	private void Awake()
	{
		IsVisible = true;
		_bg.gameObject.SetActive(value: true);
		Transform transform = base.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			LoadingCurtainBase component = child.GetComponent<LoadingCurtainBase>();
			if (!(component == null))
			{
				_curtainList.Add(component);
			}
		}
	}

	private void Start()
	{
		if (!_curtainVisible)
		{
			if (IsFirstPlayAfterCreatePlayer)
			{
				IsFirstPlayAfterCreatePlayer = false;
				Show<PrologueLoadingCurtain>();
			}
			else
			{
				Show<DefaultLoadingCurtain>();
			}
		}
	}

	public T Show<T>() where T : LoadingCurtainBase
	{
		if (_curtainVisible)
		{
		}
		IsVisible = true;
		_curtainVisible = true;
		_bg.gameObject.SetActive(value: false);
		IsFadeoutStarted = false;
		T curtain = GetCurtain<T>();
		for (int i = 0; i < _curtainList.Count; i++)
		{
			_curtainList[i].gameObject.SetActive(value: false);
		}
		if ((bool)curtain)
		{
			curtain.StateChanged = OnLoadingStateChanged;
			curtain.gameObject.SetActive(value: true);
		}
		else
		{
			OnLoadingStateChanged(LoadingCurtainBase.LoadingState.Closed);
		}
		return curtain;
	}

	private T GetCurtain<T>() where T : LoadingCurtainBase
	{
		int i = 0;
		for (int count = _curtainList.Count; i < count; i++)
		{
			T val = _curtainList[i] as T;
			if (val != null)
			{
				return val;
			}
		}
		return (T)null;
	}

	private void OnLoadingStateChanged(LoadingCurtainBase.LoadingState state)
	{
		switch (state)
		{
		case LoadingCurtainBase.LoadingState.Open:
			EventDelegate.Execute(LoadingStarted);
			break;
		case LoadingCurtainBase.LoadingState.Closing:
			IsFadeoutStarted = true;
			EventDelegate.Execute(FadeOutStarted);
			break;
		case LoadingCurtainBase.LoadingState.Closed:
			IsVisible = false;
			_curtainVisible = false;
			LoadingCount++;
			Resources.UnloadUnusedAssets();
			EventDelegate.Execute(FadeOutFinished);
			break;
		}
	}
}
