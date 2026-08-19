using System;
using Building;
using Durango.Logic.Clusters;
using Durango.Logic.Timer;
using Durango.MotionInfo;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.UI.Control;
using Durango.UI.InGame;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;

namespace Durango.UI;

public class BuildGridGroupBase : UIBase
{
	public struct Arguments
	{
		public Action<BuildSystem.GridResult> Confirmed;

		public string WarningAreaInAndOut;

		public string WarningEstateOut;

		public string Comment;

		public BuildLocator.Arguments Args;
	}

	private const float GridZoom = 0.42f;

	[SerializeField]
	protected SelectableWidget _confirmGridSelectionButton;

	[SerializeField]
	protected SelectableWidget _cancelGridSelectionButton;

	[SerializeField]
	protected SelectableWidget _rotatePreviewButton;

	protected Vector3[] _buttonPositions;

	protected string _commentTitle;

	protected string _comment;

	[SerializeField]
	private UIWidget _commentWidget;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private Transform _buttonContainer;

	private Arguments _arguments;

	private bool _isZoomOut;

	private float _prevZoom;

	protected override bool IsSoundOcclusion => false;

	protected virtual void Start()
	{
		_confirmGridSelectionButton.SetClickSound(UISound.ClickType.InteractionMenuDefault);
		_cancelGridSelectionButton.SetClickSound(UISound.ClickType.InteractionMenuDefault);
		_rotatePreviewButton.SetClickSound(UISound.ClickType.InteractionMenuDefault);
		Singleton<BuildLocator>.Instance().PreviewPositionUpdated += BuildLocatorPreviewPositionUpdated;
		_confirmGridSelectionButton.Clicked = ConfirmGridSelection_OnClick;
		_cancelGridSelectionButton.Clicked = OnCanceled;
		_rotatePreviewButton.Clicked = RotatePreview_OnClick;
		GameSystem<BuildSystem>.Instance().OccupyTimer.Started += OnStartOccupyTimer;
		GameSystem<BuildSystem>.Instance().BuildTimer.Started += OnStartBuildTimer;
		GameSystem<BuildSystem>.Instance().BuildTimer.Ended += OnEndedBuildTimer;
		base.OnOpenSucceed += delegate
		{
			SetZoomOutMode(enable: true);
		};
		base.OnCloseSucceed += delegate
		{
			SetZoomOutMode(enable: false);
			Singleton<BuildLocator>.Instance().ResetBuildingMode();
		};
		_buttonPositions = new Vector3[3];
		ref Vector3 reference = ref _buttonPositions[0];
		reference = _rotatePreviewButton.transform.localPosition;
		ref Vector3 reference2 = ref _buttonPositions[1];
		reference2 = _confirmGridSelectionButton.transform.localPosition;
		ref Vector3 reference3 = ref _buttonPositions[2];
		reference3 = _cancelGridSelectionButton.transform.localPosition;
		SetChildrenActive(activated: false);
		_comment = T._("건물이 온전히 사유지 안에 있어야 보호를 받습니다.");
	}

	public void Open([NotNull] Blueprint blueprint, Action<BuildSystem.GridResult> onConfirm)
	{
		Open(blueprint, null, null, hasRoof: true, null, onConfirm);
	}

	public void Open([NotNull] Blueprint blueprint, Point2? size, int? stories, bool hasRoof, ArtifactDisplay? display, Action<BuildSystem.GridResult> onConfirm)
	{
		if (GameManager.Region.Role() == Role.Tutorial)
		{
			UIManager.SystemMsg(T._("앙코라에서는 건설을 할 수 없습니다."));
			return;
		}
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if (localPlayer == null)
		{
			return;
		}
		RectInt? area = null;
		int? floor = null;
		if ((byte)localPlayer.Floor > 0)
		{
			TileObject tileObject = localPlayer.GetTileObject();
			if (tileObject == null)
			{
				return;
			}
			Artifact artifact = tileObject.Artifact;
			if (artifact == null || artifact.Stories == null)
			{
				return;
			}
			int? value = artifact.Stories.Value;
			if (value.HasValue && value.GetValueOrDefault() <= (byte)localPlayer.Floor)
			{
				return;
			}
			area = new RectInt(artifact.WorldTile, artifact.Size);
			floor = (byte)localPlayer.Floor;
		}
		BuildLocator.Arguments args = BuildLocator.Arguments.MakeFrom(blueprint);
		args.Size = size.GetValueOrDefault(blueprint.Size);
		args.Display = display;
		args.Area = area;
		args.Floor = floor;
		args.Stories = stories;
		args.HasRoof = hasRoof;
		Open(new Arguments
		{
			Confirmed = onConfirm,
			Comment = T._("[size=32]{0}[/size]{1}[D9D9DB]{2}[-]", _commentTitle, string.IsNullOrEmpty(_commentTitle) ? string.Empty : "\n", _comment),
			WarningAreaInAndOut = T._("실내외 경계에는 설치할 수 없습니다."),
			WarningEstateOut = T._("정말 건물을 사유지 밖에 건설하시겠습니까?\n[size=24]<alert><alert_icon/> 부족 영토와 사유지 바깥에 배치된 건축물은 포장 시 비용이 발생합니다.</alert>[/size]"),
			Args = args
		});
	}

	public void Open(Arguments arguments)
	{
		Set(arguments);
		Open();
	}

	private void Set(Arguments arguments)
	{
		_arguments = arguments;
		SetComment(_arguments.Comment);
		SetButtons(_arguments.Args.RotatableDirections > 0);
		Singleton<BuildLocator>.Instance().SetArtifactBuildingMode(arguments.Args);
	}

