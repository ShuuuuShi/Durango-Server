using System.Text;
using Messages;
using Shared.Battle;
using UnityEngine;

public class DamageRecorder
{
	private enum DamageRecordType
	{
		Damage,
		AttackCount,
		CriticalCount,
		MissCount,
		BlowCount,
		FlinchCount,
		Count
	}

	private float _combatStartTime;

	private readonly int[] _targetDamageRecord = new int[6];

	public void Start()
	{
		_combatStartTime = Time.time;
		int i = 0;
		for (int num = _targetDamageRecord.Length; i < num; i++)
		{
			_targetDamageRecord[i] = 0;
		}
	}

	public void Add(Damaged msg)
	{
		if (msg.AttackerId != PlayerBehavior.LocalPlayer.EntityId)
		{
			return;
		}
		Damage damage = msg.Damage;
		_targetDamageRecord[0] += damage.Value;
		_targetDamageRecord[1]++;
		if (damage.Value > 0)
		{
			switch (damage.Effects)
			{
			case DamageEffects.Critical:
				_targetDamageRecord[2]++;
				break;
			case DamageEffects.KnockBack:
				_targetDamageRecord[5]++;
				break;
			case DamageEffects.Blow:
				_targetDamageRecord[4]++;
				break;
			case DamageEffects.Critical | DamageEffects.KnockBack:
				break;
			}
		}
		else
		{
			_targetDamageRecord[3]++;
		}
	}

	private static string DamageRecordToString(int[] record, float startTime)
	{
		int num = record[0];
		float num2 = Time.time - startTime;
		int num3 = record[1];
		int num4 = record[3];
		int num5 = record[2];
		float num6 = (float)num / num2;
		float num7 = (float)(num3 - num4) / (float)num3;
		float num8 = (float)num5 / (float)num3;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(num).Append(" damage / ");
		stringBuilder.Append(num2.ToString("0.0")).Append(" sec").AppendLine();
		stringBuilder.Append(num3).Append(" atk, ");
		stringBuilder.Append(num4).Append(" miss, ");
		stringBuilder.Append(num5).Append(" crit").AppendLine();
		stringBuilder.Append("DPS: ").Append(num6.ToString("0.0"));
		stringBuilder.Append(" / Hit: ").Append(num7.ToString("P1"));
		stringBuilder.Append(" / Crit: ").Append(num8.ToString("P1"));
		return stringBuilder.ToString();
	}

	public string GetResult()
	{
		return DamageRecordToString(_targetDamageRecord, _combatStartTime);
	}
}
