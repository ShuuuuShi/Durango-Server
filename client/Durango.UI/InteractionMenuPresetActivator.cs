using UnityEngine;

namespace Durango.UI;

internal class InteractionMenuPresetActivator : MonoBehaviour
{
	[SerializeField]
	private InteractionMenuPreset[] _menuPresets;

	public void Activate(int index)
	{
		if (_menuPresets != null)
		{
			InteractionMenuPreset[] menuPresets = _menuPresets;
			foreach (InteractionMenuPreset interactionMenuPreset in menuPresets)
			{
				interactionMenuPreset.SetPreset(index);
			}
		}
	}

	[ExposedInEditor("오른쪽 위 (0)")]
	private void Activate_0()
	{
		Activate(0);
	}

	[ExposedInEditor("오른쪽 (1)")]
	private void Activate_1()
	{
		Activate(1);
	}

	[ExposedInEditor("오른쪽 아래 (2)")]
	private void Activate_2()
	{
		Activate(2);
	}

	[ExposedInEditor("왼쪽 아래 (3)")]
	private void Activate_3()
	{
		Activate(3);
	}

	[ExposedInEditor("왼쪽 (4)")]
	private void Activate_4()
	{
		Activate(4);
	}

	[ExposedInEditor("왼쪽 위 (5)")]
	private void Activate_5()
	{
		Activate(5);
	}
}
