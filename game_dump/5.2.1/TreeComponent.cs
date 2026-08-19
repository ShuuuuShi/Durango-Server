using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.Render.Sprite;
using Durango.Utils;
using UnityEngine;

public class TreeComponent : NaturalComponent
{
	[CompilerGenerated]
	private sealed class _003CCoLoot_003Ed__15 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TreeComponent _003C_003E4__this;

		private float _003CstartFellingTime_003E5__2;

		private GameObject _003Ctree_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoLoot_003Ed__15(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Ctree_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TreeComponent treeComponent = _003C_003E4__this;
			float num2;
			float num3;
			Vector3 vector;
			float num4;
			float num6;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CstartFellingTime_003E5__2 = Time.realtimeSinceStartup;
				_003Ctree_003E5__3 = treeComponent.Sprite.GameObject;
				goto IL_004d;
			case 1:
				_003C_003E1__state = -1;
				goto IL_004d;
			case 2:
				_003C_003E1__state = -1;
				goto IL_01a8;
			case 3:
				_003C_003E1__state = -1;
				goto IL_024c;
			case 4:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_004d:
				if ((num2 = Time.realtimeSinceStartup - _003CstartFellingTime_003E5__2) < 4f)
				{
					float alpha = 1f - num2 / 6f;
					treeComponent.Sprite.SetAlpha(alpha);
					float f = num2 / 4f;
					f = Mathf.Pow(f, 6f);
					if (f > 1f)
					{
						f = 1f;
					}
					_003Ctree_003E5__3.transform.localRotation = Quaternion.Euler(0f, 45f, -45.5f * f);
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				SoundManager.PlayEvent("Prop_tree_fallground_01", SoundPosition.Fix(treeComponent.Position));
				num3 = Mathf.Sin((float)Math.PI / 4f) * 300f;
				vector = new Vector3(num3 * Mathf.Cos((float)Math.PI / 4f), Mathf.Cos((float)Math.PI / 4f) * 300f, (0f - num3) * Mathf.Cos((float)Math.PI / 4f));
				vector += Singleton<MainCamera>.Instance().transform.forward * 500f;
				ParticleManager.Emit("Particle/FX_Prop_Tree_Fallground_01.prefab", rotation: Quaternion.Euler(270f, 180f, 0f), pos: treeComponent.Position + vector, comeForwardToCamera: true);
				goto IL_01a8;
				IL_01a8:
				if ((num4 = Time.realtimeSinceStartup - _003CstartFellingTime_003E5__2) < 4.2f)
				{
					float alpha2 = 1f - num4 / 6f;
					treeComponent.Sprite.SetAlpha(alpha2);
					float num5 = (num4 - 4f) / 0.19999981f;
					_003Ctree_003E5__3.transform.localRotation = Quaternion.Euler(0f, 45f, 0f - (45f + 0.5f * (1f - num5) + -0.2f * num5));
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				goto IL_024c;
				IL_024c:
				if ((num6 = Time.realtimeSinceStartup - _003CstartFellingTime_003E5__2) < 4.3f)
				{
					float alpha3 = 1f - num6 / 6f;
					treeComponent.Sprite.SetAlpha(alpha3);
					float num7 = (num6 - 4.2f) / 0.10000038f;
					_003Ctree_003E5__3.transform.localRotation = Quaternion.Euler(0f, 45f, 0f - (45f + -0.2f * (1f - num7)));
					_003C_003E2__current = null;
					_003C_003E1__state = 3;
					return true;
				}
				_003Ctree_003E5__3.transform.localRotation = Quaternion.Euler(0f, 45f, -45f);
				break;
			}
			float num8;
			if ((num8 = Time.realtimeSinceStartup - _003CstartFellingTime_003E5__2) < 6f)
			{
				float alpha4 = 1f - num8 / 6f;
				treeComponent.Sprite.SetAlpha(alpha4);
				_003C_003E2__current = null;
				_003C_003E1__state = 4;
				return true;
			}
			treeComponent.Sprite.SetAlpha(0f);
			treeComponent.RemoveStump();
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCoStumpFadeOut_003Ed__18 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TreeComponent _003C_003E4__this;

		private float _003CremainTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoStumpFadeOut_003Ed__18(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TreeComponent treeComponent = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (treeComponent._stumpSprite == null)
				{
					treeComponent.GameObject.SetActive(value: false);
					return false;
				}
				_003CremainTime_003E5__2 = 2f;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003CremainTime_003E5__2 >= 0f)
			{
				_003CremainTime_003E5__2 -= Time.deltaTime;
				float alpha = Mathf.Clamp01(_003CremainTime_003E5__2 / 2f);
				treeComponent._stumpSprite.SetAlpha(alpha);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			UnityEngine.Object.Destroy(treeComponent._stumpSprite.GameObject);
			treeComponent._stumpSprite = null;
			treeComponent.GameObject.SetActive(value: false);
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private const float FallenAngle = 45f;

	private const float Bouncing1Angle = 0.5f;

	private const float Bouncing2Angle = -0.2f;

	private const float CurveFactor = 6f;

	private const float Bouncing1Time = 4f;

	private const float Bouncing2Time = 4.2f;

	private const float Bouncing3Time = 4.3f;

	private const float FadingOutTime = 6f;

	private const string TreeFellingSound = "Prop_tree_felling_01";

	private const string TreeBouncingSound = "Prop_tree_fallground_01";

	private const float ParticleEmitHeight = 300f;

	public float SpriteHeight = 10f;

	private Durango.Render.Sprite.Sprite _stumpSprite;

	public TreeComponent(NaturalSpriteObject natural)
		: base(natural)
	{
		SoundManager.PrepareEvent("Prop_tree_felling_01");
		SoundManager.PrepareEvent("Prop_tree_fallground_01");
	}

	public void OnLoot()
	{
		if (base.GameObject.activeSelf)
		{
			ParticleManager.Emit("Particle/Tree_Crash_01.prefab", base.Position + new Vector3(0f, 300f, 0f), Quaternion.identity, comeForwardToCamera: true);
			SoundManager.PlayEvent("Prop_tree_felling_01", SoundPosition.Fix(base.Position));
			AddStump();
			base.Natural.StartCoroutine(CoLoot());
		}
	}

	private IEnumerator CoLoot()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoLoot_003Ed__15(0)
		{
			_003C_003E4__this = this
		};
	}

	private void AddStump()
	{
		if (!string.IsNullOrEmpty(base.Sprite.StumpName) && _stumpSprite == null)
		{
			_stumpSprite = Singleton<SpriteManager>.Instance().CreateSprite(SpriteObjectType.Shrub, base.Sprite.StumpName);
			_stumpSprite.GameObject.name = "Stump";
			_stumpSprite.GameObject.transform.position = base.Sprite.GameObject.transform.position + new Vector3(0f, 0f, 0.1f);
			_stumpSprite.GameObject.transform.rotation = base.Sprite.GameObject.transform.rotation;
			_stumpSprite.GameObject.transform.localScale = Vector3.one;
		}
	}

	private void RemoveStump()
	{
		base.Natural.StartCoroutine(CoStumpFadeOut());
	}

	private IEnumerator CoStumpFadeOut()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoStumpFadeOut_003Ed__18(0)
		{
			_003C_003E4__this = this
		};
	}

	public void BeginShake(bool emitParticle)
	{
		if (emitParticle)
		{
			ParticleManager.Emit("Particle/LeafParticle.prefab", base.Position + new Vector3(0f, SpriteHeight, 0f), Quaternion.identity, comeForwardToCamera: true);
		}
	}
}
