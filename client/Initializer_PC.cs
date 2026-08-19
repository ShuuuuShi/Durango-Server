using UnityEngine;
using UnityEngine.SceneManagement;

public class Initializer_PC : MonoBehaviour
{
	private void Awake()
	{
		SceneManager.LoadScene("Title");
	}
}
