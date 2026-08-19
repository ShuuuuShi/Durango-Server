using Durango.Network;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ClanBattleCycleWidget : UIWidget
{
	[SerializeField]
	private UILabel _currentCycleLabel;

	[SerializeField]
	private UILabel _nextCycleLabel;

	[SerializeField]
	private UISprite _cycleTermSprite;

	[SerializeField]
	private UISprite _currentCursor;

	[SerializeField]
	private UILabel _timerLabel;

	private ListObjectPool<UISprite> _cycleTerms;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_cycleTerms = new ListObjectPool<UISprite>();
			_cycleTerms.BaseObject = _cycleTermSprite;
			_cycleTerms.UseBase = true;
		}
	}

	public void Set(ClanCargoWarphole data)
	{
		ClanBattle? battleInfo = data.BattleInfo;
		if (!battleInfo.HasValue)
		{
			return;
		}
		Init();
		_currentCycleLabel.text = data.BattleInfo.Value.Cycle.ToString();
		_nextCycleLabel.text = (data.BattleInfo.Value.Cycle + 1).ToString();
		Vector3 vector = localCorners[0] + new Vector3(40f, 66f);
		Vector3 pos = vector;
		pos.x = localCorners[3].x - 40f;
		_currentCycleLabel.SetPosition(vector, 0f, 0.5f);
		_nextCycleLabel.SetPosition(pos, 1f, 0.5f);
		int num = Mathf.Max(1, (int)OptionSystem.GetClanBattleCycleRepeat());
		Vector3 basePos = vector + Vector3.right * ((float)_currentCycleLabel.width + 20f);
		_cycleTerms.Set(num);
		int num2 = (int)(pos.x - vector.x - (float)_currentCycleLabel.width - (float)_nextCycleLabel.width - 40f);
		int num3 = (num2 - (num - 1) * 2) / num;
		float margin = ((num <= 1) ? 0f : ((float)(num2 - num3 * num) / (float)(num - 1)));
		for (int i = 0; i < _cycleTerms.Count; i++)
		{
			_cycleTerms[i].type = UIBasicSprite.Type.Simple;
			_cycleTerms[i].width = num3;
		}
		UIUtility.WidgetsReposition(_cycleTerms, Vector3.right, basePos, margin);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		int num4 = Mathf.RoundToInt((float)((data.BattleInfo.Value.CycleProtectionUntil - data.BattleInfo.Value.CycleSince) / (data.BattleInfo.Value.CycleUntil - data.BattleInfo.Value.CycleSince)) * (float)num);
		float a = (float)((predictedServerTime - data.BattleInfo.Value.CycleSince) / (data.BattleInfo.Value.CycleUntil - data.BattleInfo.Value.CycleSince)) * (float)num;
		a = Mathf.Min(a, num);
		bool flag = predictedServerTime > data.BattleInfo.Value.CycleProtectionUntil;
		for (int j = 0; j < _cycleTerms.Count; j++)
		{
			_cycleTerms[j].color = ((j >= num4) ? new Color(0.73f, 0.18f, 0.18f, 0.3f) : new Color(1f, 1f, 1f, 0.3f));
		}
		int k = 0;
		for (int num5 = Mathf.CeilToInt(a); k < num5; k++)
		{
			UISprite uISprite = _cycleTerms[k];
			UISprite uISprite2 = _cycleTerms.Add();
			uISprite2.width = uISprite.width;
			uISprite2.transform.localPosition = uISprite.transform.localPosition;
			uISprite2.color = ((k >= num4) ? new Color(0.73f, 0.18f, 0.18f) : PresetColor.UIYellow);
			float num6 = a - (float)k;
			if (num6 < 1f)
			{
				uISprite2.type = UIBasicSprite.Type.Filled;
				uISprite2.fillDirection = UIBasicSprite.FillDirection.Horizontal;
				uISprite2.fillAmount = num6;
			}
			else
			{
				uISprite2.type = UIBasicSprite.Type.Simple;
			}
		}
		double seconds = ((!flag) ? (predictedServerTime - data.BattleInfo.Value.CycleSince) : (predictedServerTime - data.BattleInfo.Value.CycleProtectionUntil));
		_timerLabel.text = T._("{0} 경과", TimedeltaFormatter.Format(seconds));
		if (flag)
		{
			_timerLabel.color = new Color(0.73f, 0.18f, 0.18f);
			_currentCursor.color = new Color(0.73f, 0.18f, 0.18f);
		}
		else
		{
			_timerLabel.color = PresetColor.UIYellow;
			_currentCursor.color = PresetColor.UIYellow;
		}
		_currentCursor.SetPosition(_cycleTerms[(int)a].GetPosition(a % 1f, 0f) + new Vector3(0f, -5f), 0.5f, 1f);
		Vector3 pos2 = _currentCursor.GetPosition(0.5f, 0f) + new Vector3(0f, -10f);
		pos2.x = Mathf.Clamp(pos2.x, localCorners[0].x + 40f + (float)_timerLabel.width * 0.5f + 10f, localCorners[3].x - (40f + (float)_timerLabel.width * 0.5f + 10f));
		_timerLabel.SetPosition(pos2, 0.5f, 1f);
	}
}
