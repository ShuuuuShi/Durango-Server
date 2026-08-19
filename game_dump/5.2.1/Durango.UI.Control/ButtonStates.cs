using System;
using UnityEngine;

namespace Durango.UI.Control;

[Serializable]
public class ButtonStates
{
	[SerializeField]
	private ButtonState[] _list;

	public int Count => KUtility.GetSize(_list);

	public ButtonState this[int index] => _list[index];

	public void AddState(ButtonState state)
	{
		if (state != null)
		{
			ButtonState[] array = new ButtonState[Count + 1];
			if (Count > 0)
			{
				_list.CopyTo(array, 0);
			}
			array[array.Length - 1] = state;
			_list = array;
		}
	}
}
