using Durango.Logic.Social;

public class ChatableHuman : ChatableCharacter<HumanBehavior>
{
	private PortraitBuilder.Argument _portraitArgument;

	private int _curPortraitRandomKey;

	public override bool IsMale => Owner.CostumableModel.IsMale;

	public ChatableHuman(HumanBehavior owner)
		: base(owner)
	{
		RefreshPortrait(_curPortraitRandomKey, force: true);
	}

	public override PortraitBuilder.Argument GetPortraitArgument(PortraitEmotion emotion = PortraitEmotion.None)
	{
		return _portraitArgument;
	}

	public void RefreshPortrait(int key, bool force = false)
	{
		if (_curPortraitRandomKey != key || force)
		{
			_curPortraitRandomKey = key;
			_portraitArgument = PortraitBuilder.MakeRandomArgument(IsMale, key);
		}
	}
}
