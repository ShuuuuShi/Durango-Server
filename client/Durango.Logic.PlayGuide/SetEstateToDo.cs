using Durango.Utils.Extensions;
using Messages;
using Shared.Estate;

namespace Durango.Logic.PlayGuide;

public class SetEstateToDo : ToDoBase
{
	private readonly OwnerType _ownerType;

	public SetEstateToDo(string type)
	{
		_ownerType = type.ToEnum(OwnerType.Player);
	}

	public override void OnAddItem()
	{
		EstateSystem.GetEstateLicenses(delegate(EstateLicenses licenses)
		{
			switch (_ownerType)
			{
			case OwnerType.Player:
				if (licenses.UrbanEstate.HasValue)
				{
					CallComplete();
				}
				break;
			case OwnerType.ClanEstate:
				if (licenses.ClanEstate.HasValue)
				{
					CallComplete();
				}
				break;
			case OwnerType.PersonalPlayer:
				if (licenses.PersonalEstate.HasValue)
				{
					CallComplete();
				}
				break;
			}
		});
		GameSystem<EstateSystem>.Instance().EstateDeclared += EstateSystem_EstateDeclared;
	}

	public override void OnRemoveItem()
	{
		GameSystem<EstateSystem>.Instance().EstateDeclared -= EstateSystem_EstateDeclared;
	}

	private void EstateSystem_EstateDeclared(EstateLicense msg)
	{
		CallComplete();
	}
}
