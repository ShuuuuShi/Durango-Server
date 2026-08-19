using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.Utils;
using Durango.Utils.Extensions;
using Shared.Building;
using Shared.Etc;
using UnityEngine;

public class EnterableArtifact : ArtifactComponent
{
	public enum VisibleState
	{
		Normal,
		PlayerInside,
		Obstruction
	}

	[Flags]
	public enum TriggerFlag
	{
		NotInside = 1
	}

	private struct AlphaTweenStruct
	{
		public ModelComponent.IModel Model;

		public float StartAlpha;

		public float EndAlpha;

		public float StartAt;

		public float EndAt;
	}

	[CompilerGenerated]
	private sealed class _003CCoAlphaTweenArtifact_003Ed__58 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public EnterableArtifact _003C_003E4__this;

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
		public _003CCoAlphaTweenArtifact_003Ed__58(int _003C_003E1__state)
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
			EnterableArtifact enterableArtifact = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				enterableArtifact._isPlayAlphaTweenRoutine = true;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (enterableArtifact._alphaTweenList.Count > 0)
			{
				enterableArtifact.UpdateAlphaTween();
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			enterableArtifact._isPlayAlphaTweenRoutine = false;
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

	private readonly List<EnterableArtifact> _viewObstructions = new List<EnterableArtifact>();

	private readonly List<AlphaTweenStruct> _alphaTweenList = new List<AlphaTweenStruct>();

	private bool _isPlayAlphaTweenRoutine;

	private BoxCollider _builtCollider;

	private bool _isGodModeTarget;

	private TriggerFlag _triggerFlag;

	private bool _isDirtyVisibleState;

	private bool _isDirtyPlayerInside;

	private readonly Observable<bool> _isRooftop = new Observable<bool>();

	protected bool PlayerEntered { get; private set; }

	protected override Artifact.Interaction InteractionType => Artifact.Interaction.TouchAndContext;

	protected override string ConsiteAssetPath => "Models/Prop/module/site/site.prefab";

	protected override string ScaffoldingAssetPath => "Models/Prop/system/scaffolding/scaffolding_01_1x1_module.prefab";

	public bool LocalPlayerIsInHere { get; private set; }

	public VisibleState Visible { get; private set; }

	public virtual bool HasWall => false;

	public int Stories => base.Artifact.Stories.Value.GetValueOrDefault(1);

	public bool HasRoof => base.Artifact.HasRoof.Value.GetValueOrDefault(true);

	public override Vector3 InteractionPositionOffset => Vector3.up * 100f;

	public bool IsGodModeTarget
	{
		get
		{
			return _isGodModeTarget;
		}
		set
		{
			if (_isGodModeTarget != value)
			{
				_isGodModeTarget = value;
				SetDirtyVisibleState();
			}
		}
	}

	public override void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)
	{
		base.PostInit(blueprintId, worldTile, rotation, size);
		RooftopCheck();
		Observable<bool> isRooftop = _isRooftop;
		isRooftop.Changed = (Action<bool>)Delegate.Combine(isRooftop.Changed, (Action<bool>)delegate
		{
			_isDirtyPlayerInside = true;
		});
	}

	public override void OnUpdateCollider()
	{
		Vector3 vector = new Vector3(base.Size.x, 0f, base.Size.y) * 200f;
		vector.y = Height * 200;
		Vector3 center = vector * 0.5f;
		vector.x += 100f;
		vector.z += 100f;
		base.Artifact.CreateCollider(vector, center);
	}

	public override void ArtifactPlaced()
	{
		base.ArtifactPlaced();
		CheckTilesIndoor();
	}

	protected void CheckTilesIndoor()
	{
		SetTilesIndoor(base.Artifact.BuildState == BuildingState.Completed && HasWall);
	}

	public override void ResourcesLoadCompleted()
	{
		SetDirtyVisibleState();
	}

	public override void OnRemoved()
	{
		SetTilesIndoor(indoor: false);
	}

	private void SetVisible(VisibleState state)
	{
		Visible = state;
		SetDirtyVisibleState();
	}

