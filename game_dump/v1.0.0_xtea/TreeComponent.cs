using System;
using System.Collections;
using UnityEngine;

public class TreeComponent : NaturalComponent
{
	private const float ParticleEmitHeight = 300f;

	public float SpriteHeight = 10f;

	private KSprite _stumpSprite;

	public TreeComponent(NaturalObject natural)
		: base(natural)
	{
		SoundManager.Cache("Sound/Effect/Prop/Prop_tree_felling_01.wav");
		SoundManager.Cache("Sound/Effect/Prop/Prop_tree_fallground_01.wav");
	}

	public void OnLoot()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (base.GameObject.activeSelf)
		{
			ParticleManager.Emit("Particle/Tree_Crash_01.prefab", base.Position + new Vector3(0f, 300f, 0f), Quaternion.identity, null, useLocalPosition: true, comeForwardToCamera: true);
			SoundManager.Play("Sound/Effect/Prop/Prop_tree_felling_01.wav", base.Position);
			AddStump();
			((MonoBehaviour)base.Natural).StartCoroutine(CoLoot());
		}
	}

	private IEnumerator CoLoot()
	{
		float startFellingTime = Time.realtimeSinceStartup;
		GameObject tree = base.KSprite.GameObject;
		FellingTreeController treeController = KSingleton<FellingTreeController>.Instance();
		float fadingOutTime = treeController.fadingOutTime;
		float fallenAngle = treeController.fallenAngle;
		float bouncing1FallingAngle = treeController.bouncing1Angle;
		float bouncing2FallingAngle = treeController.bouncing2Angle;
		float curveFactor = treeController.curveFactor;
		float bouncing1Time = treeController.bouncing1Time;
		float bouncing2Time = treeController.bouncing2Time;
		float bouncing3Time = treeController.bouncing3Time;
		while (true)
		{
			float num;
			float elapsedTime4 = (num = Time.realtimeSinceStartup - startFellingTime);
			if (!(num < bouncing1Time))
			{
				break;
			}
			float alpha4 = 1f - elapsedTime4 / fadingOutTime;
			base.KSprite.SetAlpha(alpha4);
			float rotFactor4 = elapsedTime4 / bouncing1Time;
			rotFactor4 = Mathf.Pow(rotFactor4, curveFactor);
			if (rotFactor4 > 1f)
			{
				rotFactor4 = 1f;
			}
			tree.transform.localRotation = Quaternion.Euler(0f, 45f, (0f - (fallenAngle + bouncing1FallingAngle)) * rotFactor4);
			yield return null;
		}
		SoundManager.Play("Sound/Effect/Prop/Prop_tree_fallground_01.wav", base.Position);
		float x_z = Mathf.Sin((float)Math.PI / 4f) * 300f;
		Vector3 particlePos2 = new Vector3(x_z * Mathf.Cos((float)Math.PI / 4f), Mathf.Cos((float)Math.PI / 4f) * 300f, (0f - x_z) * Mathf.Cos((float)Math.PI / 4f));
		particlePos2 += ((Component)KSingleton<MainCamera>.Instance()).transform.forward * 500f;
		ParticleManager.Emit(rotation: Quaternion.Euler(270f, 180f, 0f), assetPath: "Particle/FX_Prop_Tree_Fallground_01.prefab", pos: base.Position + particlePos2, followingParent: null, useLocalPosition: true, comeForwardToCamera: true);
		while (true)
		{
			float num;
			float elapsedTime4 = (num = Time.realtimeSinceStartup - startFellingTime);
			if (num < bouncing2Time)
			{
				float alpha3 = 1f - elapsedTime4 / fadingOutTime;
				base.KSprite.SetAlpha(alpha3);
				float rotFactor2 = (elapsedTime4 - bouncing1Time) / (bouncing2Time - bouncing1Time);
				tree.transform.localRotation = Quaternion.Euler(0f, 45f, 0f - (fallenAngle + bouncing1FallingAngle * (1f - rotFactor2) + bouncing2FallingAngle * rotFactor2));
				yield return null;
				continue;
			}
			break;
		}
		while (true)
		{
			float num;
			float elapsedTime4 = (num = Time.realtimeSinceStartup - startFellingTime);
			if (!(num < bouncing3Time))
			{
				break;
			}
			float alpha2 = 1f - elapsedTime4 / fadingOutTime;
			base.KSprite.SetAlpha(alpha2);
			float rotFactor = (elapsedTime4 - bouncing2Time) / (bouncing3Time - bouncing2Time);
			tree.transform.localRotation = Quaternion.Euler(0f, 45f, 0f - (fallenAngle + bouncing2FallingAngle * (1f - rotFactor)));
			yield return null;
		}
		tree.transform.localRotation = Quaternion.Euler(0f, 45f, 0f - fallenAngle);
		while (true)
		{
			float num;
			float elapsedTime4 = (num = Time.realtimeSinceStartup - startFellingTime);
			if (!(num < fadingOutTime))
			{
				break;
			}
			float alpha = 1f - elapsedTime4 / fadingOutTime;
			base.KSprite.SetAlpha(alpha);
			yield return null;
		}
		base.KSprite.SetAlpha(0f);
		RemoveStump();
	}

	private void AddStump()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(base.KSprite.StumpName) && _stumpSprite == null)
		{
			_stumpSprite = KSingleton<SpriteManager>.Instance().CreateSprite(SpriteObjectType.Shrub, base.KSprite.StumpName);
			((Object)_stumpSprite.GameObject).name = "Stump";
			_stumpSprite.GameObject.transform.position = base.KSprite.GameObject.transform.position + new Vector3(0f, 0f, 0.1f);
			_stumpSprite.GameObject.transform.rotation = base.KSprite.GameObject.transform.rotation;
			_stumpSprite.GameObject.transform.localScale = Vector3.one;
		}
	}

	private void RemoveStump()
	{
		((MonoBehaviour)base.Natural).StartCoroutine(CoStumpFadeOut());
	}

	private IEnumerator CoStumpFadeOut()
	{
		if (_stumpSprite == null)
		{
			base.GameObject.SetActive(false);
			yield break;
		}
		float remainTime = 2f;
		while (remainTime >= 0f)
		{
			remainTime -= Time.deltaTime;
			float alpha = Mathf.Clamp01(remainTime / 2f);
			_stumpSprite.SetAlpha(alpha);
			yield return null;
		}
		Object.Destroy((Object)(object)_stumpSprite.GameObject);
		_stumpSprite = null;
		base.GameObject.SetActive(false);
	}

	public void BeginShake(bool emitParticle)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (emitParticle)
		{
			ParticleManager.Emit("Particle/LeafParticle.prefab", base.Position + new Vector3(0f, SpriteHeight, 0f), Quaternion.identity, null, useLocalPosition: true, comeForwardToCamera: true);
		}
	}
}
