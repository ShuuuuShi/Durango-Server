using UnityEngine;

public class PlayerHudGroup : UIBase
{
	[SerializeField]
	private HyperGaugeViewer _lifeViewer;

	[SerializeField]
	private HyperGaugeViewer _energyViewer;

	[SerializeField]
	private AnimationWidget _selectedBox;

	[SerializeField]
	private StatusEffectsControl _statusEffectsControl;

	[SerializeField]
	private ExpGauge _expBar;

	private GameObject _nextVisibleTooltipHudGauge;

	private void Start()
	{
		GameObject[] array = (GameObject[])(object)new GameObject[2]
		{
			((Component)_lifeViewer).gameObject,
			((Component)_energyViewer).gameObject
		};
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UIEventListener.Get(array[i]).onClick = OnClickHudGauge;
		}
		PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated += LocalPlayer_SurvivalGaugeUpdated;
	}

	private void LocalPlayer_SurvivalGaugeUpdated(CharacterBehavior character)
	{
		SetLife(character.GetGauge("life"));
		Gauge gauge = character.GetGauge("stamina");
		SetEnergy((gauge == null) ? character.GetGauge("energy") : gauge);
	}

	public void ShowExpBar(bool show)
	{
		if (!UIManager.IsPortraitMode && Object.op_Implicit((Object)(object)_expBar))
		{
			_expBar.Show(show);
		}
	}

	private void SetLife(Gauge gauge)
	{
		_lifeViewer.Set(gauge);
		_lifeViewer.ToIntFunction = Mathf.FloorToInt;
	}

	private void SetEnergy(Gauge gauge)
	{
		_energyViewer.Set(gauge);
		_energyViewer.ToIntFunction = Mathf.FloorToInt;
	}

	private void ResetNextvisibleHudGaugetooltip()
	{
		_nextVisibleTooltipHudGauge = null;
	}

	private void OnClickHudGauge(GameObject go)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_nextVisibleTooltipHudGauge != (Object)null)
		{
			go = _nextVisibleTooltipHudGauge;
		}
		string format = "#survival_tooltip_title_{0}";
		string format2 = "#survival_tooltip_{0}";
		string text = null;
		if ((Object)(object)go == (Object)(object)((Component)_lifeViewer).gameObject)
		{
			text = "life_health";
			_nextVisibleTooltipHudGauge = ((Component)_energyViewer).gameObject;
		}
		else
		{
			if (!((Object)(object)go == (Object)(object)((Component)_energyViewer).gameObject))
			{
				return;
			}
			text = "stamina_energy";
			_nextVisibleTooltipHudGauge = ((Component)_lifeViewer).gameObject;
		}
		((MonoBehaviour)this).CancelInvoke("ResetNextvisibleHudGaugetooltip");
		((MonoBehaviour)this).Invoke("ResetNextvisibleHudGaugetooltip", 5f);
		format = LocalizeSystem.Get(string.Format(format, text));
		format2 = LocalizeSystem.Get(string.Format(format2, text));
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Sign = 1;
		widgetTooltipControl.Set(format, format2);
		widgetTooltipControl.Show(go.GetComponent<UIWidget>(), Vector2.right * 10f, 5f);
		ShowSelectedBox(go);
	}

	private void ShowSelectedBox(GameObject go)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider component = go.GetComponent<BoxCollider>();
		Vector3 localPosition = go.transform.localPosition + component.center;
		((Component)_selectedBox).transform.localPosition = localPosition;
		_selectedBox.Widget.width = (int)component.size.x + 10;
		_selectedBox.Widget.height = (int)component.size.y + 10;
		((Component)_selectedBox).gameObject.SetActive(true);
		_selectedBox.SetAlpha(1f, useTween: false);
		_selectedBox.SetAlpha(0f);
		TweenScale component2 = ((Component)_selectedBox).GetComponent<TweenScale>();
		Vector3 one = Vector3.one;
		one.x = 1f - 10f / (float)_selectedBox.Widget.width;
		one.y = 1f - 10f / (float)_selectedBox.Widget.height;
		component2.from = one;
		component2.tweenFactor = 0f;
		component2.PlayForward();
	}
}
