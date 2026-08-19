using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
	public float lifetime;

	private void Start()
	{
		((MonoBehaviour)this).Invoke("DestroyMe", lifetime);
	}

	private void DestroyMe()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
