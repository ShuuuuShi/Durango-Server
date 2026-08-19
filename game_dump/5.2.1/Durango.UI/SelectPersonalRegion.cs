using System;
using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class SelectPersonalRegion : MonoBehaviour, IEditPlayerDisplayPage
{
	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private SelectableButton _button;

	[SerializeField]
	private Texture[] _regionTextures;

	private AnimationWidget _animWidget;

	public string SelectedRegionid { get; private set; }

	public event Action<string> Selected;

	public event Action Confirmed;

	private void Awake()
	{
		SelectableButton button = _button;
		button.Clicked = (Action)Delegate.Combine(button.Clicked, (Action)delegate
		{
			if (!string.IsNullOrEmpty(SelectedRegionid))
			{
				if (this.Selected != null)
				{
					this.Selected(SelectedRegionid);
				}
				if (this.Confirmed != null)
				{
					this.Confirmed();
				}
			}
		});
		ListObjectPool nodes = _scrollView.Nodes;
		nodes.BeginLoad();
		int size = KUtility.GetSize(Singleton<Constants>.Instance.PersonalRegion.RegionTemplateIds);
		for (int i = 0; i < size; i++)
		{
			GameObject next = nodes.GetNext();
			SelectableWidget component = next.GetComponent<SelectableWidget>();
			int index = i;
			component.Clicked = (Action)Delegate.Combine(component.Clicked, (Action)delegate
			{
				SelectNode(index);
			});
			next.FindComponent<UITexture>("Texture").mainTexture = _regionTextures[i];
		}
		nodes.EndLoad();
		_scrollView.ResetPosition();
		SelectNode(UnityEngine.Random.Range(0, size));
	}

	private void SelectNode(int index)
	{
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			_scrollView.Nodes.Get<SelectableWidget>(i).Selected = i == index;
		}
		List<string> regionTemplateIds = Singleton<Constants>.Instance.PersonalRegion.RegionTemplateIds;
		if (index >= 0 && index < KUtility.GetSize(regionTemplateIds))
		{
			SelectedRegionid = regionTemplateIds[index];
		}
	}

	public void Initialize(EditPlayerDisplayProxy display)
	{
		_animWidget = AnimationWidget.Get(base.gameObject, 0.3f, 0f, deactiveWhenFadeout: true);
	}

	public void Show(bool instant)
	{
		base.gameObject.SetActive(value: true);
		_animWidget.SetAlpha(1f, !instant);
		int index = Singleton<Constants>.Instance.PersonalRegion.RegionTemplateIds.IndexOf(SelectedRegionid);
		_scrollView.MoveToVisibleArea(index, instant: true);
	}

	public void Hide(bool instant)
	{
		_animWidget.SetAlpha(0f, !instant);
	}

	public Transform GetModelPosition()
	{
		return null;
	}

	public void SetConfirmText(string text)
	{
		_button.Text = text;
	}

	public void WaitForLoading(bool loading)
	{
		UIManager.ShowLoadingIcon(loading);
		_button.Disabled = loading;
	}
}