	private void CheckPlayerInside()
	{
		bool flag = PlayerEntered && !CheckTriggerFlag(TriggerFlag.NotInside) && !_isRooftop;
		flag &= base.Artifact.BuildState == BuildingState.Completed;
		if (flag)
		{
			SetVisible(VisibleState.PlayerInside);
		}
		else if (Visible == VisibleState.PlayerInside)
		{
			SetVisible(VisibleState.Normal);
		}
		LocalPlayerIsInHere = flag && HasWall;
		PlayerBehavior.LocalPlayer.IsIndoor = LocalPlayerIsInHere;
		CheckObstructions();
		RefreshInteriorsMusicSwitch();
	}

	public override void Update()
	{
		base.Update();
		if (_isDirtyVisibleState)
		{
			_isDirtyVisibleState = false;
			UpdateVisibleState();
		}
		if (_isDirtyPlayerInside)
		{
			_isDirtyPlayerInside = false;
			CheckPlayerInside();
		}
	}

	private void SetDirtyVisibleState()
	{
		_isDirtyVisibleState = true;
	}

	protected virtual void UpdateVisibleState()
	{
	}

	public void OnTriggerEnter(TriggerFlag triggerFlag)
	{
		_triggerFlag |= triggerFlag;
		_isDirtyPlayerInside = true;
	}

	public void OnTriggerExit(TriggerFlag triggerFlag)
	{
		_triggerFlag &= ~triggerFlag;
		_isDirtyPlayerInside = true;
	}

	private bool CheckTriggerFlag(TriggerFlag flag)
	{
		return (_triggerFlag & flag) != 0;
	}

	public override void OnPlayerFloorChange()
	{
		base.OnPlayerFloorChange();
		RooftopCheck();
	}

	private void RooftopCheck()
	{
		if (HasRoof)
		{
			_isRooftop.Value = false;
		}
		else
		{
			_isRooftop.Value = (byte)PlayerBehavior.LocalPlayer.Floor + 1 == Stories;
		}
	}

	protected void AlphaTweenArtifactComponent(ModelComponent.IModel model, float alpha, float duration)
	{
		int num = -1;
		int i = 0;
		for (int count = _alphaTweenList.Count; i < count; i++)
		{
			if (_alphaTweenList[i].Model == model)
			{
				num = i;
				break;
			}
		}
		Color color = model.GetColor();
		if (duration <= 0f || Math.Abs(color.a - alpha) < 0.001f)
		{
			model.SetColor(color.WithA(alpha));
			if (num != -1)
			{
				_alphaTweenList.RemoveAt(num);
			}
			return;
		}
		AlphaTweenStruct alphaTweenStruct = default(AlphaTweenStruct);
		alphaTweenStruct.Model = model;
		alphaTweenStruct.StartAlpha = model.GetColor().a;
		alphaTweenStruct.EndAlpha = alpha;
		alphaTweenStruct.StartAt = Time.time;
		alphaTweenStruct.EndAt = Time.time + duration;
		AlphaTweenStruct alphaTweenStruct2 = alphaTweenStruct;
		if (num == -1)
		{
			_alphaTweenList.Add(alphaTweenStruct2);
		}
		else
		{
			_alphaTweenList[num] = alphaTweenStruct2;
		}
		if (!_isPlayAlphaTweenRoutine && _alphaTweenList.Count > 0 && base.Artifact != null)
		{
			base.Artifact.StartCoroutine(CoAlphaTweenArtifact());
		}
	}

