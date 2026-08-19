using System;
using Player;
using UnityEngine;

public class SharedMapGroup : UIBase
{
	[SerializeField]
	private SharedMapContext _sharedMapContext;

	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private GameObject _worldMapBG;

	[SerializeField]
	private UILabel _regionName;

	[SerializeField]
	private WorldMapScaleInfo _scaleInfo;

	private ulong _entityId;

	private void Start()
	{
		UIEventListener.Get(_closeButton).onClick = delegate
		{
			ForceClose();
		};
		base.OnOpenSucceed += delegate
		{
			KSingleton<PlayerController>.Instance().IsGestureProcessed += PlayerController_IsGestureProcessed;
		};
		base.OnCloseSucceed += delegate
		{
			KSingleton<PlayerController>.Instance().IsGestureProcessed -= PlayerController_IsGestureProcessed;
		};
		_sharedMapContext.InitFinished += SharedMapContextInitFinished;
		_sharedMapContext.ZoomChanged += SharedMapContextZoomChanged;
		UIEventListener uIEventListener = UIEventListener.Get(_worldMapBG);
		uIEventListener.onDrag = OnDragWorldMap;
		OnClose();
	}

	public void Open(ulong regionId, string regionName, ulong entityId, Vector2 posPinPoint)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		_entityId = entityId;
		_sharedMapContext.InitTerrain(regionId, posPinPoint);
		_regionName.text = LocalizeSystem.Format("#worldmap_region_name_label", regionName);
	}

	private void SharedMapContextInitFinished()
	{
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_entityId, delegate(PlayerInfo info)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (info.Valid)
			{
				_sharedMapContext.SetPinPoint(info, _sharedMapContext.FocusPoint);
				RefreshScaleInfo();
				Open();
				MoveToFocusPoint();
			}
		});
	}

	private void SharedMapContextZoomChanged()
	{
		_sharedMapContext.RefreshPinPosition();
		RefreshScaleInfo();
	}

	private void MoveToFocusPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		Vector2 focusPoint = _sharedMapContext.FocusPoint;
		int num = _sharedMapContext.MapSize / 2;
		focusPoint.x -= (float)num;
		focusPoint.y -= (float)num;
		Vector2 val = focusPoint / (float)_sharedMapContext.MapSize * (float)_sharedMapContext.MapNGUISize;
		float num2 = Mathf.Sin((float)Math.PI / 4f);
		float num3 = Mathf.Cos((float)Math.PI / 4f);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(val.x * num3 - val.y * num2, val.x * num2 + val.y * num3);
		val2 *= _sharedMapContext.ZoomScale;
		_sharedMapContext.Offset = -val2;
	}

	private void RefreshScaleInfo()
	{
		_scaleInfo.Refresh(_sharedMapContext.ZoomScale, (float)_sharedMapContext.MapSize * 200f / 100f / 1280f);
	}

	private void PlayerController_IsGestureProcessed(PlayerController.Gesture type, Vector3 pos, bool touchedUI, ref bool result)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (type == PlayerController.Gesture.Zoom)
		{
			_sharedMapContext.Zoom(pos.z, new Vector2(pos.x, pos.y));
		}
		result = true;
	}

	private void OnDragWorldMap(GameObject obj, Vector2 delta)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		SharedMapContext sharedMapContext = _sharedMapContext;
		sharedMapContext.Offset += delta;
	}
}
