using Durango.UI;
using Durango.Utils.Extensions;
using Messages;
using Shared.Region;

namespace Durango.Logic.PlayGuide;

public class SailingToDo : ToDoBase
{
	private readonly Role _role;

	private readonly Biome _biome;

	private readonly int _level;

	public SailingToDo(string id, int level)
	{
		_level = level;
		_role = Role.Invalid;
		_biome = Biome.Invalid;
		if (!string.IsNullOrEmpty(id))
		{
			string[] array = id.Split(':');
			_role = array[0].Trim().ToEnum(Role.Invalid);
			if (array.Length > 1)
			{
				_biome = array[1].Trim().ToEnum(Biome.Invalid);
			}
		}
	}

	public override void OnAddItem()
	{
		UIManager.FindScript<ExploreGroup>().TravelRequested += ExploreGroup_TravelRequested;
	}

	public override void OnRemoveItem()
	{
		ExploreGroup exploreGroup = UIManager.FindScript<ExploreGroup>();
		if (!(exploreGroup == null))
		{
			exploreGroup.TravelRequested -= ExploreGroup_TravelRequested;
		}
	}

	private void ExploreGroup_TravelRequested(Route route)
	{
		if ((_role == Role.Invalid || route.Region().Role() == _role) && (_biome == Biome.Invalid || _biome == route.Region().MajorBiome()) && (_level <= 0 || _level <= route.Region().Level))
		{
			CallComplete();
		}
	}
}
