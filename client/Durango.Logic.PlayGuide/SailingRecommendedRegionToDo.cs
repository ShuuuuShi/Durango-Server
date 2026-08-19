using Durango.Utils.Extensions;
using Shared.Region;

namespace Durango.Logic.PlayGuide;

public class SailingRecommendedRegionToDo : ToDoBase
{
	private readonly Role[] _roles;

	public SailingRecommendedRegionToDo(string roles)
	{
		string[] array = roles.SplitAndTrim('|');
		_roles = new Role[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_roles[i] = array[i].ToEnum(Role.Rural);
		}
	}

	public override void OnAddItem()
	{
		GameSystem<MapSystem>.Instance().TriedTravelToStableRegion += MapSystem_TriedTravelToStableRegion;
	}

	public override void OnRemoveItem()
	{
		GameSystem<MapSystem>.Instance().TriedTravelToStableRegion -= MapSystem_TriedTravelToStableRegion;
	}

	private void MapSystem_TriedTravelToStableRegion(Role role)
	{
		if (_roles.Contains(role))
		{
			CallComplete();
		}
	}
}
