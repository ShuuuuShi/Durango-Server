using System;
using UnityEngine;

namespace Durango.Utils;

public static class LayerHelper
{
	private static int _uiLayer = -1;

	private static int _uiOverLayer = -1;

	private static int _propLayer = -1;

	private static int _defaultLayer = -1;

	private static int _overlayLayer = -1;

	public static int UILayer => (_uiLayer != -1) ? _uiLayer : (_uiLayer = LayerMask.NameToLayer("NGUI"));

	public static int UIOverLayer => (_uiOverLayer != -1) ? _uiOverLayer : (_uiOverLayer = LayerMask.NameToLayer("NGUI Over"));

	public static int PropLayer => (_propLayer != -1) ? _propLayer : (_propLayer = LayerMask.NameToLayer("Prop"));

	public static LayerMask PropMask => 1 << PropLayer;

	public static int DefaultLayer => (_defaultLayer != -1) ? _defaultLayer : (_defaultLayer = LayerMask.NameToLayer("Default"));

	public static LayerMask DefaultMask => 1 << DefaultLayer;

	public static LayerMask InteractionMask => (int)PropMask | (int)DefaultMask;

	public static int OverlayLayer => (_overlayLayer != -1) ? _overlayLayer : (_overlayLayer = LayerMask.NameToLayer("Overlay Effect"));

	public static bool IsUILayer(int layer)
	{
		return layer == UILayer || layer == UIOverLayer;
	}

	public static void SetLayer(GameObject go, int layer, Func<GameObject, bool> filter = null)
	{
		go.layer = layer;
		Transform transform = go.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (filter == null || !filter(child.gameObject))
			{
				SetLayer(child.gameObject, layer, filter);
			}
		}
	}
}
