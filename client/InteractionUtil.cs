using System;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

public static class InteractionUtil
{
	private static RenderTexture _renderTarget;

	private static Texture2D _pixelPicker;

	private static RenderTexture RenderTarget => (!(_renderTarget == null)) ? _renderTarget : (_renderTarget = new RenderTexture(1, 1, 0));

	private static Texture2D PixelPicker => (!(_pixelPicker == null)) ? _pixelPicker : (_pixelPicker = new Texture2D(1, 1));

	public static GameObject PickingObject(GameObject selectedObject, Ray ray, Vector2 currentPos, out bool isPrev, Func<GameObject, bool> filterFunc)
	{
		GameObject result = null;
		float num = float.MaxValue;
		int? num2 = null;
		if (PlayerBehavior.LocalPlayer != null)
		{
			num2 = PlayerBehavior.LocalPlayer.Floor.Value;
		}
		int count;
		RaycastHit[] array = Collisions.RayCast(ray, float.PositiveInfinity, LayerHelper.InteractionMask, out count);
		isPrev = false;
		float num3 = float.MaxValue;
		Vector2 size = default(Vector2);
		for (int i = 0; i < count; i++)
		{
			RaycastHit raycastHit = array[i];
			Transform transform = ((!(raycastHit.collider == null)) ? raycastHit.collider.transform : raycastHit.transform);
			GameObject interactionObject = GetInteractionObject(transform.gameObject);
			if (interactionObject == null)
			{
				continue;
			}
			if (num2.HasValue)
			{
				ImmovableBase component = interactionObject.GetComponent<ImmovableBase>();
				if (component != null && component.Floor.GetValueOrDefault() != num2.Value)
				{
					continue;
				}
				CharacterBehavior component2 = interactionObject.GetComponent<CharacterBehavior>();
				if (component2 != null && component2.Floor.Value != num2.Value)
				{
					continue;
				}
			}
			tk2dSprite component3 = interactionObject.GetComponent<tk2dSprite>();
			if (component3 != null)
			{
				tk2dSpriteCollectionData collection = component3.Collection;
				tk2dSpriteDefinition currentSpriteDef = component3.GetCurrentSpriteDef();
				BoxCollider boxCollider = raycastHit.collider as BoxCollider;
				if (boxCollider == null)
				{
					continue;
				}
				Vector3 vector = interactionObject.transform.InverseTransformPoint(raycastHit.point);
				if (vector.z != 0f && ray.direction.z != 0f)
				{
					vector -= ray.direction * (vector.z / ray.direction.z);
				}
				Vector3 vector2 = boxCollider.center - boxCollider.size * 0.5f;
				vector -= vector2;
				vector.x /= boxCollider.size.x;
				vector.y /= boxCollider.size.y;
				Vector2 vector3 = (currentSpriteDef.uvs[1] - currentSpriteDef.uvs[0]) * vector.x;
				Vector2 vector4 = (currentSpriteDef.uvs[2] - currentSpriteDef.uvs[0]) * vector.y;
				vector = currentSpriteDef.uvs[0] + vector3 + vector4;
				Material material = collection.materials[currentSpriteDef.materialId];
				bool flag = material.IsKeywordEnabled("CENTER_TRANSPARENT");
				if (flag)
				{
					material.DisableKeyword("CENTER_TRANSPARENT");
				}
				size.x = 1f / (float)material.mainTexture.width;
				size.y = 1f / (float)material.mainTexture.height;
				RenderTexture renderTarget = RenderTarget;
				RenderTexture active = RenderTexture.active;
				RenderTexture.active = renderTarget;
				renderTarget.MarkRestoreExpected();
				GL.Clear(clearDepth: true, clearColor: true, Color.clear);
				material.SetPass(0);
				if (flag)
				{
					material.EnableKeyword("CENTER_TRANSPARENT");
				}
				DrawQuads(new Rect(vector, size), new Rect(0f, 0f, 1f, 1f));
				Texture2D pixelPicker = PixelPicker;
				pixelPicker.ReadPixels(new Rect(0f, 0f, 1f, 1f), 0, 0);
				pixelPicker.Apply();
				RenderTexture.active = active;
				if (((Color32)pixelPicker.GetPixel(0, 0)).a == 0)
				{
					continue;
				}
				if (interactionObject == selectedObject)
				{
					isPrev = true;
					continue;
				}
				if (!(component3.color.a < 1f))
				{
					if (!(raycastHit.distance < num3))
					{
						continue;
					}
					num3 = raycastHit.distance;
				}
			}
			else if (interactionObject == selectedObject)
			{
				isPrev = true;
				continue;
			}
			if (filterFunc != null && !filterFunc(interactionObject))
			{
				continue;
			}
			float distance = InteractionObject.GetDistance(interactionObject);
			if (!(distance > 2000f))
			{
				Vector3 interactionPosition = InteractionObject.GetInteractionPosition(interactionObject, ignoreY: false);
				Vector3 vector5 = MainCamera.WorldToScreenPos(interactionPosition);
				vector5.z = 0f;
				Vector3 vector6 = currentPos;
				float sqrMagnitude = (vector5 - vector6).sqrMagnitude;
				if (!(num < distance))
				{
					result = interactionObject;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	private static GameObject GetInteractionObject(GameObject gameObject)
	{
		int layer = gameObject.layer;
		GameObject result = null;
		if (layer == LayerHelper.DefaultLayer)
		{
			result = InteractionSystem.MovableInteractionObjectFilter(gameObject);
		}
		else if (layer == LayerHelper.PropLayer)
		{
			result = InteractionSystem.PropInteractionObjectFilter(gameObject);
		}
		return result;
	}

	private static void DrawQuads(Rect uv, Rect vert)
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Begin(7);
		GL.TexCoord(new Vector3(uv.x, uv.y, 0f));
		GL.Vertex(new Vector3(vert.x, vert.y, 0f));
		GL.TexCoord(new Vector3(uv.xMax, uv.y, 0f));
		GL.Vertex(new Vector3(vert.xMax, vert.y, 0f));
		GL.TexCoord(new Vector3(uv.xMax, uv.yMax, 0f));
		GL.Vertex(new Vector3(vert.xMax, vert.yMax, 0f));
		GL.TexCoord(new Vector3(uv.x, uv.yMax, 0f));
		GL.Vertex(new Vector3(vert.x, vert.yMax, 0f));
		GL.End();
		GL.PopMatrix();
	}
}
