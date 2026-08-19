using Durango.Network;
using Durango.Utils.Extensions;
using Messages;

namespace Durango.Logic.PlayGuide;

internal class PrivateEstateExpirationCondition : FlowCondition
{
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
		EstateSystem.GetEstateLicenses(delegate(EstateLicenses licenses)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			if (licenses.UrbanEstate.HasValue)
			{
				double? num = licenses.UrbanEstate.Value.ExpiresAt - predictedServerTime;
				if (num.HasValue && num.GetValueOrDefault() < (double)(base.Param.ToInt() * 60 * 60 * 24))
				{
					Interrupt();
				}
			}
		});
	}
}
