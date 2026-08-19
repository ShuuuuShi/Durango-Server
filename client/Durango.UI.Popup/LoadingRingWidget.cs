using Durango.Render.Camera;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Popup;

public class LoadingRingWidget : MonoBehaviour
{
	public enum Mode
	{
		Hide,
		UIWidget,
		InteractionTarget,
		ClientPosition
	}

	private abstract class Positioner
	{
		protected readonly GameObject Ring;

		protected Vector3 Offset = Vector3.zero;

		public abstract bool IsValid { get; }

		protected Positioner([NotNull] GameObject ring)
		{
			Ring = ring;
		}

		public abstract void UpdatePosition();
	}

	private class WidgetPositioner : Positioner
	{
		private readonly Transform _parentTransform;

		public GameObject ParentWidget { get; private set; }

		public override bool IsValid => ParentWidget != null && ParentWidget.activeInHierarchy;

		public WidgetPositioner([NotNull] GameObject ring, [NotNull] Transform parentTransform)
			: base(ring)
		{
			_parentTransform = parentTransform;
		}

		public void Set(GameObject parentWidget, Vector3 offset)
		{
			ParentWidget = parentWidget;
			Offset = offset;
		}

		public override void UpdatePosition()
		{
			if (ParentWidget != null)
			{
				Ring.transform.localPosition = _parentTransform.InverseTransformPoint(ParentWidget.transform.position) + Offset;
			}
		}
	}

	private class TargetPositioner : Positioner
	{
		public override bool IsValid => GameSystem<InteractionSystem>.Instance().Target != null;

		public TargetPositioner([NotNull] GameObject ring)
			: base(ring)
		{
		}

		public void Set(Vector3 offset)
		{
			Offset = offset;
		}

		public override void UpdatePosition()
		{
			InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
			if (target != null)
			{
				Ring.transform.localPosition = MainCamera.WorldToNGUIPos(target.Position) + Offset;
			}
		}
	}

	private class WorldPositioner : Positioner
	{
		private Vector3 _position = Vector3.zero;

		public override bool IsValid => true;

		public WorldPositioner([NotNull] GameObject ring)
			: base(ring)
		{
		}

		public void Set(Vector3 position, Vector3 offset)
		{
			_position = position;
			Offset = offset;
		}

		public override void UpdatePosition()
		{
			Ring.transform.localPosition = MainCamera.WorldToNGUIPos(_position) + Offset;
		}
	}

	private const int MinDepth = 1;

	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private TweenAlpha _tweenAlpha;

	[SerializeField]
	private UIPanel _panel;

	private bool _initialized;

	private Transform _parentTransform;

	private WidgetPositioner _widgetPositioner;

	private TargetPositioner _targetPositioner;

	private WorldPositioner _worldPositioner;

	private Positioner _currentPositioner;

	public Mode AttachMode { get; private set; }

	private void LateUpdate()
	{
		if (_currentPositioner != null)
		{
			if (_currentPositioner.IsValid)
			{
				_currentPositioner.UpdatePosition();
			}
			else
			{
				Hide();
			}
		}
	}

	public void Init()
	{
		if (!_initialized)
		{
			_parentTransform = _panel.transform;
			_widgetPositioner = new WidgetPositioner(base.gameObject, _parentTransform);
			_targetPositioner = new TargetPositioner(base.gameObject);
			_worldPositioner = new WorldPositioner(base.gameObject);
			Hide();
			_initialized = true;
		}
	}

	public void AttachToWidget([NotNull] GameObject parentWidget, Vector3? offset = null)
	{
		_widgetPositioner.Set(parentWidget, (!offset.HasValue) ? Vector3.zero : offset.Value);
		_panel.gameObject.layer = parentWidget.layer;
		_panel.depth = GetPanelDepth(parentWidget);
		Show(Mode.UIWidget, _widgetPositioner);
	}

	public void AttachToWidget([NotNull] GameObject targetWidget, [NotNull] GameObject parentWidget, Vector3? offset = null)
	{
		Vector3 offset2 = _parentTransform.InverseTransformPoint(targetWidget.transform.position - parentWidget.transform.position);
		offset2 += ((!offset.HasValue) ? Vector3.zero : offset.Value);
		_widgetPositioner.Set(parentWidget, offset2);
		_panel.gameObject.layer = parentWidget.layer;
		_panel.depth = GetPanelDepth(parentWidget);
		Show(Mode.UIWidget, _widgetPositioner);
	}

	public void DetachFromWidget(GameObject parentWidget)
	{
		if (AttachMode == Mode.UIWidget && _widgetPositioner.ParentWidget == parentWidget)
		{
			Hide();
		}
	}

	public void AttachToInteractionTarget(Vector3? offset = null)
	{
		_targetPositioner.Set((!offset.HasValue) ? Vector3.zero : offset.Value);
		_panel.gameObject.layer = LayerHelper.UILayer;
		_panel.depth = 1;
		Show(Mode.InteractionTarget, _targetPositioner);
	}

	public void AttachToClientPosition(Vector3 position, Vector3? offset = null)
	{
		_worldPositioner.Set(position, (!offset.HasValue) ? Vector3.zero : offset.Value);
		_panel.gameObject.layer = LayerHelper.UILayer;
		_panel.depth = 1;
		Show(Mode.ClientPosition, _worldPositioner);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		_currentPositioner = null;
		AttachMode = Mode.Hide;
	}

	public void ShowInstantly()
	{
		_tweenAlpha.enabled = false;
		_widget.alpha = 1f;
	}

	private void Show(Mode mode, Positioner positioner)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
			_widget.alpha = 0f;
			_tweenAlpha.tweenFactor = 0f;
			_tweenAlpha.PlayForward();
		}
		AttachMode = mode;
		_currentPositioner = positioner;
		_currentPositioner.UpdatePosition();
	}

	private int GetPanelDepth(GameObject widget)
	{
		int num = 1;
		if (LayerHelper.IsUILayer(widget.layer))
		{
			UIPanel componentInParent = widget.GetComponentInParent<UIPanel>();
			if (componentInParent != null)
			{
				num += componentInParent.depth;
			}
		}
		return num;
	}
}
