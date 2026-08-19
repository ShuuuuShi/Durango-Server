using Durango.Utils.Extensions;
using Shared.Region;

namespace Durango.Logic.PlayGuide;

internal class CurrentRegionCondition : FlowCondition
{
	private readonly int _level;

	private readonly Biome _biome;

	public CurrentRegionCondition(string param)
	{
		_level = 0;
		_biome = Biome.Invalid;
		if (!string.IsNullOrEmpty(param))
		{
			string[] array = param.Split(':');
			_level = array[0].Trim().ToInt();
			if (array.Length > 1)
			{
				_biome = array[1].Trim().ToEnum(Biome.Invalid);
			}
		}
	}

	protected override void OnRegister()
	{
		GameSystem<PlayGuideSystem>.Instance().Begun += PlayGuideSystem_Begun;
	}

	protected override void OnUnregister()
	{
		GameSystem<PlayGuideSystem>.Instance().Begun -= PlayGuideSystem_Begun;
	}

	private void PlayGuideSystem_Begun(GuideRole prev, GuideRole cur)
	{
		if ((_biome == Biome.Invalid || _biome == GameManager.Region.MajorBiome()) && (_level <= 0 || _level <= GameManager.Region.Level))
		{
			Interrupt();
		}
	}
}
