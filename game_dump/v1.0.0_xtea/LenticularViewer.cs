using UnityEngine;

[RequireComponent(typeof(ApngTexture))]
public class LenticularViewer : MonoBehaviour
{
	private ApngTexture _apngTexture;

	private void OnEnable()
	{
		ApngTexture component = ((Component)this).GetComponent<ApngTexture>();
		((Behaviour)component).enabled = false;
		_apngTexture = component;
	}

	private void Update()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_apngTexture == (Object)null) && _apngTexture.FrameLength > 1)
		{
			float num = MainCamera.WorldToNGUIPos(((Component)this).transform.position).x / 320f;
			num += (AccelerationChecker.Acceleration.x + 1f) / 2f * 3f;
			num = Mathf.Repeat(num, 1f);
			_apngTexture.SetFrame((float)_apngTexture.FrameLength * num);
		}
	}

	public static void Enable(ApngTexture tex, bool enable)
	{
		if (enable)
		{
			LenticularViewer lenticularViewer = ((Component)tex).gameObject.AddMissingComponent<LenticularViewer>();
			((Behaviour)lenticularViewer).enabled = true;
			return;
		}
		LenticularViewer component = ((Component)tex).GetComponent<LenticularViewer>();
		if ((Object)(object)component != (Object)null)
		{
			((Behaviour)component).enabled = false;
		}
	}
}
