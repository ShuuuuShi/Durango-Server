using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class PlayerHudGroup : PlayerHudGroupBase
{
	private GameObject _nextVisibleGaugeTooltip;

	private float _lastGaugeTooltipTime;

	[SerializeField]
	private AnimationWidget _selectedBox;

	protected override void Start()
	{
		GameObject[] array = new GameObject[3] { _lifeViewer.gameObject, _energyViewer.gameObject, _fatigueGauge.gameObject };
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UIEventListener.Get(array[i]).onClick = OnClickHudGauge;
		}
		base.Start();
	}

	protected void OnClickHudGauge(GameObject go)
	{
		float time = Time.time;
		if (_nextVisibleGaugeTooltip != null && time <= _lastGaugeTooltipTime + 5f)
		{
			go = _nextVisibleGaugeTooltip;
		}
		_lastGaugeTooltipTime = Time.time;
		if (go == _lifeViewer.gameObject)
		{
			_nextVisibleGaugeTooltip = _energyViewer.gameObject;
		}
		else if (go == _energyViewer.gameObject)
		{
			_nextVisibleGaugeTooltip = _fatigueGauge.gameObject;
		}
		else if (go == _fatigueGauge.gameObject)
		{
			_nextVisibleGaugeTooltip = _lifeViewer.gameObject;
		}
		ShowGaugeTooltip(go, 5f);
		ShowSelectedBox(go);
	}

	private void ShowSelectedBox(GameObject go)
	{
		BoxCollider component = go.GetComponent<BoxCollider>();
		Vector3 localPosition = go.transform.localPosition + component.center;
		_selectedBox.transform.localPosition = localPosition;
		_selectedBox.Widget.width = (int)component.size.x + 10;
		_selectedBox.Widget.height = (int)component.size.y + 10;
		_selectedBox.gameObject.SetActive(value: true);
		_selectedBox.SetAlpha(1f, useTween: false);
		_selectedBox.SetAlpha(0f);
		TweenScale component2 = _selectedBox.GetComponent<TweenScale>();
		Vector3 one = Vector3.one;
		one.x = 1f - 10f / (float)_selectedBox.Widget.width;
		one.y = 1f - 10f / (float)_selectedBox.Widget.height;
		component2.from = one;
		component2.tweenFactor = 0f;
		component2.PlayForward();
	}
}
