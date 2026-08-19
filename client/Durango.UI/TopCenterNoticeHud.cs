using System;
using Durango.Logic;
using Durango.Network;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TopCenterNoticeHud : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UIWidget _landscapeWidget;

	[SerializeField]
	private UIWidget _portraitWidget;

	public event Action PositionChanged;

	void IUIInitializable.Init()
	{
		UIManager.AddOnScreenResized(OnScreenResized);
		Hide();
	}

	public void Hide()
	{
		_landscapeWidget.gameObject.SetActive(value: false);
		_portraitWidget.gameObject.SetActive(value: false);
	}

	[ExposedInEditor(null)]
	private void ShowTest()
	{
		Show(Connections.Frontend.GetPredictedServerTime() + 321.0);
	}

	public void Show()
	{
		Show(GameSystem<PvpIslandSystem>.Instance().TimeInfo.GameStartAt);
	}

	public void Show(double at)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (at > predictedServerTime)
		{
			UpdateLayout();
			_label.SetText(new SyncString(delegate(out string text, out float period)
			{
				double num = at - Connections.Frontend.GetPredictedServerTime();
				if (num > 0.0)
				{
					text = GetCountdownText(num);
					period = (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
				}
				else
				{
					text = string.Empty;
					period = 0f;
					Hide();
				}
			}));
		}
		else
		{
			Hide();
		}
	}

	private void OnScreenResized()
	{
		if ((_portraitWidget.gameObject.activeSelf && !UIManager.IsPortraitScreen) || (_landscapeWidget.gameObject.activeSelf && UIManager.IsPortraitScreen))
		{
			Show();
			if (this.PositionChanged != null)
			{
				this.PositionChanged();
			}
		}
	}

	private void UpdateLayout()
	{
		_portraitWidget.gameObject.SetActive(UIManager.IsPortraitScreen);
		_landscapeWidget.gameObject.SetActive(!UIManager.IsPortraitScreen);
		_label.text = GetCountdownText(3599.0);
		UIWidget uIWidget = ((!UIManager.IsPortraitScreen) ? _landscapeWidget : _portraitWidget);
		_label.transform.parent = uIWidget.transform;
		uIWidget.width = (int)(_label.printedSize.x + 20f);
		_label.pivot = ((!UIManager.IsPortraitScreen) ? UIWidget.Pivot.Center : UIWidget.Pivot.Left);
		UIUtility.UpdateAnchors(base.transform);
		_label.transform.localPosition = ((!UIManager.IsPortraitScreen) ? Vector3.zero : new Vector3(10f, 0f));
	}

	private static string GetCountdownText(double d)
	{
		return T._("난투 시작까지 <em>{0}</em> 남음", TimedeltaFormatter.Format(d));
	}
}
