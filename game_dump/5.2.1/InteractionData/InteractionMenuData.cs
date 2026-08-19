using System;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using L10N;
using UnityEngine;

namespace InteractionData;

public struct InteractionMenuData : IComparable<InteractionMenuData>
{
	private string _name;

	public string Id;

	public int Count;

	public float Duration;

	public bool Disabled;

	public bool AccessDenied;

	public GatheringData GatheringData;

	private Color _color;

	private ItemIcon _icon;

	public Interaction Action { get; private set; }

	public string Name
	{
		get
		{
			if (_name == null)
			{
				_name = Action.GetName();
			}
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public ItemIcon Icon
	{
		get
		{
			if (string.IsNullOrEmpty(_icon.Main))
			{
				_icon = new ItemIcon(Action.GetIcon("icon_question"));
			}
			return _icon;
		}
		set
		{
			_icon = value;
		}
	}

	public Color Color
	{
		get
		{
			if (Disabled || AccessDenied)
			{
				return Color.gray;
			}
			if (_color == Color.clear)
			{
				return Color.white;
			}
			return _color;
		}
	}

	public Timer Timer { get; private set; }

	public InteractionMenuList Parent { get; set; }

	public InteractionMenuData(Interaction action)
	{
		this = default(InteractionMenuData);
		Action = action;
		Duration = -1f;
		SetTimer(null);
		SetColor(InteractionMenuColor(action));
	}

	public InteractionMenuData(GatheringData data, int parentLevel)
		: this(Interaction.Collect)
	{
		Id = data.GeneratorId;
		GatheringData = data;
		if (parentLevel > 0)
		{
			if (parentLevel > data.Level)
			{
				Name = T._("{0} <em>{1:lv:}</em>", data.Name, data.Level);
			}
			else if (parentLevel < data.Level)
			{
				Name = T._("{0} <weak>{1:lv:}</weak>", data.Name, data.Level);
			}
			else
			{
				Name = data.Name;
			}
		}
		else
		{
			Name = T._("{0} <em>{1:lv:}</em>", data.Name, data.Level);
		}
		Icon = data.Icon;
		Count = data.Amount;
		Duration = data.Duration;
		SetTimer(null);
	}

	public static implicit operator InteractionMenuData(Interaction action)
	{
		return new InteractionMenuData(action);
	}

	public void SetTimer(Timer timer)
	{
		if (Timer != timer)
		{
			Timer = timer;
		}
	}

	public void SetColor(Color col)
	{
		_color = col;
	}

	public static Color InteractionMenuColor(Interaction action)
	{
		switch (action)
		{
		case Interaction.Attack:
		case Interaction.DestructArtifact:
		case Interaction.GiveUpDistribution:
		case Interaction.RemoveNatural:
		case Interaction.RemoveGrazingPet:
			return Color.red;
		case Interaction.CompleteArtifact:
			return new Color32(53, 172, 26, byte.MaxValue);
		default:
			return Color.white;
		}
	}

	public int CompareTo(InteractionMenuData other)
	{
		int num = InteractionMenuPriority.Priority(Action);
		int num2 = InteractionMenuPriority.Priority(other.Action);
		if (num == num2)
		{
			return Action - other.Action;
		}
		return num2 - num;
	}

	public static bool IsKeepInteractionMenuAction(Interaction action)
	{
		if (action == Interaction.Collect || action == Interaction.Sprinkle || action == Interaction.ExtendFloor)
		{
			return true;
		}
		return false;
	}

	public static bool IsRangeInteractionMenuAction(Interaction action)
	{
		if (action == Interaction.HostConcert || action == Interaction.RegisterConcert || action == Interaction.GetProfile || action == Interaction.Whisper || action == Interaction.Attack || action == Interaction.OnePunch || action == Interaction.SendReport || action == Interaction.DeclareWar || action == Interaction.GiveUpDistribution || action == Interaction.RemoveGrazingPet)
		{
			return true;
		}
		return false;
	}

	public static bool IsQueueableAction(Interaction action)
	{
		if (action == Interaction.Collect || action == Interaction.Sprinkle)
		{
			return true;
		}
		return false;
	}

	public static bool IsRidableAction(Interaction action)
	{
		switch (action)
		{
		case Interaction.CompleteArtifact:
		case Interaction.AddOnManage:
		case Interaction.HelpPostprocess:
		case Interaction.GetTimeline:
		case Interaction.RepairArtifactImmediately:
		case Interaction.EstateLicense:
		case Interaction.ExtendEstate:
		case Interaction.CloseGate:
		case Interaction.OpenGate:
		case Interaction.Inventory:
		case Interaction.OpenWorkbench:
		case Interaction.SetAsHome:
		case Interaction.SetAsBase:
		case Interaction.UseKiosk:
		case Interaction.UseWarehouse:
		case Interaction.ClanResearch:
		case Interaction.Craft:
		case Interaction.InviteToClan:
		case Interaction.Dye:
		case Interaction.GrowRapidly:
		case Interaction.ReadPinboard:
		case Interaction.ViewPunchRanking:
		case Interaction.Bleach:
		case Interaction.NameArtifact:
		case Interaction.RenameArtifact:
		case Interaction.AcceptMission:
		case Interaction.CancelMission:
		case Interaction.CancelAllMissions:
		case Interaction.WarpCargoToPrivate:
		case Interaction.WarpCargoToClan:
		case Interaction.ActivateCargoReceiver:
		case Interaction.GetProfile:
		case Interaction.Whisper:
		case Interaction.SendReport:
		case Interaction.Dismount:
		case Interaction.OpenPetInven:
		case Interaction.RenamePet:
		case Interaction.SetAccess:
		case Interaction.DeclareWar:
		case Interaction.SkipPostprocess:
		case Interaction.SearchWarphole:
		case Interaction.EstateMenu:
		case Interaction.DismountAirBalloon:
		case Interaction.CaptureScreenShot:
		case Interaction.OpenPetMenu:
		case Interaction.PetActiveSkill:
			return true;
		default:
			return false;
		}
	}

	public static bool IsVehicleAction(Interaction action)
	{
		switch (action)
		{
		case Interaction.Mount:
		case Interaction.Dismount:
		case Interaction.OpenPetInven:
		case Interaction.Feeding:
		case Interaction.ReturnPet:
		case Interaction.MountVehicle:
		case Interaction.AddProjectileToVehicle:
		case Interaction.MountAirBalloon:
		case Interaction.DismountAirBalloon:
			return true;
		default:
			return false;
		}
	}

	public static bool IsMovingAction(Interaction action)
	{
		if (action == Interaction.Dash)
		{
			return true;
		}
		return false;
	}

	public bool IsEqualKey(InteractionMenuData data)
	{
		return IsEqualKey(data.Action, data.Id);
	}

	public bool IsEqualKey(Interaction action, string id)
	{
		if (Action == action)
		{
			return Id == id;
		}
		return false;
	}
}
