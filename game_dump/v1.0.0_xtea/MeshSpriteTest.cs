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
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
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
