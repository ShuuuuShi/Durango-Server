using UnityEngine;

public class CombatActionProgressGauge : IconProgressGauge
{
	protected override void Reposition()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)base.Target == (Object)null))
		{
			((Component)this).transform.localPosition = MainCamera.NGUILocalPositionToNGUIPosition(base.Target.localPosition, base.Target.parent) + PositionOffset;
		}
	}
}
