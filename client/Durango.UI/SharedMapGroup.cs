using Durango.Player;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class SharedMapGroup : UIBase
{
	[SerializeField]
	private SharedMapContext _sharedMapContext;

	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private UILabel _regionName;

	[SerializeField]
	private WorldMapScaleInfo _scaleInfo;

	private string _entityId;

	private string _regionId;

	private void Start()
	{
		UIEventListener.Get(_closeButton).onClick = delegate
		{
			UIBase.CloseAllUI();
		};
		base.OnOpenSucceed += delegate
		{
			GameSystem<InputSystem>.Instance().On(InputCommand.GesturePanning, OnGesturePanningProcess);
			GameSystem<InputSystem>.Instance().On(InputCommand.GestureZoom, OnGestureZoomProcess);
		};
		base.OnCloseSucceed += delegate
		{
			_regionId = string.Empty;
			_entityId = string.Empty;
			GameSystem<InputSystem>.Instance().Off(InputCommand.GesturePanning, OnGesturePanningProcess);
			GameSystem<InputSystem>.Instance().Off(InputCommand.GestureZoom, OnGestureZoomProcess);
		};
		_sharedMapContext.ZoomChanged += RefreshScaleInfo;
		TryClose();
	}

	public void Open(string regionId, string regionName, string entityId, Vector2 pinPoint)
	{
		_regionId = regionId;
		_entityId = entityId;
		_regionName.text = regionName;
		_sharedMapContext.Load(regionId, delegate
		{
			if (!(_regionId != regionId) && !(_entityId != entityId))
			{
				_sharedMapContext.FocusToTilePostion(pinPoint);
				Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(PlayerInfo info)
				{
					if (info.Valid && !(_regionId != regionId) && !(_entityId != entityId))
					{
						_sharedMapContext.SetPinInfo(info, pinPoint);
					}
				});
				RefreshScaleInfo();
				Open();
			}
		});
	}

	private void RefreshScaleInfo()
	{
		_scaleInfo.Refresh(_sharedMapContext.ZoomScale, (float)_sharedMapContext.MapSize * 200f / 100f / 1280f);
	}

	private void OnGesturePanningProcess(InputCommandMessage message)
	{
		Vector3 gestureVector = message.GestureVector;
		_sharedMapContext.Offset += new Vector2(gestureVector.x, gestureVector.y);
		GameSystem<InputSystem>.Instance().Gesture.NotifyGestureProcessed();
	}

	private void OnGestureZoomProcess(InputCommandMessage message)
	{
		Vector3 gestureVector = message.GestureVector;
		_sharedMapContext.Zoom(gestureVector.z, new Vector2(gestureVector.x, gestureVector.y));
		GameSystem<InputSystem>.Instance().Gesture.NotifyGestureProcessed();
		GameSystem<InputSystem>.Instance().StopPropagation();
	}
}