	private IEnumerator CoAlphaTweenArtifact()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoAlphaTweenArtifact_003Ed__58(0)
		{
			_003C_003E4__this = this
		};
	}

	private void UpdateAlphaTween()
	{
		float time = Time.time;
		for (int num = _alphaTweenList.Count - 1; num >= 0; num--)
		{
			AlphaTweenStruct alphaTweenStruct = _alphaTweenList[num];
			Color color = alphaTweenStruct.Model.GetColor();
			if (alphaTweenStruct.EndAt < time)
			{
				alphaTweenStruct.Model.SetColor(color.WithA(alphaTweenStruct.EndAlpha));
				_alphaTweenList.RemoveAt(num);
			}
			else
			{
				color.a = Mathf.Lerp(alphaTweenStruct.StartAlpha, alphaTweenStruct.EndAlpha, (time - alphaTweenStruct.StartAt) / (alphaTweenStruct.EndAt - alphaTweenStruct.StartAt));
				alphaTweenStruct.Model.SetColor(color);
			}
		}
	}

	public override void OnPlayerEnter()
	{
		PlayerEntered = true;
		base.OnPlayerEnter();
		_isDirtyPlayerInside = true;
		RooftopCheck();
		Singleton<ArtifactManager>.Instance().ShowArtifactInteriorMsg(base.Artifact, base.Artifact.ArtifactState);
	}

	public override void OnPlayerExit()
	{
		PlayerEntered = false;
		base.OnPlayerExit();
		SetVisible(VisibleState.Normal);
		LocalPlayerIsInHere = false;
		PlayerBehavior.LocalPlayer.IsIndoor = LocalPlayerIsInHere;
		CheckObstructions();
		RefreshInteriorsMusicSwitch();
		Singleton<ArtifactManager>.Instance().HideArtifactInteriorMsg();
	}

	private void CheckObstructions()
	{
		ClearViewObstructions();
		if (Visible != VisibleState.PlayerInside)
		{
			return;
		}
		Point2 size = base.Size;
		Vector3 position = base.Artifact.transform.position;
		Vector3 position2 = Singleton<MainCamera>.Instance().transform.position;
		int mask = LayerHelper.PropMask;
		int i = 0;
		for (int num = size.x + size.y + 1; i < num; i++)
		{
			Vector3 vector;
			if (i == 0)
			{
				vector = position;
			}
			else if (i <= size.x)
			{
				int num2 = i;
				vector = position + Vector3.right * num2 * 200f;
			}
			else
			{
				int num3 = i - size.x;
				vector = position + Vector3.forward * num3 * 200f;
			}
			Vector3 direction = vector - position2;
			int count;
			RaycastHit[] array = Collisions.RayCast(new Ray(position2, direction), direction.magnitude - 100f, mask, out count);
			for (int j = 0; j < count; j++)
			{
				RaycastHit raycastHit = array[j];
				if (raycastHit.collider != null)
				{
					AddViewObstructions(raycastHit.collider.GetComponentInParent<Artifact>());
				}
			}
		}
	}

	private void RefreshInteriorsMusicSwitch()
	{
		Artifact[] interiors = base.Artifact.GetInteriors();
		if (interiors == null)
		{
			return;
		}
		bool occlusion = !LocalPlayerIsInHere;
		Artifact[] array = interiors;
		foreach (Artifact artifact in array)
		{
			if (artifact != null)
			{
				artifact.RefreshMusicSwitch(occlusion);
			}
		}
	}

	private void AddViewObstructions(Artifact artifact)
	{
		if (!(artifact == null))
		{
			EnterableArtifact artifactComponent = artifact.GetArtifactComponent<EnterableArtifact>();
			if (artifactComponent != null && artifactComponent != this && !_viewObstructions.Contains(artifactComponent))
			{
				artifactComponent.SetVisible(VisibleState.Obstruction);
				_viewObstructions.Add(artifactComponent);
			}
		}
	}

	private void ClearViewObstructions()
	{
		foreach (EnterableArtifact viewObstruction in _viewObstructions)
		{
			viewObstruction?.SetVisible(VisibleState.Normal);
		}
		_viewObstructions.Clear();
	}

	protected void SetTilesIndoor(bool indoor)
	{
		for (int i = 0; i < base.Size.y; i++)
		{
			for (int j = 0; j < base.Size.x; j++)
			{
				Point2 worldTile = base.WorldTile + new Point2(j, i);
				TileObject tileObject = Singleton<TerrainBase>.Instance().GetTileObject(worldTile, warning: false);
				if (tileObject != null)
				{
					tileObject.IsIndoor = indoor;
				}
			}
		}
	}

	public override void OnUpdateBuildState()
	{
		BuildingState buildState = base.Artifact.BuildState;
		if (buildState == BuildingState.Built || buildState == BuildingState.Remodeling)
		{
			MakeBuiltCollider();
		}
		else
		{
			RemoveBuiltCollider();
		}
		CheckTilesIndoor();
	}

	private void MakeBuiltCollider()
	{
		if (_builtCollider == null)
		{
			_builtCollider = base.Artifact.gameObject.AddComponent<BoxCollider>();
		}
		Vector3 vector = new Vector3(base.Size.x, Height, base.Size.y) * 200f;
		_builtCollider.center = vector * 0.5f;
		_builtCollider.size = vector;
	}

	private void RemoveBuiltCollider()
	{
		if (_builtCollider != null)
		{
			UnityEngine.Object.Destroy(_builtCollider);
		}
	}
}
