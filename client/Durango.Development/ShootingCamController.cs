using Durango.Render.Sprite;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.Development;

public class ShootingCamController : MonoBehaviour
{
	public static Vector3 ControlledCamAngle { get; set; }

	private void Start()
	{
		GameSystem<InputSystem>.Instance().On(InputCommand.CameraRotation, InputRotated);
		GameSystem<InputSystem>.Instance().On(InputCommand.CameraReset, InputCameraReset);
	}

	private void InputRotated(InputCommandMessage message)
	{
		Vector3 vector = message.MoveDirection.normalized * 2f;
		ControlledCamAngle += new Vector3(0f - vector.y, vector.x, 0f);
		RotateSpriteNaturalsForcely();
	}

	private void InputCameraReset(InputCommandMessage message)
	{
		ControlledCamAngle = Vector3.zero;
		RotateSpriteNaturalsForcely();
	}

	private void RotateSpriteNaturalsForcely()
	{
		MeshRenderer[] componentsInChildren = Singleton<TerrainBase>.Instance().gameObject.GetComponentsInChildren<MeshRenderer>();
		MeshRenderer[] array = componentsInChildren;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer == null || meshRenderer.sharedMaterial == null || !meshRenderer.gameObject.activeSelf)
			{
				continue;
			}
			if (meshRenderer.sharedMaterial.shader.name.Contains("Floor4Grass"))
			{
				meshRenderer.transform.eulerAngles = new Vector3(ControlledCamAngle.x, 45f + ControlledCamAngle.y, 0f);
				continue;
			}
			NaturalSpriteObject component = meshRenderer.GetComponent<NaturalSpriteObject>();
			if (!(component == null) && component.Sprite != null && component.Sprite.SpriteObjectType != SpriteObjectType.Puddle && (meshRenderer.sharedMaterial.shader.name == "Durango/Sprite/Transparent" || meshRenderer.sharedMaterial.shader.name == "Durango/Sprite/WithShadow"))
			{
				meshRenderer.transform.eulerAngles = component.Sprite.InitialRotation.eulerAngles + new Vector3(ControlledCamAngle.x, ControlledCamAngle.y, 0f);
				int childCount = meshRenderer.transform.childCount;
				for (int j = 0; j < childCount; j++)
				{
					Transform child = meshRenderer.transform.GetChild(j);
					child.rotation = component.Sprite.InitialRotation;
				}
			}
		}
	}
}