	protected virtual void SetButtons(bool rotatable)
	{
		_rotatePreviewButton.gameObject.SetActive(rotatable);
	}

	private void SetZoomOutMode(bool enable)
	{
		if (_isZoomOut != enable)
		{
			_isZoomOut = enable;
			if (enable)
			{
				_prevZoom = Singleton<MainCamera>.Instance().Zoom;
				Singleton<CameraController>.Instance().ZoomRange(0.42f, 0.42f, 0.3f).Zoom(0.42f, 0.3f)
					.LockZoomControl(isLock: true);
			}
			else
			{
				Singleton<CameraController>.Instance().ZoomRange(0.42f, 2.2f, 0.3f).Zoom(_prevZoom, 0.3f)
					.LockZoomControl(isLock: false);
			}
		}
	}

	private void OnConfirm()
	{
		BuildSystem.GridResult result = Singleton<BuildLocator>.Instance().GetResult();
		if (!Debug.isDebugBuild || !Input.GetKey(KeyCode.LeftControl))
		{
			UIBase.CloseAllUI();
		}
		if (_arguments.Confirmed != null)
		{
			_arguments.Confirmed(result);
		}
	}

	protected void OnCanceled()
	{
		Close();
	}

	private void OnStartOccupyTimer(PredictTimer timer)
	{
		IconProgressGauge iconProgressGauge = Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer.Timer);
		string occupiedBlueprintId = GameSystem<BuildSystem>.Instance().OccupiedBlueprintId;
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(occupiedBlueprintId);
		iconProgressGauge.AddIcon((blueprint != null) ? blueprint.Icon : "icon_question");
		MotionMap.Instance().GetBuildMotion("Occupy", null, out var motion, out var equip);
		timer.SetMotion(motion, equip);
	}

	private void OnStartBuildTimer(PredictTimer timer)
	{
		BuildSlotContainer slotContainer = GameSystem<BuildSystem>.Instance().SlotContainer;
		ArtifactSiteDecoration siteDecoration = slotContainer.Artifact.GetSiteDecoration();
		if (siteDecoration != null)
		{
			siteDecoration.Visible(visible: false);
		}
		Artifact artifact = slotContainer.Artifact;
		Blueprint blueprint = slotContainer.Blueprint;
		IconProgressGauge iconProgressGauge = Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer.Timer);
		iconProgressGauge.SetTarget(artifact.gameObject, artifact.InteractionPosition - artifact.transform.position);
		iconProgressGauge.AddIcon(blueprint.Icon);
		Singleton<PlayerController>.Instance().RotateToPosition(artifact.InteractionPosition);
		MotionMap.Instance().GetBuildMotion(slotContainer.Blueprint.Id, null, out var motion, out var equip);
		timer.SetMotion(motion, equip);
	}

	private void OnEndedBuildTimer(PredictTimer timer)
	{
		BuildSlotContainer slotContainer = GameSystem<BuildSystem>.Instance().SlotContainer;
		ArtifactSiteDecoration artifactSiteDecoration = ((!(slotContainer.Artifact == null)) ? slotContainer.Artifact.GetSiteDecoration() : null);
		if (artifactSiteDecoration != null)
		{
			artifactSiteDecoration.Visible(visible: true);
		}
	}

	protected void ConfirmGridSelection_OnClick()
	{
		if (BuildLocator.IsAreaInAndOut)
		{
			if (!string.IsNullOrEmpty(_arguments.WarningAreaInAndOut))
			{
				UIManager.SystemMsg(_arguments.WarningAreaInAndOut);
			}
		}
		else if (!string.IsNullOrEmpty(_arguments.WarningEstateOut) && BuildLocator.CurrentGridMaxState == BuildLocator.BuildGridState.Estate && BuildLocator.CurrentGridMinState < BuildLocator.CurrentGridMaxState)
		{
			UIManager.MessageBox.Show(_arguments.WarningEstateOut, delegate(bool ok)
			{
				if (ok)
				{
					OnConfirm();
				}
			});
		}
		else if (_arguments.Args.Blueprint.TimeLimited && GameManager.ClusterMode == Mode.Online)
		{
			UIManager.MessageBox.Show(T._("유효기간 동안만 이용이 가능한 건축물입니다. 건설하시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					OnConfirm();
				}
			});
		}
		else
		{
			OnConfirm();
		}
	}

	protected void RotatePreview_OnClick()
	{
		Singleton<BuildLocator>.Instance().RotatePreview();
	}

	private void SetComment(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			_commentWidget.gameObject.SetActive(value: false);
		}
		_commentWidget.gameObject.SetActive(value: true);
		_commentLabel.text = text;
	}

	private void BuildLocatorPreviewPositionUpdated()
	{
		Vector3 vector = Util.TilePositionToClientPosition(Singleton<BuildLocator>.Instance().WorldTilePos);
		_confirmGridSelectionButton.Widget.alpha = ((BuildLocator.CurrentGridMinState != 0) ? 1f : 0.5f);
		_confirmGridSelectionButton.Disabled = BuildLocator.CurrentGridMinState == BuildLocator.BuildGridState.Invalid;
		Vector3 localPosition = MainCamera.WorldToNGUIPos(vector + (Vector3.left + Vector3.back) * 0.1f * 200f);
		float num = MainCamera.NGUIScale();
		localPosition.x = Mathf.Clamp(localPosition.x, (float)(-Screen.width) * num * 0.5f + 160f, (float)Screen.width * num * 0.5f - 160f);
		localPosition.y = Mathf.Clamp(localPosition.y, (float)(-Screen.height) * num * 0.5f + 120f, (float)Screen.height * num * 0.5f - 60f);
		_buttonContainer.localPosition = localPosition;
	}
}
