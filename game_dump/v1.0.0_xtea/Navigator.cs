using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigator : MonoBehaviour
{
	public void NavigateTo(int level)
	{
		SceneManager.LoadScene(level);
	}
}
