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
			Vector3 position = Origin.cachedTransform.position;
			Vector3 lossyScale = Origin.cachedTransform.lossyScale;
			position = new Vector3(position.x / lossyScale.x, position.y / lossyScale.y, position.z / lossyScale.z);
			Copy.localPosition = position;
		}

		public void UpdateArgument()
		{
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
		if (!(Target == null) && !_panels.Contains(panel) && NGUITools.IsChild(Target.transform, panel.transform))
		{
			panel.onDrawcallListUpdated = (Action)Delegate.Combine(panel.onDrawcallListUpdated, new Action(OnChangeDrawcall));
			_panels.Add(panel);
			OnChangeDrawcall();
		}
	}

	private void RemovePanel(UIPanel panel)
	{
		if (!(Target == null) && NGUITools.IsChild(Target.transform, panel.transform))
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
		if (Target == null)
		{
			return;
		}
		_stack.Clear();
		_stack.Push(Target.transform);
		while (_stack.Count > 0)
		{
			Transform transform = _stack.Pop();
			UIPanel component = transform.GetComponent<UIPanel>();
			if (component != null)
			{
				AddPanel(component);
			}
			int i = 0;
			for (int childCount = transform.childCount; i < childCount; i++)
			{
				_stack.Push(transform.GetChild(i));
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
				clone.Copy.gameObject.SetActive(value: true);
				clone.Origin = uIDrawCall;
				MeshFilter component = uIDrawCall.GetComponent<MeshFilter>();
				MeshRenderer component2 = uIDrawCall.GetComponent<MeshRenderer>();
				clone.MeshFilter.sharedMesh = component.sharedMesh;
				Material material = new Material(component2.sharedMaterial);
				clone.MeshRender.sharedMaterial = material;
				clone.CopyMat = material;
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
			GameObject gameObject = base.gameObject.AddChild();
			gameObject.name = "Clone";
			DrawcallClone item = new DrawcallClone
			{
				Origin = null,
				Copy = gameObject.transform,
				MeshFilter = gameObject.AddComponent<MeshFilter>(),
				MeshRender = gameObject.AddComponent<MeshRenderer>()
			};
			UIExtendEventListener uIExtendEventListener = UIExtendEventListener.Get(gameObject);
			uIExtendEventListener.onWillRenderObject = (UIEventListener.VoidDelegate)Delegate.Combine(uIExtendEventListener.onWillRenderObject, new UIEventListener.VoidDelegate(WillRenderObject));
			_list.Add(item);
		}
		return _list[index];
	}

	private void DeactiveClone(int index)
	{
		DrawcallClone drawcallClone = _list[index];
		Material copyMat = drawcallClone.CopyMat;
		drawcallClone.MeshRender.sharedMaterial = null;
		UnityEngine.Object.Destroy(copyMat);
		drawcallClone.Copy.gameObject.SetActive(value: false);
	}

	private void SetClipping(Material mat, int index, Vector4 cr, Vector2 soft, float angle)
	{
		angle *= -(float)Math.PI / 180f;
		Vector2 vector = new Vector2(1000f, 1000f);
		if (soft.x > 0f)
		{
			vector.x = cr.z / soft.x;
		}
		if (soft.y > 0f)
		{
			vector.y = cr.w / soft.y;
		}
		if (index < _clipRange.Length)
		{
			mat.SetVector(_clipRange[index], new Vector4((0f - cr.x) / cr.z, (0f - cr.y) / cr.w, 1f / cr.z, 1f / cr.w));
			mat.SetVector(_clipArgs[index], new Vector4(vector.x, vector.y, Mathf.Sin(angle), Mathf.Cos(angle)));
		}
	}

	private void WillRenderObject(GameObject obj)
	{
		DrawcallClone drawcallClone = null;
		Transform transform = obj.transform;
		int i = 0;
		for (int validCount = _validCount; i < validCount; i++)
		{
			if (_list[i].Copy == transform)
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
		if (panel != null && panel.clipping == UIDrawCall.Clipping.TextureMask)
		{
			Vector4 drawCallClipRange = panel.drawCallClipRange;
			drawcallClone.CopyMat.SetVector(_clipRange[0], new Vector4((0f - drawCallClipRange.x) / drawCallClipRange.z, (0f - drawCallClipRange.y) / drawCallClipRange.w, 1f / drawCallClipRange.z, 1f / drawCallClipRange.w));
			drawcallClone.CopyMat.SetTexture("_ClipTex", panel.clipTexture);
			return;
		}
		UIPanel uIPanel = panel;
		int num = 0;
		while (uIPanel != null)
		{
			if (uIPanel.hasClipping)
			{
				float angle = 0f;
				Vector4 drawCallClipRange2 = uIPanel.drawCallClipRange;
				if (uIPanel != panel)
				{
					Vector3 vector = uIPanel.cachedTransform.InverseTransformPoint(panel.cachedTransform.position);
					drawCallClipRange2.x -= vector.x;
					drawCallClipRange2.y -= vector.y;
					Vector3 eulerAngles = panel.cachedTransform.rotation.eulerAngles;
					Vector3 vector2 = uIPanel.cachedTransform.rotation.eulerAngles - eulerAngles;
					vector2.x = NGUIMath.WrapAngle(vector2.x);
					vector2.y = NGUIMath.WrapAngle(vector2.y);
					vector2.z = NGUIMath.WrapAngle(vector2.z);
					if (!(Mathf.Abs(vector2.x) > 0.001f))
					{
						Mathf.Abs(vector2.y);
						_ = 0.001f;
					}
					angle = vector2.z;
				}
				SetClipping(drawcallClone.CopyMat, num++, drawCallClipRange2, uIPanel.clipSoftness, angle);
			}
			uIPanel = uIPanel.parentPanel;
		}
	}
}
