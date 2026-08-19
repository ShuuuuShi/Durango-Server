using System.Collections.Generic;
using L10N;
using UnityEngine;

public static class DeathActionDescriptor
{
	public enum ActionType
	{
		Craft,
		Construct,
		Destruct,
		Eat,
		Drink,
		DrawWater,
		Wash,
		BattleBegin,
		BattleEnd,
		BattleKill,
		Move,
		Respawn,
		Gather,
		Fishing,
		Rest,
		Watering
	}

	private static readonly Dictionary<int, string[]> DeathMsgActionMap = new Dictionary<int, string[]>
	{
		{
			0,
			new string[1] { T.N_("{0:을} 제작하려고 끄적대다가,") }
		},
		{
			1,
			new string[1] { T.N_("{0:을} 건설하려고 열심히 노동 하다가,") }
		},
		{
			2,
			new string[1] { T.N_("{0:을} 열심히 철거하다가,") }
		},
		{
			3,
			new string[1] { T.N_("{0} 하지만,") }
		},
		{
			4,
			new string[1] { T.N_("목이 타서 {0:을} 들이켰으나,") }
		},
		{
			5,
			new string[1] { T.N_("{0:을} 떴으나 써보지도 못한채,") }
		},
		{
			6,
			new string[1] { T.N_("잘 보이려고 깨끗이 몸을 씻었으나,") }
		},
		{
			7,
			new string[1] { T.N_("{0:과} 격렬한 전투를 벌이던 중,") }
		},
		{
			8,
			new string[1] { T.N_("{0:을} 간신히 따돌렸지만,") }
		},
		{
			9,
			new string[1] { T.N_("{0:을} 가까스로 격퇴했지만,") }
		},
		{
			10,
			new string[1] { T.N_("하염없이 걷다가 그만,") }
		},
		{
			11,
			new string[1] { T.N_("{0}에서 새 생명을 얻었는데,") }
		},
		{
			12,
			new string[1] { T.N_("{0:을} 채집하다가,") }
		},
		{
			13,
			new string[1] { T.N_("작살 낚시를 하다가,") }
		},
		{
			14,
			new string[1] { T.N_("잠시 앉아 졸다가,") }
		},
		{
			15,
			new string[1] { T.N_("{0}에 물을 주다가") }
		}
	};

	private static readonly string[] DeathMsgAttackedList = new string[4]
	{
		T.N_("{0}{1} 죽었습니다."),
		T.N_("숙적 {0}{1} 생을 마감했습니다."),
		T.N_("원수 {0}{1}, 죽어도 편히 눈을 못 감습니다."),
		T.N_("갑자기 {0}{1} 돌연사!!!")
	};

	private static string _lastAction;

	private static string _deathMsg;

	public static string GetDeathMsg()
	{
		Gauge gauge = PlayerBehavior.LocalPlayer.GetGauge("fatigue");
		if (gauge != null && gauge.Get() >= gauge.Max())
		{
			_deathMsg = T._("피... 곤... 해...");
		}
		if (string.IsNullOrEmpty(_deathMsg))
		{
			_deathMsg = T._("알 수 없는 이유로 돌연사했습니다.");
		}
		return $"{_lastAction} {_deathMsg}";
	}

	public static void SetLastAction(ActionType actionType, string targetName = null)
	{
		if (string.IsNullOrEmpty(targetName))
		{
			targetName = T._("무언가");
		}
		string[] list = DeathMsgActionMap.Get((int)actionType);
		string random = LocalizeSystem.GetRandom(list);
		if (string.IsNullOrEmpty(random))
		{
		}
		_lastAction = LocalizeSystem.Format(random, targetName);
	}

	public static void Attacked(CharacterBehavior enemyChracter, float damage)
	{
		if (!((Object)(object)enemyChracter == (Object)null))
		{
			string name = enemyChracter.GetName();
			string attackNameForDeathMsg = enemyChracter.GetAttackNameForDeathMsg();
			_deathMsg = string.Format(LocalizeSystem.GetRandom(DeathMsgAttackedList), name, attackNameForDeathMsg);
		}
	}
}
