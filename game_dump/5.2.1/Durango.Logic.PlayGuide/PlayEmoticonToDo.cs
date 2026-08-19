using System;

namespace Durango.Logic.PlayGuide;

public class PlayEmoticonToDo : ToDoBase
{
	private readonly string _id;

	public PlayEmoticonToDo(string id)
	{
		_id = id;
	}

	public override void OnAddItem()
	{
		GameSystem<SocialSystem>.Instance().EmoticonPlayed += SocialSystem_EmoticonPlayed;
	}

	public override void OnRemoveItem()
	{
		GameSystem<SocialSystem>.Instance().EmoticonPlayed -= SocialSystem_EmoticonPlayed;
	}

	private void SocialSystem_EmoticonPlayed(string key)
	{
		if (string.IsNullOrEmpty(_id) || _id.Equals(key, StringComparison.OrdinalIgnoreCase))
		{
			CallComplete();
		}
	}
}
