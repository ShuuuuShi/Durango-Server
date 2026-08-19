using System;
using UnityEngine;

namespace Durango.UI;

public abstract class AlarmNotifyQueueBase : MonoBehaviour
{
	protected struct AlarmStruct
	{
		public string Key;

		public string Text;

		public string Icon;

		public Color32 IconColor;

		public bool IsPortrait;

		public PortraitBuilder.Argument Portrait;

		public float Duration;

		public Action ViewmoreAction;
	}

	[SerializeField]
	protected AlarmNotifyWidget _baseAlarmWidget;

	[SerializeField]
	protected Texture _portraitMask;

	public abstract bool HasAlarm(string key);

	public abstract void ShowAlarm(string key, string text, PortraitBuilder.Argument arg, float duration, Action viewMoreAction);

	public abstract void ShowAlarm(string key, string text, string icon, Color32 iconColor, float duration, Action viewMoreAction);

	public abstract void HideAlarm(string key);

	public abstract void ClearAlarms();
}
