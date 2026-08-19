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

	public static int UILayer
	{
		get
		{
			if (_uiLayer == -1)
			{
				return _uiLayer = LayerMask.NameToLayer("NGUI");
			}
			return _uiLayer;
		}
	}

	public static int UIOverLayer
	{
		get
		{
			if (_uiOverLayer == -1)
			{
				return _uiOverLayer = LayerMask.NameToLayer("NGUI Over");
			}
			return _uiOverLayer;
		}
	}

	public static int PropLayer
	{
		get
		{
			if (_propLayer == -1)
			{
				return _propLayer = LayerMask.NameToLayer("Prop");
			}
			return _propLayer;
		}
	}

	public static LayerMask PropMask => 1 << PropLayer;

	public static int DefaultLayer
	{
		get
		{
			if (_defaultLayer == -1)
			{
				return _defaultLayer = LayerMask.NameToLayer("Default");
			}
			return _defaultLayer;
		}
	}

	public static LayerMask DefaultMask => 1 << DefaultLayer;

	public static LayerMask InteractionMask => (int)PropMask | (int)DefaultMask;

	public static int OverlayLayer
	{
		get
		{
			if (_overlayLayer == -1)
			{
				return _overlayLayer = LayerMask.NameToLayer("Overlay Effect");
			}
			return _overlayLayer;
		}
	}

	public static bool IsUILayer(int layer)
	{
		if (layer != UILayer)
		{
			return layer == UIOverLayer;
		}
		return true;
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
