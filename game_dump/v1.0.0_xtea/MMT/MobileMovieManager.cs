using System.Collections;
using UnityEngine;

namespace MMT;

public class MobileMovieManager : MonoBehaviour
{
	public static MobileMovieManager Instance;

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private void OnEnable()
	{
		((MonoBehaviour)this).StartCoroutine("DecodeCoroutine");
	}

	private void OnDisable()
	{
		((MonoBehaviour)this).StopCoroutine("DecodeCoroutine");
	}

	private IEnumerator DecodeCoroutine()
	{
		while (true)
		{
			yield return (object)new WaitForEndOfFrame();
			GL.IssuePluginEvent(7);
		}
	}
}
