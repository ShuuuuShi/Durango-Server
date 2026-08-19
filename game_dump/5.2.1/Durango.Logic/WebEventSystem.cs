using System;
using System.Collections.Generic;
using System.Linq;
using Durango.System;
using Durango.Utils;
using L10N;
using Shared.Region;

namespace Durango.Logic;

public class WebEventSystem : GameSystem<WebEventSystem>
{
	private string _url;

	public bool HasEvent
	{
		get
		{
			if (!string.IsNullOrEmpty(_url))
			{
				return OptionSystem.IsWebEventEnabled();
			}
			return false;
		}
	}

	public event Action Updated;

	private void Start()
	{
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			Role role = GameManager.Region.Role();
			if (role != Role.Invalid && role != Role.Instance && OptionSystem.IsWebEventEnabled())
			{
				RequestEvents();
			}
		};
		GameSystem<OptionSystem>.Instance().AddOnChange("quest.web_ui_enabled", delegate(bool on)
		{
			if (on)
			{
				RequestEvents();
			}
		});
	}

	private void RequestEvents()
	{
		Http.RequestYml<Dictionary<string, string>>(GameManager.GatewayUrl + "/events", UpdateEvents, disableCache: true);
	}

	private void UpdateEvents(Dictionary<string, string> events)
	{
		_url = events?.FirstOrDefault().Value;
		if (this.Updated != null)
		{
			this.Updated();
		}
	}

	public void Show()
	{
		if (HasEvent)
		{
			string url = _url + "?npsn=" + Platform.Instance.NPSN + "&token=" + Platform.Instance.Token + "&lang=" + LocalizeSystem.LocaleLanguage + "&cid=" + GameManager.PlayerId;
			Platform.Instance.ShowWeb(T._("이벤트"), url);
		}
	}
}
