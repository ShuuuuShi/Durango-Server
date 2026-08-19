using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.Social;
using Durango.Player.Animation;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using Messages;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class ShopCommodityItemPreview : UIWidget
{
	[SerializeField]
	private GameObject _previewCloseButton;

	[SerializeField]
	private UIModelViewer _previewViewer;

	[SerializeField]
	private GameObject _emotionalMotionsContainer;

	[SerializeField]
	private UIWidget _emotionalMotionsListWidget;

	[SerializeField]
	private ListObjectPool _emotionalMotionsWidget;

	private string _selectedMotion;

	private readonly List<string> _emotionalMotions = new List<string>();

	private PlayerAnimationClipInfo _defaultPlayerClip;

	public event Action Closed;

	protected override void OnStart()
	{
		base.OnStart();
		if (!Application.isPlaying)
		{
			return;
		}
		UIEventListener uIEventListener = UIEventListener.Get(_previewCloseButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.Closed != null)
			{
				this.Closed();
			}
		});
		_emotionalMotionsWidget.Init(delegate(GameObject obj)
		{
			Selectable component = obj.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnMotionClick));
		});
	}

	public bool SetPreview(ContentDescription data)
	{
		_defaultPlayerClip = null;
		if (data == null)
		{
			return false;
		}
		if (data.Item.SetPreview(_previewViewer, delegate
		{
			PlayerBehavior component = _previewViewer.ModelObject.GetComponent<PlayerBehavior>();
			if (!(component == null))
			{
				_defaultPlayerClip = component.CurrentPlayerClipInfo;
			}
		}))
		{
			SetEmotionalMotions((data.Item != null) ? data.Item.EmotionalMotions : null);
			PlayMotion(null);
			return true;
		}
		if (!string.IsNullOrEmpty(data.Motion))
		{
			Durango.Logic.Social.Motion motion = GameSystem<SocialSystem>.Instance().Emotional.GetMotion(data.Motion);
			if (motion != null)
			{
				PlayerDisplay display = PlayerBehavior.LocalPlayer.Display;
				display.Equip = null;
				_previewViewer.SetPlayerModel(PlayerBehavior.LocalPlayer.IsMale, display, new UIModelViewer.Arguments
				{
					CameraAngle = 35f,
					Rotation = 140f
				});
				SetEmotionalMotions(null);
				PlayMotion(motion.Key);
				return true;
			}
		}
		return false;
	}

	private void SetEmotionalMotions(string[] motions)
	{
		_emotionalMotions.Clear();
		_emotionalMotionsWidget.BeginLoad();
		if (motions != null)
		{
			Emotional emotional = GameSystem<SocialSystem>.Instance().Emotional;
			foreach (string text in motions)
			{
				Durango.Logic.Social.Motion motion = emotional.GetMotion(text);
				if (motion != null)
				{
					_emotionalMotions.Add(text);
					GameObject next = _emotionalMotionsWidget.GetNext();
					next.transform.Find("Text").GetComponent<UILabel>().text = motion.Name;
					next.GetComponent<RectLayoutComponent>().UpdateLayout();
				}
			}
		}
		_emotionalMotionsWidget.EndLoad();
		if (_emotionalMotionsWidget.Count > 0)
		{
			_emotionalMotionsContainer.gameObject.SetActive(value: true);
			UIUtility.WidgetsReposition(_emotionalMotionsWidget, _emotionalMotionsListWidget, Vector3.right, 5f);
		}
		else
		{
			_emotionalMotionsContainer.gameObject.SetActive(value: false);
		}
	}

	private void OnMotionClick()
	{
		int num = _emotionalMotionsWidget.IndexOf(Selectable.Current.gameObject);
		if (num != -1)
		{
			string text = _emotionalMotions[num];
			if (_selectedMotion == text && _defaultPlayerClip != null)
			{
				text = null;
			}
			PlayMotion(text);
		}
	}

	private void PlayMotion(string m)
	{
		_selectedMotion = m;
		Durango.Logic.Social.Motion motion = GameSystem<SocialSystem>.Instance().Emotional.GetMotion(m);
		if (motion == null)
		{
			_selectedMotion = null;
			if (_defaultPlayerClip != null)
			{
				PlayerBehavior component = _previewViewer.ModelObject.GetComponent<PlayerBehavior>();
				if (component != null)
				{
					component.PlayMotionForcely(_defaultPlayerClip.Clip);
				}
			}
		}
		else
		{
			PlayerBehavior component2 = _previewViewer.ModelObject.GetComponent<PlayerBehavior>();
			if (component2 != null)
			{
				component2.PlayMotionForcely(motion.MotionNames.Random());
			}
		}
		for (int i = 0; i < _emotionalMotionsWidget.Count; i++)
		{
			_emotionalMotionsWidget[i].GetComponent<Selectable>().Selected = _emotionalMotions[i] == _selectedMotion;
		}
	}
}
