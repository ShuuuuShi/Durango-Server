using System;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelClone : MonoBehaviour
{
	private class DrawcallClone
	{
		public UIDrawCall Origin;

		public Transform Copy;

		public Material CopyMat;

		public MeshFilter MeshFilter;

		public MeshRenderer MeshRender;

		public void UpdatePosition()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = Origin.cachedTransform.position;
			Vector3 lossyScale = Origin.cachedTransform.lossyScale;
			((Vector3)(ref position))._002Ector(position.x / lossyScale.x, position.y / lossyScale.y, position.z / lossyScale.z);
			Copy.localPosition = position;
		}

		public void UpdateArgument()
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			if (Origin.isClipped)
			{
				int clipCount = Origin.manager.clipCount;
				for (int i = 0; i < clipCount; i++)
				{
					Vector4 vector = Origin.baseMaterial.GetVector(_clipRange[i]);
					Vector4 vector2 = Origin.baseMaterial.GetVector(_clipArgs[i]);
					CopyMat.SetVector(_clipRange[i], vector);
					CopyMat.SetVector(_clipArgs[i], vector2);
				}
			}
		}
	}

	public Action CloneUpdated;

	private List<UIPanel> _panels = new List<UIPanel>();

	private List<DrawcallClone> _list = new List<DrawcallClone>();

	private Stack<Transform> _stack = new Stack<Transform>();

	private int _validCount;

	private bool _isDirty;

	private static int[] _clipRange;

	private static int[] _clipArgs;

	public UIPanel Target { get; private set; }

	private void Awake()
	{
		if (_clipRange == null)
		{
			_clipRange = new int[4]
			{
				Shader.PropertyToID("_ClipRange0"),
				Shader.PropertyToID("_ClipRange1"),
				Shader.PropertyToID("_ClipRange2"),
				Shader.PropertyToID("_ClipRange4")
			};
		}
		if (_clipArgs == null)
		{
			_clipArgs = new int[4]
			{
				Shader.PropertyToID("_ClipArgs0"),
				Shader.PropertyToID("_ClipArgs1"),
				Shader.PropertyToID("_ClipArgs2"),
				Shader.PropertyToID("_ClipArgs3")
			};
		}
	}

	private void OnEnable()
	{
		UIPanel.onPanelAdded = (Action<UIPanel>)Delegate.Combine(UIPanel.onPanelAdded, new Action<UIPanel>(AddPanel));
		UIPanel.onPanelRemoved = (Action<UIPanel>)Delegate.Combine(UIPanel.onPanelRemoved, new Action<UIPanel>(RemovePanel));
		InitPanels();
	}

	private void OnDisable()
	{
		UIPanel.onPanelAdded = (Action<UIPanel>)Delegate.Remove(UIPanel.onPanelAdded, new Action<UIPanel>(AddPanel));
		UIPanel.onPanelRemoved = (Action<UIPanel>)Delegate.Remove(UIPanel.onPanelRemoved, new Action<UIPanel>(RemovePanel));
	}

	private void LateUpdate()
	{
		if (_isDirty)
		{
			_isDirty = false;
			UpdatePanelClone();
		}
		int i = 0;
		for (int validCount = _validCount; i < validCount; i++)
		{
			_list[i].UpdatePosition();
		}
	}

	private void AddPanel(UIPanel panel)
	{
		if (!((Object)(object)Target == (Object)null) && !_panels.Contains(panel) && NGUITools.IsChild(((Component)Target).transform, ((Component)panel).transform))
		{
			panel.onDrawcallListUpdated = (Action)Delegate.Combine(panel.onDrawcallListUpdated, new Action(OnChangeDrawcall));
			_panels.Add(panel);
			OnChangeDrawcall();
		}
	}

	private void RemovePanel(UIPanel panel)
	{
		if (!((Object)(object)Target == (Object)null) && NGUITools.IsChild(((Component)Target).transform, ((Component)panel).transform))
		{
			panel.onDrawcallListUpdated = (Action)Delegate.Remove(panel.onDrawcallListUpdated, new Action(OnChangeDrawcall));
			_panels.Remove(panel);
			OnChangeDrawcall();
		}
	}

	private void ClearPanel()
	{
		int i = 0;
		for (int count = _panels.Count; i < count; i++)
		{
			UIPanel uIPanel = _panels[i];
			uIPanel.onDrawcallListUpdated = (Action)Delegate.Remove(uIPanel.onDrawcallListUpdated, new Action(OnChangeDrawcall));
		}
		_panels.Clear();
		OnChangeDrawcall();
	}

	private void InitPanels()
	{
		ClearPanel();
		if ((Object)(object)Target == (Object)null)
		{
			return;
		}
		_stack.Clear();
		_stack.Push(((Component)Target).transform);
		while (_stack.Count > 0)
		{
			Transform val = _stack.Pop();
			UIPanel component = ((Component)val).GetComponent<UIPanel>();
			if ((Object)(object)component != (Object)null)
			{
				AddPanel(component);
			}
			int i = 0;
			for (int childCount = val.childCount; i < childCount; i++)
			{
				_stack.Push(val.GetChild(i));
			}
		}
	}

	public void SetTarget(UIPanel panel)
	{
		Target = panel;
		InitPanels();
	}

	private void OnChangeDrawcall()
	{
		_isDirty = true;
	}

	private void UpdatePanelClone()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		int num = 0;
		int i = 0;
		for (int count = _panels.Count; i < count; i++)
		{
			UIPanel uIPanel = _panels[i];
			int j = 0;
			for (int count2 = uIPanel.drawCalls.Count; j < count2; j++)
			{
				UIDrawCall uIDrawCall = uIPanel.drawCalls[j];
				DrawcallClone clone = GetClone(num);
				((Component)clone.Copy).gameObject.SetActive(true);
				clone.Origin = uIDrawCall;
				MeshFilter component = ((Component)uIDrawCall).GetComponent<MeshFilter>();
				MeshRenderer component2 = ((Component)uIDrawCall).GetComponent<MeshRenderer>();
				clone.MeshFilter.sharedMesh = component.sharedMesh;
				Material val = new Material(((Renderer)component2).sharedMaterial);
				((Renderer)clone.MeshRender).sharedMaterial = val;
				clone.CopyMat = val;
				clone.UpdatePosition();
				num++;
			}
		}
		_validCount = num;
		int k = num;
		for (int count3 = _list.Count; k < count3; k++)
		{
			DeactiveClone(k);
		}
		if (CloneUpdated != null)
		{
			CloneUpdated();
		}
	}

	private DrawcallClone GetClone(int index)
	{
		if (_list.Count == index)
		{
			GameObject val = ((Component)this).gameObject.AddChild();
			((Object)val).name = "Clone";
			DrawcallClone drawcallClone = new DrawcallClone();
			drawcallClone.Origin = null;
			drawcallClone.Copy = val.transform;
			drawcallClone.MeshFilter = val.AddComponent<MeshFilter>();
			drawcallClone.MeshRender = val.AddComponent<MeshRenderer>();
			DrawcallClone item = drawcallClone;
			UIExtendEventListener uIExtendEventListener = UIExtendEventListener.Get(val);
			uIExtendEventListener.onWillRenderObject = (UIEventListener.VoidDelegate)Delegate.Combine(uIExtendEventListener.onWillRenderObject, new UIEventListener.VoidDelegate(WillRenderObject));
			_list.Add(item);
		}
		return _list[index];
	}

	private void DeactiveClone(int index)
	{
		DrawcallClone drawcallClone = _list[index];
		Material copyMat = drawcallClone.CopyMat;
		((Renderer)drawcallClone.MeshRender).sharedMaterial = null;
		Object.Destroy((Object)(object)copyMat);
		((Component)drawcallClone.Copy).gameObject.SetActive(false);
	}

	private void SetClipping(Material mat, int index, Vector4 cr, Vector2 soft, float angle)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		angle *= -(float)Math.PI / 180f;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(1000f, 1000f);
		if (soft.x > 0f)
		{
			val.x = cr.z / soft.x;
		}
		if (soft.y > 0f)
		{
			val.y = cr.w / soft.y;
		}
		if (index < _clipRange.Length)
		{
			mat.SetVector(_clipRange[index], new Vector4((0f - cr.x) / cr.z, (0f - cr.y) / cr.w, 1f / cr.z, 1f / cr.w));
			mat.SetVector(_clipArgs[index], new Vector4(val.x, val.y, Mathf.Sin(angle), Mathf.Cos(angle)));
		}
	}

	private void WillRenderObject(GameObject obj)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		DrawcallClone drawcallClone = null;
		Transform transform = obj.transform;
		int i = 0;
		for (int validCount = _validCount; i < validCount; i++)
		{
			if ((Object)(object)_list[i].Copy == (Object)(object)transform)
			{
				drawcallClone = _list[i];
				break;
			}
		}
		if (drawcallClone == null)
		{
			return;
		}
		UIPanel panel = drawcallClone.Origin.panel;
		if (panel.clipping == UIDrawCall.Clipping.TextureMask)
		{
			Vector4 drawCallClipRange = panel.drawCallClipRange;
			drawcallClone.CopyMat.SetVector(_clipRange[0], new Vector4((0f - drawCallClipRange.x) / drawCallClipRange.z, (0f - drawCallClipRange.y) / drawCallClipRange.w, 1f / drawCallClipRange.z, 1f / drawCallClipRange.w));
			drawcallClone.CopyMat.SetTexture("_ClipTex", (Texture)(object)panel.clipTexture);
			return;
		}
		UIPanel uIPanel = panel;
		int num = 0;
		while ((Object)(object)uIPanel != (Object)null)
		{
			if (uIPanel.hasClipping)
			{
				float angle = 0f;
				Vector4 drawCallClipRange2 = uIPanel.drawCallClipRange;
				if ((Object)(object)uIPanel != (Object)(object)panel)
				{
					Vector3 val = uIPanel.cachedTransform.InverseTransformPoint(panel.cachedTransform.position);
					drawCallClipRange2.x -= val.x;
					drawCallClipRange2.y -= val.y;
					Quaternion rotation = panel.cachedTransform.rotation;
					Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
					Quaternion rotation2 = uIPanel.cachedTransform.rotation;
					Vector3 eulerAngles2 = ((Quaternion)(ref rotation2)).eulerAngles;
					Vector3 val2 = eulerAngles2 - eulerAngles;
					val2.x = NGUIMath.WrapAngle(val2.x);
					val2.y = NGUIMath.WrapAngle(val2.y);
					val2.z = NGUIMath.WrapAngle(val2.z);
					if (Mathf.Abs(val2.x) > 0.001f || Mathf.Abs(val2.y) > 0.001f)
					{
					}
					angle = val2.z;
				}
				SetClipping(drawcallClone.CopyMat, num++, drawCallClipRange2, uIPanel.clipSoftness, angle);
			}
			uIPanel = uIPanel.parentPanel;
		}
	}
}
