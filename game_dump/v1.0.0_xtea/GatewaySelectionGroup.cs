using System;
using UnityEngine;

public class GatewaySelectionGroup : MonoBehaviour
{
	[Serializable]
	private struct PresetHost
	{
		public string Name;

		public string URL;
	}

	[SerializeField]
	private UIInput _hostField;

	[SerializeField]
	private Selectable _okButton;

	[SerializeField]
	private ListObjectPool _buttons;

	[SerializeField]
	private PresetHost[] _presets;

	private void Awake()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		_buttons.Set(_presets.Length);
		int width = _buttons.BaseObject.GetComponent<UIWidget>().width;
		float y = _buttons.BaseObject.transform.localPosition.y;
		int i = 0;
		for (int count = _buttons.Count; i < count; i++)
		{
			DefaultSelectableButton component = _buttons[i].GetComponent<DefaultSelectableButton>();
			component.Text = _presets[i].Name;
			component.Clicked = OnClickPresetButton;
			((Component)component).transform.localPosition = Vector3.right * (float)width * ((float)i - (float)(count - 1) * 0.5f) + Vector3.up * y;
		}
		_okButton.Clicked = OkButton_Clicked;
	}

	private void OnClickPresetButton()
	{
		GameObject gameObject = ((Component)Selectable.Current).gameObject;
		int num = _buttons.IndexOf(gameObject);
		_hostField.value = _presets[num].URL;
	}

	private void OkButton_Clicked()
	{
		string value = _hostField.value;
		value = MakeFullUrl(value);
		KSingleton<GameManager>.Instance().GatewayUrl = value;
		Clear();
	}

	private static string MakeFullUrl(string host)
	{
		if (!host.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
		{
			host = "http://" + host;
		}
		if (!host.EndsWith("/"))
		{
			host += "/";
		}
		return host;
	}

	private void Clear()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
