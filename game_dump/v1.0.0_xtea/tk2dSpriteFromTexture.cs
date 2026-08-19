using UnityEngine;
using tk2dRuntime;

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteFromTexture")]
public class tk2dSpriteFromTexture : MonoBehaviour
{
	public Texture texture;

	public tk2dSpriteCollectionSize spriteCollectionSize = new tk2dSpriteCollectionSize();

	public tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.MiddleCenter;

	private tk2dSpriteCollectionData spriteCollection;

	private tk2dBaseSprite _sprite;

	private tk2dBaseSprite Sprite
	{
		get
		{
			if ((Object)(object)_sprite == (Object)null)
			{
				_sprite = ((Component)this).GetComponent<tk2dBaseSprite>();
				if ((Object)(object)_sprite == (Object)null)
				{
					_sprite = ((Component)this).gameObject.AddComponent<tk2dSprite>();
				}
			}
			return _sprite;
		}
	}

	public bool HasSpriteCollection => (Object)(object)spriteCollection != (Object)null;

	private void Awake()
	{
		Create(spriteCollectionSize, texture, anchor);
	}

	private void OnDestroy()
	{
		DestroyInternal();
		Renderer component = ((Component)this).GetComponent<Renderer>();
		if ((Object)(object)component != (Object)null)
		{
			component.material = null;
		}
	}

	public void Create(tk2dSpriteCollectionSize spriteCollectionSize, Texture texture, tk2dBaseSprite.Anchor anchor)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		DestroyInternal();
		if ((Object)(object)texture != (Object)null)
		{
			this.spriteCollectionSize.CopyFrom(spriteCollectionSize);
			this.texture = texture;
			this.anchor = anchor;
			GameObject val = new GameObject("tk2dSpriteFromTexture - " + ((Object)texture).name);
			val.transform.localPosition = Vector3.zero;
			val.transform.localRotation = Quaternion.identity;
			val.transform.localScale = Vector3.one;
			((Object)val).hideFlags = (HideFlags)52;
			Vector2 anchorOffset = tk2dSpriteGeomGen.GetAnchorOffset(anchor, texture.width, texture.height);
			spriteCollection = SpriteCollectionGenerator.CreateFromTexture(val, texture, spriteCollectionSize, new Vector2((float)texture.width, (float)texture.height), new string[1] { "unnamed" }, (Rect[])(object)new Rect[1]
			{
				new Rect(0f, 0f, (float)texture.width, (float)texture.height)
			}, null, (Vector2[])(object)new Vector2[1] { anchorOffset }, new bool[1]);
			string text = "SpriteFromTexture " + ((Object)texture).name;
			spriteCollection.spriteCollectionName = text;
			((Object)spriteCollection.spriteDefinitions[0].material).name = text;
			((Object)spriteCollection.spriteDefinitions[0].material).hideFlags = (HideFlags)54;
			Sprite.SetSprite(spriteCollection, 0);
		}
	}

	public void Clear()
	{
		DestroyInternal();
	}

	public void ForceBuild()
	{
		DestroyInternal();
		Create(spriteCollectionSize, texture, anchor);
	}

	private void DestroyInternal()
	{
		if ((Object)(object)spriteCollection != (Object)null)
		{
			if ((Object)(object)spriteCollection.spriteDefinitions[0].material != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)spriteCollection.spriteDefinitions[0].material);
			}
			Object.DestroyImmediate((Object)(object)((Component)spriteCollection).gameObject);
			spriteCollection = null;
		}
	}
}
