using System;
using Building;
using Durango.Logic.Timeline;
using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class TimelineLog : UIWidget
{
	[Serializable]
	private struct IconWidgetStruct
	{
		[SerializeField]
		public UIWidget Widget;

		[SerializeField]
		public UITexture PortraitTexture;

		[SerializeField]
		public UISprite IconSprite;

		[SerializeField]
		public UISprite IconBgSprite;
	}

	[SerializeField]
	private UIWidget _commentWidget;

	[SerializeField]
	private UIWidget _timeWidget;

	[SerializeField]
	private GameObject _iconArrow;

	[SerializeField]
	private IconWidgetStruct[] _iconWidgets;

	[SerializeField]
	private Texture2D _portraitMask;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private UILabel _timeLabel;

	[SerializeField]
	private Color _colorPositive;

	[SerializeField]
	private Color _colorNegative;

	[SerializeField]
	private RectLayout _layout;

	private bool _isInit;

	private Vector3[] _iconWidgetsPos;

	private DelayedFunction _updateLayout;

	private bool _isLoaded;

	public void SetLog(TimelineLogBuilder logBuilder)
	{
		Init();
		_commentWidget.alpha = 0f;
		_timeWidget.alpha = 0f;
		_isLoaded = false;
		_timeLabel.text = $"{Times.Timeago(logBuilder.At)}";
		logBuilder.Build(OnParamLoaded);
		_updateLayout.Call(this);
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_iconWidgetsPos = new Vector3[_iconWidgets.Length];
			for (int i = 0; i < _iconWidgetsPos.Length; i++)
			{
				ref Vector3 reference = ref _iconWidgetsPos[i];
				reference = _iconWidgets[i].Widget.transform.localPosition;
			}
			_updateLayout = new DelayedFunction(UpdateLayout);
		}
	}

	private void OnParamLoaded(TimelineLogBuilder logBuilder)
	{
		SetPortrait(_iconWidgets[0], logBuilder.AgentPlayer);
		if (logBuilder.TargetPlayer != null)
		{
			SetPortrait(_iconWidgets[1], logBuilder.TargetPlayer);
		}
		else
		{
			SetArtifact(_iconWidgets[1], logBuilder.Blueprint, logBuilder.IsNegative());
		}
		_commentLabel.text = logBuilder.Text;
		_isLoaded = true;
		_updateLayout.Call(this);
	}

	private void SetPortrait(IconWidgetStruct comp, [CanBeNull] PlayerInfo playerInfo)
	{
		if (playerInfo == null || !playerInfo.Valid)
		{
			comp.Widget.gameObject.SetActive(value: false);
			return;
		}
		comp.Widget.gameObject.SetActive(value: true);
		comp.PortraitTexture.gameObject.SetActive(value: true);
		comp.IconSprite.gameObject.SetActive(value: false);
		comp.IconBgSprite.gameObject.SetActive(value: false);
		PortraitBuilder.Argument portraitArgument = playerInfo.GetPortraitArgument();
		portraitArgument.Mask = _portraitMask;
		PortraitBuilder.Set(portraitArgument, comp.PortraitTexture);
	}

	private void SetArtifact(IconWidgetStruct comp, [CanBeNull] Blueprint blueprint, bool negative)
	{
		if (blueprint == null)
		{
			comp.Widget.gameObject.SetActive(value: false);
			return;
		}
		comp.Widget.gameObject.SetActive(value: true);
		comp.PortraitTexture.gameObject.SetActive(value: false);
		comp.IconSprite.gameObject.SetActive(value: true);
		comp.IconBgSprite.gameObject.SetActive(value: true);
		comp.IconBgSprite.color = ((!negative) ? _colorPositive : _colorNegative);
		comp.IconSprite.spriteName = blueprint.Icon;
	}

	private void UpdateLayout()
	{
		if (!_isLoaded)
		{
			return;
		}
		_commentWidget.alpha = 1f;
		_timeWidget.alpha = 1f;
		_layout.UpdateLayout();
		int num = 0;
		for (int i = 0; i < _iconWidgets.Length; i++)
		{
			if (_iconWidgets[i].Widget.gameObject.activeSelf)
			{
				_iconWidgets[i].Widget.transform.localPosition = _iconWidgetsPos[num++];
			}
		}
		_iconArrow.gameObject.SetActive(num >= 2);
		int num2 = -_commentLabel.rightAnchor.absolute;
		if (num == 0)
		{
			_commentLabel.leftAnchor.absolute = num2;
		}
		else
		{
			UIWidget widget = _iconWidgets[num - 1].Widget;
			_commentLabel.leftAnchor.absolute = (int)widget.GetPosition(1f, 1f).x + num2;
		}
		_commentLabel.UpdateAnchors();
		_timeLabel.UpdateAnchors();
	}
}
