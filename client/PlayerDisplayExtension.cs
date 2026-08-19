using Durango.Logic.Social;
using Durango.Utils.Extensions;
using Messages;

public static class PlayerDisplayExtension
{
	public static PortraitBuilder.Argument GetPortraitArgument(this PlayerDisplay display, string entityId, bool isMale)
	{
		if (string.IsNullOrEmpty(display.PortraitIcon))
		{
			PortraitBuilder.Argument result = PortraitBuilder.MakeArgument(display.Portrait, display.PortraitBg, display.PortraitBgColor.ToColor(), isMale, PortraitEmotion.Normal, display.SkinColor.ToColor(), display.HairColor.ToColor(), display.EyeColor.ToColor(), display.LipColor.ToColor());
			PortraitBuilder.FillEmptyBackground(entityId, ref result.Background, ref result.BgColor);
			return result;
		}
		PortraitBuilder.Argument result2 = default(PortraitBuilder.Argument);
		result2.Preset = display.PortraitIcon;
		return result2;
	}
}
