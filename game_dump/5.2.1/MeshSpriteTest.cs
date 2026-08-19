using UnityEngine;

public class MeshSpriteTest : MonoBehaviour
{
	public GameObject sprite;

	public GameObject mesh;

	private bool toggle;

	private void Awake()
	{
		sprite = GameObject.Find("Sprite");
		mesh = GameObject.Find("Mesh");
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 50f, 50f), "Sprite"))
		{
			sprite.SetActive(!sprite.activeSelf);
		}
		if (GUI.Button(new Rect(10f, 60f, 50f, 50f), "Mesh"))
		{
			mesh.SetActive(!mesh.activeSelf);
		}
		if (GUI.Button(new Rect(10f, 110f, 50f, 50f), "Toggle"))
		{
			toggle = !toggle;
			sprite.SetActive(toggle);
			mesh.SetActive(!toggle);
		}
	}
}
