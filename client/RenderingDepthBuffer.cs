using UnityEngine;

[ExecuteInEditMode]
public class RenderingDepthBuffer : MonoBehaviour
{
	public void Start()
	{
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
	}
}
