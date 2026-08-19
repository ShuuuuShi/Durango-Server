using UnityEngine;

namespace Durango.UI;

public static class GameCursorUtil
{
	private static GameCursorManager _cursorManager;

	private static readonly InteractionObject InteractionObjectCache = new InteractionObject();

	public static void SetGameCursorManager(GameCursorManager manager)
	{
		_cursorManager = manager;
	}

	private static GameCursorType ConvertToCursorType(InteractionObject interactionObject)
	{
		switch (interactionObject.ObjectType)
		{
		case InteractionObject.Type.Animal:
			if (ObjectIdentifier.IsTargetableEnemy(InteractionObjectCache.Target, includePets: false))
			{
				return GameCursorType.Battle;
			}
			if (ObjectIdentifier.IsDeadBody(InteractionObjectCache.Target))
			{
				return GameCursorType.Gathering;
			}
			break;
		case InteractionObject.Type.Prop:
			return GameCursorType.Gathering;
		}
		return GameCursorType.Normal;
	}

	public static void ChangeGameCursor(GameObject target, bool isHovered)
	{
		if (_cursorManager != null)
		{
			if (isHovered)
			{
				InteractionObjectCache.Target = target;
				GameCursorType type = ConvertToCursorType(InteractionObjectCache);
				_cursorManager.SetType(type);
			}
			else
			{
				_cursorManager.SetType(GameCursorType.Normal);
			}
		}
	}

	public static void SetGameCursorDisabled(bool isDisabled)
	{
		if (_cursorManager != null)
		{
			GameCursorState state = (isDisabled ? GameCursorState.Disabled : GameCursorState.Normal);
			_cursorManager.SetState(state);
		}
	}

	public static void SetGameCursorLocked(bool isLocked)
	{
		if (_cursorManager != null)
		{
			_cursorManager.SetLocked(isLocked);
		}
	}
}
