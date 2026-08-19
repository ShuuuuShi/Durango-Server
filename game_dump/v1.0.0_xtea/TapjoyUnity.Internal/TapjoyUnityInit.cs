using UnityEngine;

namespace TapjoyUnity.Internal;

public sealed class TapjoyUnityInit : MonoBehaviour
{
	private void Awake()
	{
		ApiBindingAndroid.Install();
	}
}
