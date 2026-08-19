using System;
using L10N;
using Shared.System;
using TimerData;
using UnityEngine;

namespace InteractionData;

public struct InteractionMenuData : IComparable<InteractionMenuData>
{
	private string _name;

	private string _icon;

	public ulong Id;

	public Color Color;

	public string Description;

	public float Duration;

	public bool Disabled;

	public bool AccessDenied;

	public GatheringData GatheringData;

	private Timer _timer;

	public int Action { get; private set; }

	public bool IsServer { get; private set; }

	public int Priority => ActionPriority(Action, IsServer);

	public string Name
	{
		get
		{
			if (_name == null)
			{
				_name = ((!IsServer) ? ((Interaction)Action).GetName() : ((Shared.System.Interaction)Action).GetName());
			}
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public string Icon
	{
		get
		{
			if (_icon == null)
			{
				_icon = ((!IsServer) ? IconMap.Get((Interaction)Action) : IconMap.Get((Shared.System.Interaction)Action));
			}
			return _icon;
		}
		set
		{
			_icon = value;
		}
	}

	public string GatheringId => (GatheringData != null) ? GatheringData.Id : null;

	public Timer Timer => _timer;

	public InteractionMenuData(Shared.System.Interaction action)
	{
		Set((int)action, isServer: true);
	}

	public InteractionMenuData(Interaction action)
	{
		Set((int)action, isServer: false);
	}

	private void Set(int action, bool isServer)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Action = action;
		IsServer = isServer;
		Duration = -1f;
		SetTimer(null);
		SetColor((!isServer) ? InteractionMenuColor((Interaction)action) : InteractionMenuColor((Shared.System.Interaction)action));
	}

	public void Set(GatheringData data)
	{
		GatheringData = data;
		Name = T.Format("{0} <em>{1:lv:}</em>", data.Name, data.Level);
		Icon = data.Icon;
		Description = ((data.Amount <= 0) ? null : data.Amount.ToString());
		Duration = data.Duration;
		SetTimer(null);
	}

	public void SetTimer(Timer timer)
	{
		_timer = timer;
	}

	public void SetColor(Color col)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Color = ((!Disabled) ? col : Color.gray);
	}

	public static Color InteractionMenuColor(Interaction action)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Color.white;
	}

	public static Color InteractionMenuColor(Shared.System.Interaction action)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		switch (action)
		{
		case Shared.System.Interaction.Attack:
		case Shared.System.Interaction.DestructArtifact:
			return Color.red;
		case Shared.System.Interaction.CompleteArtifact:
			return Color32.op_Implicit(new Color32((byte)53, (byte)172, (byte)26, byte.MaxValue));
		default:
			return Color.white;
		}
	}

	private static int ActionPriority(int action, bool isServer)
	{
		return (!isServer) ? InteractionMenuPriority.Priority((Interaction)action) : InteractionMenuPriority.Priority((Shared.System.Interaction)action);
	}

	public int CompareTo(InteractionMenuData other)
	{
		int priority = Priority;
		int priority2 = other.Priority;
		if (priority == priority2)
		{
			return Action - other.Action;
		}
		return priority2 - priority;
	}

	public static bool IsKeepInteractionMenuAction(InteractionMenuData menu)
	{
		if (menu.IsServer)
		{
			Shared.System.Interaction action = (Shared.System.Interaction)menu.Action;
			if (action == Shared.System.Interaction.Take || action == Shared.System.Interaction.Collect)
			{
				return true;
			}
		}
		else
		{
			Interaction action2 = (Interaction)menu.Action;
			if (action2 == Interaction.CraftingItem)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsRangeInteractionMenuAction(InteractionMenuData menu)
	{
		if (menu.IsServer)
		{
			Shared.System.Interaction action = (Shared.System.Interaction)menu.Action;
			if (action == Shared.System.Interaction.GetProfile || action == Shared.System.Interaction.Whisper || action == Shared.System.Interaction.Attack || action == Shared.System.Interaction.DeclareWar || action == Shared.System.Interaction.SendReport)
			{
				return true;
			}
		}
		else
		{
			Interaction action2 = (Interaction)menu.Action;
			if (action2 == Interaction.DeclareWar)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEqualKey(InteractionMenuData data)
	{
		return Action == data.Action && IsServer == data.IsServer && Id == data.Id && GatheringId == data.GatheringId;
	}
}
