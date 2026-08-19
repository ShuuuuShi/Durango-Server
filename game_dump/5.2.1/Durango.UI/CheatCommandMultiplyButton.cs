using System;
using UnityEngine;

namespace Durango.UI;

public class CheatCommandMultiplyButton : MonoBehaviour
{
	public event Action<bool> Pressed;

	private void OnPress(bool press)
	{
		if (this.Pressed != null)
		{
			this.Pressed(press);
		}
	}
}
