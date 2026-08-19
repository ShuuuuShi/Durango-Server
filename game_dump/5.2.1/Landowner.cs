using Durango.Logic.Clan;
using Durango.Logic.Estate;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using UnityEngine;

public class Landowner : ArtifactComponent
{
	private enum State
	{
		None,
		Protected,
		War
	}

	private enum Owner
	{
		None,
		Ally,
		Enemy
	}

	public const string DrawContainerName = "drawing_board";

	private MeshRenderer _drawContainer;

	private bool _drawContainerFlag;

	private bool _eventFlag;

	private Timer _protectedTimer;

	private EstateInfo _estateInfo;

	private State _state;

	private Owner _owner;

	private MeshRenderer DrawContainer
	{
		get
		{
			if (!_drawContainerFlag)
			{
				_drawContainerFlag = true;
				GameObject gameObject = KUtility.FindObjectByName(base.Artifact.gameObject, "drawing_board", includeInactive: true);
				if (gameObject != null)
				{
					_drawContainer = gameObject.GetComponent<MeshRenderer>();
				}
			}
			return _drawContainer;
		}
	}

	public ApngTexture DrawTexture { get; private set; }

	public override void ResourcesLoadCompleted()
	{
		MakeComponent();
		OnUpdateEstate();
		if (!_eventFlag && GameSystem<EstateSystem>.HasInstance())
		{
			_eventFlag = true;
			GameSystem<EstateSystem>.Instance().EstateGridUpdated += OnUpdateEstateGrids;
		}
	}

	public override void OnRemoved()
	{
		if (_eventFlag && GameSystem<EstateSystem>.HasInstance())
		{
			_eventFlag = false;
			GameSystem<EstateSystem>.Instance().EstateGridUpdated -= OnUpdateEstateGrids;
		}
		base.OnRemoved();
	}

	private void OnUpdateEstateGrids()
	{
		OnUpdateEstate();
	}

	private void MakeComponent()
	{
		MeshRenderer drawContainer = DrawContainer;
		if (drawContainer != null)
		{
			DrawTexture = drawContainer.gameObject.AddMissingComponent<ApngTexture>();
		}
		if (DrawTexture != null)
		{
			DrawTexture.enabled = false;
		}
	}

	private void OnUpdateEstate()
	{
		Point2 worldTile = base.WorldTile;
		Point2 size = base.Size;
		Point2 centerChunkCoords = Singleton<TerrainBase>.Instance().CenterChunkCoords;
		EstateInfo estateInfo = null;
		bool flag = false;
		for (int i = 0; i < size.x; i += Mathf.Max(1, size.x - 1))
		{
			for (int j = 0; j < size.y; j += Mathf.Max(1, size.y - 1))
			{
				Point2 point = worldTile + new Point2(i, j);
				Point2 point2 = Durango.Terrain.Util.TilePositionToChunkCoords(point);
				if (Mathf.Abs(point2.x - centerChunkCoords.x) < 3 && Mathf.Abs(point2.y - centerChunkCoords.y) < 3 && EstateSystem.TryGetEstateInfo(point, out var info))
				{
					flag = true;
					if (info != null)
					{
						estateInfo = info;
						break;
					}
				}
			}
		}
		if (!flag)
		{
			_estateInfo = null;
			return;
		}
		_estateInfo = estateInfo;
		if (estateInfo == null)
		{
			if (DrawTexture != null)
			{
				DrawTexture.gameObject.SetActive(value: false);
			}
			if (_protectedTimer != null)
			{
				_protectedTimer.Stop();
			}
			SetState(Owner.Enemy, State.War);
			return;
		}
		Owner owner = Owner.Enemy;
		State state = State.War;
		if (ClanSystem.IsMyClanOrAlliance(estateInfo.License.OwnerId))
		{
			owner = Owner.Ally;
		}
		if (estateInfo.License.IsProtected())
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			double value = estateInfo.License.ProtectedUntil.Value;
			double num = predictedServerTime;
			float num2 = (float)(value - num);
			float ratio = (float)((predictedServerTime - num) / (double)num2);
			if (_protectedTimer == null || _protectedTimer.IsStop)
			{
				_protectedTimer = new Timer(base.EntityId, "protected", num2, ratio);
				Timer.Play<UnknownSinceProgressGauge>(_protectedTimer).SetTarget(base.Artifact.gameObject, base.Artifact.InteractionPosition - base.Artifact.transform.localPosition);
			}
			else
			{
				_protectedTimer.SetDuration(base.EntityId, "protected", num2, ratio);
			}
			state = State.Protected;
		}
		else if (_protectedTimer != null)
		{
			_protectedTimer.Stop();
		}
		SetState(owner, state);
		if (!(DrawTexture == null))
		{
			ClanSystem.GetEmblem(estateInfo.License.OwnerId, OnEmblem);
		}
	}

	private void SetState(Owner owner, State state)
	{
		if (_owner == owner && _state == state)
		{
			return;
		}
		if (_owner == Owner.None || _state == State.None)
		{
			_owner = owner;
			_state = state;
			return;
		}
		if (_owner == owner)
		{
			switch (state)
			{
			case State.Protected:
				if (owner == Owner.Ally)
				{
					UIManager.Alarm.War.Show(AlarmWar.Type.DefenceSuccess);
				}
				else
				{
					UIManager.Alarm.War.Show(AlarmWar.Type.AttackFail);
				}
				break;
			case State.War:
				UIManager.Alarm.War.Show(AlarmWar.Type.ProtectedEnded);
				break;
			}
		}
		else
		{
			switch (owner)
			{
			case Owner.Ally:
				UIManager.Alarm.War.Show(AlarmWar.Type.Occupied);
				break;
			case Owner.Enemy:
				if (_owner == Owner.Ally)
				{
					UIManager.Alarm.War.Show(AlarmWar.Type.Unoccupied);
				}
				else
				{
					if (_owner != Owner.Enemy || _estateInfo == null)
					{
						break;
					}
					ClanSystem.GetClanInfo(_estateInfo.License.OwnerId, delegate(Clan clan)
					{
						if (clan != null)
						{
							UIManager.Alarm.War.Show(AlarmWar.Type.OtherOccupied, null, new object[1] { clan.Name });
						}
					}, refresh: false, detail: false);
				}
				break;
			}
			Transform transform = KUtility.FindTransformByName(base.Artifact.gameObject, "effect");
			if (transform == null)
			{
				transform = base.Artifact.transform;
			}
			ParticleManager.Emit("Particle/Prefabs/Durango_01/06_Structure/FX_Structure_Warphole_Occupation_01.prefab", transform.position, Quaternion.identity);
		}
		_owner = owner;
		_state = state;
	}

	private void OnEmblem(Point2 pos)
	{
		if (!(DrawTexture == null))
		{
			if (pos.x < 0 || pos.y < 0)
			{
				DrawTexture.gameObject.SetActive(value: false);
				return;
			}
			DrawTexture.gameObject.SetActive(value: true);
			EmblemTexture.Set(DrawTexture, pos);
		}
	}
}
