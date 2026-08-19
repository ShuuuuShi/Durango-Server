using System;
using UnityEngine;

namespace Durango.UI;

public class GameCursorManager : MonoBehaviour
{
	[Serializable]
	private class GameCursor
	{
		[SerializeField]
		public Texture2D Normal;

		[SerializeField]
		public Texture2D Clicked;

		[SerializeField]
		public Texture2D Disabled;

		[SerializeField]
		public Vector2 Hotspot;
	}

	[EnumList(typeof(GameCursorType), false, 0, -1)]
	[SerializeField]
	private GameCursor[] _cursors;

	private GameCursorType _currentType;

	private GameCursorState _currentState;

	private bool _locked;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		GameCursorUtil.SetGameCursorManager(this);
		GameManager.Started += delegate
		{
			GameSystem<InputSystem>.Instance().On(InputCommand.HoverPicking, OnPickObject);
			GameSystem<InputSystem>.Instance().On(InputCommand.PressPicking, OnPressObject);
			GameSystem<InputSystem>.Instance().On(InputCommand.ReleasePicking, OnReleaseObject);
		};
		SetState(GameCursorState.Normal, force: true);
	}

	public void SetState(GameCursorState state, bool force = false)
	{
		if (_currentState != state || force)
		{
			GameCursor gameCursor = _cursors[(int)_currentType];
			switch (state)
			{
			case GameCursorState.Normal:
				Cursor.SetCursor(gameCursor.Normal, gameCursor.Hotspot, CursorMode.Auto);
				break;
			case GameCursorState.Clicked:
				Cursor.SetCursor(gameCursor.Clicked, gameCursor.Hotspot, CursorMode.Auto);
				break;
			case GameCursorState.Disabled:
				Cursor.SetCursor(gameCursor.Disabled, gameCursor.Hotspot, CursorMode.Auto);
				break;
			}
			_currentState = state;
		}
	}

	public void SetType(GameCursorType cursorType)
	{
		if (!_locked && _currentType != cursorType && _currentState != GameCursorState.Clicked)
		{
			_currentType = cursorType;
			SetState(_currentState, force: true);
		}
	}

	public void SetLocked(bool locked)
	{
		if (_locked != locked)
		{
			if (locked)
			{
				SetType(GameCursorType.Normal);
			}
			_locked = locked;
		}
	}

	public void SetSelectMode(bool isSelect)
	{
		GameCursor gameCursor = _cursors[(int)_currentType];
		Cursor.SetCursor((!isSelect) ? gameCursor.Normal : gameCursor.Clicked, gameCursor.Hotspot, CursorMode.Auto);
	}

	public void SetVisible(bool isVisible)
	{
		Cursor.visible = isVisible;
	}

	public bool IsVisible()
	{
		return Cursor.visible;
	}

	private void OnPressObject(InputCommandMessage message)
	{
		SetState(GameCursorState.Clicked);
	}

	private void OnReleaseObject(InputCommandMessage message)
	{
		SetState(GameCursorState.Normal);
	}

	private void OnPickObject(InputCommandMessage message)
	{
		if (message.PickingTouchEvent.IsNguiTouched)
		{
			GameObject hoveredObject = UICamera.hoveredObject;
			IUICursorChangable iUICursorChangable = ((!(hoveredObject == null)) ? hoveredObject.GetComponent<IUICursorChangable>() : null);
			if (iUICursorChangable == null || !iUICursorChangable.IsCursorChangable())
			{
				SetType(GameCursorType.Normal);
				return;
			}
			GameCursorType cursorType = GameCursorType.Normal;
			if (iUICursorChangable.IsCursorSpecified(ref cursorType))
			{
				SetType(cursorType);
				return;
			}
		}
		bool isPrev;
		GameObject gameObject = InteractionUtil.PickingObject(null, message.PickingRay, message.PickingTouchEvent.CurrentPos, out isPrev, null);
		if (gameObject != null)
		{
			GameCursorUtil.ChangeGameCursor(gameObject, isHovered: true);
		}
		else
		{
			SetType(GameCursorType.Normal);
		}
	}
}
