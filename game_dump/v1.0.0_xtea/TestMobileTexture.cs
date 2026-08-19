using MMT;
using UnityEngine;

[RequireComponent(typeof(MobileMovieTexture))]
public class TestMobileTexture : MonoBehaviour
{
	private MobileMovieTexture m_movieTexture;

	private void Awake()
	{
		m_movieTexture = ((Component)this).GetComponent<MobileMovieTexture>();
		m_movieTexture.onFinished += OnFinished;
	}

	private void OnFinished(MobileMovieTexture sender)
	{
	}

	private void OnGUI()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginArea(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height));
		float num = (float)m_movieTexture.PlayPosition;
		float num2 = GUILayout.HorizontalSlider(num, 0f, (float)m_movieTexture.Duration, (GUILayoutOption[])(object)new GUILayoutOption[0]);
		if (num2 != num)
		{
			m_movieTexture.PlayPosition = num2;
		}
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
		if (GUILayout.Button((!m_movieTexture.IsPlaying) ? "Play" : "Pause", (GUILayoutOption[])(object)new GUILayoutOption[0]))
		{
			if (m_movieTexture.IsPlaying)
			{
				m_movieTexture.Pause = true;
			}
			else if (!m_movieTexture.Pause)
			{
				m_movieTexture.Play();
			}
			else
			{
				m_movieTexture.Pause = false;
			}
		}
		if (GUILayout.Button("Stop", (GUILayoutOption[])(object)new GUILayoutOption[0]))
		{
			m_movieTexture.Stop();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
