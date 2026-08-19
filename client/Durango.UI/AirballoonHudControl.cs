using System;
using Durango.Network;
using Durango.Render.Camera;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class AirballoonHudControl : MonoBehaviour
{
	[SerializeField]
	private UISprite _crossHair;

	[SerializeField]
	private Color _normalColor = new Color(1f, 1f, 1f, 0.09f);

	[SerializeField]
	private Color _prohibitedColor = new Color(1f, 0f, 0f, 0.19f);

	[SerializeField]
	private GameObject _timer;

	[SerializeField]
	private UILabel _remainTime;

	private VehicleAirBalloon _target;

	public void Set(bool show, VehicleAirBalloon target)
	{
		_target = ((!show) ? null : target);
		if (show)
		{
			Vector3 position = target.transform.position;
			position.y = 0f;
			_crossHair.transform.localPosition = MainCamera.WorldToNGUIPos(position);
		}
		base.gameObject.SetActive(show);
	}

	private void LateUpdate()
	{
		if (!(_target == null))
		{
			Vector3 position = _target.transform.position;
			position.y = 0f;
			_crossHair.transform.localPosition = MainCamera.WorldToNGUIPos(position);
			_crossHair.color = ((!_target.OnLandingArea) ? _prohibitedColor : _normalColor);
			TimeRange boardingTime = PlayerBehavior.LocalPlayer.Display.BoardingTime;
			double num = ((!boardingTime.Until.HasValue) ? 0.0 : boardingTime.Until.Value);
			double val = num - Connections.Frontend.GetPredictedServerTime();
			val = Math.Max(0.0, val);
			bool flag = val > 0.0;
			_timer.SetActive(flag);
			if (flag)
			{
				string text = TimedeltaFormatter.Format(val);
				_remainTime.text = T._("[icon=icon_skill_time] {0} 남음", text);
			}
		}
	}
}
