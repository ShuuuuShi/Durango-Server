using Building;
using Durango.Logic.Map;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class MissionAlertTargetWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private GameObject _idleIcon;

	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private UISprite _arrow;

	[SerializeField]
	private UISprite _bg;

	[SerializeField]
	private string _component;

	[SerializeField]
	private Vector2 _offset;

	private FactionSystem.MissionState _currentState;

	private Vector3 _targetPos;

	private MapIndicator _indicator;

	private bool _initedArtifact;

	public string Component => _component;

	private void Awake()
	{
		_currentState = FactionSystem.MissionState.Disabled;
		_icon.color = PresetColor.UIYellow;
		base.gameObject.SetActive(value: false);
		UpdateSprite(active: false);
	}

	private void LateUpdate()
	{
		if (_currentState != 0 && _initedArtifact)
		{
			Vector3 targetPos = _targetPos;
			Vector2 offset = _offset;
			targetPos.x += offset.x;
			targetPos.y += offset.y;
			Vector3 localPosition = MainCamera.WorldToNGUIPos(targetPos);
			Vector3 vector = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			if (localPosition.sqrMagnitude < vector.sqrMagnitude)
			{
				UpdateSprite(active: true);
				base.transform.localPosition = localPosition;
			}
			else
			{
				UpdateSprite(active: false);
			}
		}
	}

	public void InitArtifact(Artifact artifact)
	{
		if (!_initedArtifact)
		{
			_targetPos = artifact.InteractionPosition;
			_initedArtifact = true;
		}
	}

	public void Release()
	{
		_initedArtifact = false;
	}

	public void Set(FactionSystem.MissionState state)
	{
		if (_currentState != state)
		{
			_currentState = state;
			Refresh();
		}
	}

	public void Refresh()
	{
		switch (_currentState)
		{
		case FactionSystem.MissionState.Ready:
			Show();
			break;
		case FactionSystem.MissionState.Idle:
			Idle();
			break;
		case FactionSystem.MissionState.Disabled:
			Hide();
			break;
		}
	}

	public void UpdateIndicator()
	{
		if (_indicator != null)
		{
			return;
		}
		for (int i = 0; i < TerrainMeta.Indicators.Count; i++)
		{
			Indicator indicator = TerrainMeta.Indicators[i];
			Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(indicator.EntityType);
			if (blueprint != null && blueprint.HasComponent(Component) && blueprint.Indicator != null)
			{
				Point2 point = new Point2(indicator.Tile[0], indicator.Tile[1]);
				MapIndicator indicator2 = Singleton<MapIndicators>.Instance().GetIndicator(point.ToString(), IndicatorType.Artifact);
				if (indicator2 != null)
				{
					_indicator = indicator2;
					break;
				}
			}
		}
	}

	public bool IsInitedArtifact()
	{
		return _initedArtifact;
	}

	private void Show()
	{
		if (_indicator != null)
		{
			Singleton<MapIndicators>.Instance().AddAreaEffectIndicator(_indicator, PresetColor.UIYellow, 32f, 0f, fixedScale: true);
		}
		if (_initedArtifact)
		{
			base.gameObject.SetActive(value: true);
			_border.color = PresetColor.UIYellow;
			_arrow.color = PresetColor.UIYellow;
			UpdateSprite(active: true);
		}
	}

	private void Idle()
	{
		if (_indicator != null)
		{
			Singleton<MapIndicators>.Instance().RemoveAreaEffectIndicator(_indicator);
		}
		if (_initedArtifact)
		{
			base.gameObject.SetActive(value: true);
			_border.color = PresetColor.UIWhite;
			_arrow.color = PresetColor.UIWhite;
			UpdateSprite(active: true);
		}
	}

	private void Hide()
	{
		if (_indicator != null)
		{
			Singleton<MapIndicators>.Instance().RemoveAreaEffectIndicator(_indicator);
		}
		if (_initedArtifact)
		{
			base.gameObject.SetActive(value: false);
			UpdateSprite(active: false);
		}
	}

	private void UpdateSprite(bool active)
	{
		_icon.gameObject.SetActive(active && _currentState == FactionSystem.MissionState.Ready);
		_idleIcon.gameObject.SetActive(active && _currentState == FactionSystem.MissionState.Idle);
		_border.gameObject.SetActive(active);
		_arrow.gameObject.SetActive(active);
		_bg.gameObject.SetActive(active);
	}
}
