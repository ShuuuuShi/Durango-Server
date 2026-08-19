using System;
using MsgPack;

namespace Player;

public class MainStatus
{
	public string MainStatusId { get; private set; }

	public string MainStatusName { get; private set; }

	public PortraitEmotion PortraitEmotion { get; private set; }

	public PortraitEffect PortraitEffect { get; private set; }

	public ScreenEffectType ScreenEffectType { get; private set; }

	public event Action<string> Changed;

	private bool UpdateMainStatus(string id, string name, string portrait, string portraitEffect, string screenEffect)
	{
		if (MainStatusId != null && MainStatusId == id)
		{
			return false;
		}
		MainStatusId = id;
		MainStatusName = name;
		try
		{
			PortraitEmotion = (PortraitEmotion)(int)Enum.Parse(typeof(PortraitEmotion), portrait, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			PortraitEmotion = PortraitEmotion.None;
		}
		try
		{
			PortraitEffect = (PortraitEffect)(int)Enum.Parse(typeof(PortraitEffect), portraitEffect, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			PortraitEffect = PortraitEffect.None;
		}
		try
		{
			ScreenEffectType = (ScreenEffectType)(int)Enum.Parse(typeof(ScreenEffectType), screenEffect, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			ScreenEffectType = ScreenEffectType.None;
		}
		if (this.Changed != null)
		{
			this.Changed(MainStatusId);
		}
		return true;
	}

	public void RequestRefresh()
	{
	}

	public bool UpdateMainStatus(MessagePackObjectDictionary data)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		MessagePackObject val = data[MessagePackObject.op_Implicit("id")];
		string id = ((MessagePackObject)(ref val)).AsString();
		MessagePackObject val2 = data[MessagePackObject.op_Implicit("name")];
		string name = ((MessagePackObject)(ref val2)).AsString();
		MessagePackObject val3 = data[MessagePackObject.op_Implicit("portrait")];
		string portrait = ((MessagePackObject)(ref val3)).AsString();
		MessagePackObject val4 = data[MessagePackObject.op_Implicit("portrait_effect")];
		string portraitEffect = ((MessagePackObject)(ref val4)).AsString();
		MessagePackObject val5 = data[MessagePackObject.op_Implicit("screen_effect")];
		string screenEffect = ((MessagePackObject)(ref val5)).AsString();
		return UpdateMainStatus(id, name, portrait, portraitEffect, screenEffect);
	}

	public ScreenEffectType GetScreenEffectType()
	{
		return ScreenEffectType;
	}
}
