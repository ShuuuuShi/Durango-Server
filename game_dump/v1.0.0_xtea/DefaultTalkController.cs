using MoPhoGames.USpeak.Interface;
using UnityEngine;

[AddComponentMenu("USpeak/Default Talk Controller")]
public class DefaultTalkController : MonoBehaviour, IUSpeakTalkController
{
	[SerializeField]
	[HideInInspector]
	public KeyCode TriggerKey;

	[HideInInspector]
	[SerializeField]
	public int ToggleMode;

	private bool val;

	public void OnInspectorGUI()
	{
	}

	public bool ShouldSend()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (ToggleMode == 0)
		{
			val = Input.GetKey(TriggerKey);
		}
		else if (Input.GetKeyDown(TriggerKey))
		{
			val = !val;
		}
		return val;
	}
}
