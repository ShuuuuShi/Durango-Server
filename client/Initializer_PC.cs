using UnityEngine;
using UnityEngine.SceneManagement;

public class Initializer_PC : MonoBehaviour
{
	private void Awake()
	{
		if (!DurangoUpdateGate.EnsureUpdaterLaunchAllowed())
		{
			return;
		}

		SceneManager.LoadScene("Title");
	}
}
