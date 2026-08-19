using Durango.Render.Camera;
using UnityEngine;

[RequireComponent(typeof(ApngTexture))]
public class LenticularViewer : MonoBehaviour
{
	private ApngTexture _apngTexture;

	private void OnEnable()
	{
		ApngTexture component = GetComponent<ApngTexture>();
		component.enabled = false;
		_apngTexture = component;
	}

	private void Update()
	{
		if (!(_apngTexture == null) && _apngTexture.FrameLength > 1)
		{
			float num = MainCamera.WorldToNGUIPos(base.transform.position).x / 320f;
			num += (AccelerationChecker.Acceleration.x + 1f) / 2f * 3f;
			num = Mathf.Repeat(num, 1f);
			_apngTexture.SetFrame((float)_apngTexture.FrameLength * num);
		}
	}

	public static void Enable(ApngTexture tex, bool enable)
	{
		if (enable)
		{
			LenticularViewer lenticularViewer = tex.gameObject.AddMissingComponent<LenticularViewer>();
			lenticularViewer.enabled = true;
			return;
		}
		LenticularViewer component = tex.GetComponent<LenticularViewer>();
		if (component != null)
		{
			component.enabled = false;
		}
	}
}
