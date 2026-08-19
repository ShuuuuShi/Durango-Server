using UnityEngine;

public abstract class UISystem<T, TU> : GameSystem<T> where T : MonoBehaviour where TU : UIBase
{
	private TU _uiGroup;

	protected TU UIGroup
	{
		get
		{
			if ((Object)(object)_uiGroup == (Object)null)
			{
				_uiGroup = UIManager.FindScript<TU>();
			}
			return _uiGroup;
		}
	}

	public virtual bool IsOpened()
	{
		int result;
		if ((Object)(object)UIGroup != (Object)null)
		{
			TU uIGroup = UIGroup;
			result = (uIGroup.IsOpen ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}

	public virtual void Open()
	{
		if ((Object)(object)UIGroup != (Object)null)
		{
			TU uIGroup = UIGroup;
			uIGroup.Open();
		}
	}

	public virtual void Close()
	{
		if ((Object)(object)UIGroup != (Object)null)
		{
			TU uIGroup = UIGroup;
			uIGroup.Close();
		}
	}
}
