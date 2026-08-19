using UnityEngine;

public static class LayerHelper
{
	private static int _propLayer = -1;

	private static int _defaultLayer = -1;

	private static int _overlayLayer = -1;

	public static int PropLayer => (_propLayer != -1) ? _propLayer : (_propLayer = LayerMask.NameToLayer("Prop"));

	public static LayerMask PropMask => LayerMask.op_Implicit(1 << PropLayer);

	public static int DefaultLayer => (_defaultLayer != -1) ? _defaultLayer : (_defaultLayer = LayerMask.NameToLayer("Default"));

	public static LayerMask DefaultMask => LayerMask.op_Implicit(1 << DefaultLayer);

	public static LayerMask InteractionMask => LayerMask.op_Implicit(LayerMask.op_Implicit(PropMask) | LayerMask.op_Implicit(DefaultMask));

	public static int OverlayLayer => (_overlayLayer != -1) ? _overlayLayer : (_overlayLayer = LayerMask.NameToLayer("Overlay Effect"));
}
