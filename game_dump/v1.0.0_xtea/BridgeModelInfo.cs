using UnityEngine;

public class BridgeModelInfo : MonoBehaviour
{
	[SerializeField]
	private float _height;

	[SerializeField]
	private GameObject _fence;

	public float Height => _height;

	public GameObject Fence => _fence;
}
