using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Keys (Legacy)")]
[ExecuteInEditMode]
public class UIButtonKeys : UIKeyNavigation
{
	public UIButtonKeys selectOnClick;

	public UIButtonKeys selectOnUp;

	public UIButtonKeys selectOnDown;

	public UIButtonKeys selectOnLeft;

	public UIButtonKeys selectOnRight;

	protected override void OnEnable()
	{
		Upgrade();
		base.OnEnable();
	}

	public void Upgrade()
	{
		if ((Object)(object)onClick == (Object)null && (Object)(object)selectOnClick != (Object)null)
		{
			onClick = ((Component)selectOnClick).gameObject;
			selectOnClick = null;
			NGUITools.SetDirty((Object)(object)this);
		}
		if ((Object)(object)onLeft == (Object)null && (Object)(object)selectOnLeft != (Object)null)
		{
			onLeft = ((Component)selectOnLeft).gameObject;
			selectOnLeft = null;
			NGUITools.SetDirty((Object)(object)this);
		}
		if ((Object)(object)onRight == (Object)null && (Object)(object)selectOnRight != (Object)null)
		{
			onRight = ((Component)selectOnRight).gameObject;
			selectOnRight = null;
			NGUITools.SetDirty((Object)(object)this);
		}
		if ((Object)(object)onUp == (Object)null && (Object)(object)selectOnUp != (Object)null)
		{
			onUp = ((Component)selectOnUp).gameObject;
			selectOnUp = null;
			NGUITools.SetDirty((Object)(object)this);
		}
		if ((Object)(object)onDown == (Object)null && (Object)(object)selectOnDown != (Object)null)
		{
			onDown = ((Component)selectOnDown).gameObject;
			selectOnDown = null;
			NGUITools.SetDirty((Object)(object)this);
		}
	}
}
