using System;
using Durango.Network;
using Durango.Utils;
using Messages;

namespace Durango.Logic;

public class EngagementSystem : GameSystem<EngagementSystem>
{
	private const string AgreedKey = "engagement_agreed";

	public bool Agreed
	{
		get
		{
			if (OptionSystem.IsShutdownEngagement())
			{
				return false;
			}
			return Preferences.GetBool("engagement_agreed");
		}
		set
		{
			if (!OptionSystem.IsShutdownEngagement() && Agreed != value)
			{
				Preferences.SetBool("engagement_agreed", value);
				UpdateEngagement();
				if (this.AgreedChanged != null)
				{
					this.AgreedChanged(value);
				}
			}
		}
	}

	public bool EngagementRewardSent { get; private set; }

	public event Action<bool> AgreedChanged;

	private void Awake()
	{
		Singleton<GameManager>.Instance().WelcomeReceived += delegate(Welcome welcome)
		{
			EngagementRewardSent = welcome.EngagementRewardSent;
		};
		Singleton<GameManager>.Instance().AddOnReady(UpdateEngagement);
		GameSystem<OptionSystem>.Instance().AddOnChange("shutdown.engagement.disable", (Action<bool>)delegate
		{
			UpdateEngagement();
		});
	}

	private void UpdateEngagement()
	{
		Connections.Frontend.Send(new EngagementAgreementChanged
		{
			Agreed = Agreed
		});
	}
}
