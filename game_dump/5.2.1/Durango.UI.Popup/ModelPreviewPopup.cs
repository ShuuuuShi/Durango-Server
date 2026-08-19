using Durango.UI.Control;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class ModelPreviewPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIModelViewer _previewTexture;

	public override bool DragLock => true;

	protected override void OnShow()
	{
		base.OnShow();
		GameSystem<InputSystem>.Instance().On(InputCommand.GestureZoom, OnGestureZoomProcess);
		GameSystem<InputSystem>.Instance().On(InputCommand.GesturePanning, OnGesturePanningProcess);
	}

	protected override void OnHide()
	{
		base.OnHide();
		GameSystem<InputSystem>.Instance().Off(InputCommand.GestureZoom, OnGestureZoomProcess);
		GameSystem<InputSystem>.Instance().Off(InputCommand.GesturePanning, OnGesturePanningProcess);
	}

	public void Show(ArtifactPreview preview, [NotNull] string title)
	{
		_titleLabel.text = title;
		_previewTexture.SetArtifactModel(new UIModelViewer.ArtifactArguments
		{
			Display = preview.Display,
			Size = preview.Size,
			Rotation = preview.Rotation,
			IsModular = preview.IsModular
		}, new UIModelViewer.Arguments
		{
			CameraAngle = 35f,
			Rotation = -45f
		});
		Show();
	}

	private void OnGestureZoomProcess(InputCommandMessage message)
	{
		if (_previewTexture.ModelRender != null)
		{
			Vector3 gestureVector = message.GestureVector;
			_previewTexture.ModelRender.Zoom(gestureVector.z, new Vector2(gestureVector.x, gestureVector.y));
		}
		GameSystem<InputSystem>.Instance().Gesture.NotifyGestureProcessed();
		GameSystem<InputSystem>.Instance().StopPropagation();
	}

	private void OnGesturePanningProcess(InputCommandMessage message)
	{
		if (_previewTexture.ModelRender != null)
		{
			_previewTexture.ModelRender.Panning(message.GestureVector);
		}
		GameSystem<InputSystem>.Instance().Gesture.NotifyGestureProcessed();
	}
}
