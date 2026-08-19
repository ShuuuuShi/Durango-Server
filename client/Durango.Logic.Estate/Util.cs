using L10N;
using Shared.Estate;

namespace Durango.Logic.Estate;

public static class Util
{
	public static string GetName(OwnerType type, AccessRights rights)
	{
		switch (type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			switch (rights)
			{
			case AccessRights.Enter:
				return T._("출입");
			case AccessRights.UseFacility:
				return T._("설비 이용");
			case AccessRights.Give:
				return T._("사유지에 주기");
			case AccessRights.Take:
				return T._("사유지에서 가져가기");
			case AccessRights.Occupy:
				return T._("건설");
			case AccessRights.Destruct:
				return T._("파괴");
			}
			break;
		case OwnerType.ClanEstate:
			switch (rights)
			{
			case AccessRights.Enter:
				return T._("출입");
			case AccessRights.UseFacility:
				return T._("설비 이용");
			case AccessRights.Give:
				return T._("부족 영토 및 거점에 주기");
			case AccessRights.Take:
				return T._("부족 영토 및 거점에서 가져가기");
			case AccessRights.Occupy:
				return T._("건설");
			case AccessRights.Destruct:
				return T._("파괴");
			}
			break;
		}
		return null;
	}

	public static string GetDescription(OwnerType type, AccessRights rights)
	{
		switch (type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			switch (rights)
			{
			case AccessRights.Enter:
				return T._("사유지의 문들을 열고 닫을 수 있도록 한다.");
			case AccessRights.UseFacility:
				return T._("사유지의 시설물들을 사용할 수 있도록 한다.");
			case AccessRights.Give:
				return T._("사유지에 아이템을 놓는 것 등을 할 수 있게 한다.");
			case AccessRights.Take:
				return T._("사유지에 있는 아이템을 가져가거나 사유지 내에서 채집 등을 할 수 있게 한다.");
			case AccessRights.Occupy:
				return T._("사유지 내에 건설 부지를 선정할 수 있게 한다.");
			case AccessRights.Destruct:
				return T._("사유지 내의 것들을 파괴할 수 있게 한다.");
			}
			break;
		case OwnerType.ClanEstate:
			switch (rights)
			{
			case AccessRights.Enter:
				return T._("부족 영토 및 거점의 문들을 열고 닫을 수 있도록 한다.");
			case AccessRights.UseFacility:
				return T._("부족 영토 및 거점의 시설물들을 사용할 수 있도록 한다.");
			case AccessRights.Give:
				return T._("부족 영토 및 거점에 아이템을 놓는 것 등을 할 수 있게 한다.");
			case AccessRights.Take:
				return T._("부족 영토 및 거점에 있는 아이템을 가져가거나 부족 영토 내에서 채집 등을 할 수 있게 한다.");
			case AccessRights.Occupy:
				return T._("부족 영토 및 거점 내에 건설 부지를 선정할 수 있게 한다.");
			case AccessRights.Destruct:
				return T._("부족 영토 및 거점 내의 것들을 파괴할 수 있게 한다.");
			}
			break;
		}
		return null;
	}
}
