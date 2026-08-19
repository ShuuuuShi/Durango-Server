namespace Durango.Player;

public class PlayerEquipment
{
	private enum State
	{
		None,
		Reserved,
		On
	}

	private State _state;

	private string _motionPath;

	private ItemColor _motionColor;

	private string _currentPath;

	private ItemColor _currentColor;

	public string GetCurrentPath()
	{
		return (!IsMotionEquipped()) ? _currentPath : _motionPath;
	}

	public ItemColor GetCurrentColor()
	{
		return (!IsMotionEquipped()) ? _currentColor : _motionColor;
	}

	public bool IsMotionEquipped()
	{
		return _state == State.On;
	}

	public void SetMotionEquipImmediately(string path, ItemColor color = default(ItemColor))
	{
		_motionPath = path;
		_motionColor = color;
		_state = State.On;
	}

	public void ReserveMotionEquipment(string path, ItemColor color = default(ItemColor))
	{
		ItemColor itemColor = color.ToThreeColor();
		if (!(_motionPath == path) || !(_motionColor == itemColor))
		{
			_motionPath = path;
			_motionColor = itemColor;
			_state = State.Reserved;
		}
	}

	public void ResetMotionEquipment()
	{
		_motionPath = null;
		_state = State.None;
	}

	public void ChangePath(string path)
	{
		_currentPath = path;
	}

	public void ChangeColor(ItemColor color)
	{
		_currentColor = color.ToThreeColor();
	}

	public void AnimMotionChanged()
	{
		if (_state == State.On)
		{
			ResetMotionEquipment();
		}
		if (_state == State.Reserved)
		{
			_state = State.On;
		}
	}
}
