using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.Logic.Estate;
using Durango.Network;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.InGame;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Estate;
using UnityEngine;

public class EstateSystem : GameSystem<EstateSystem>
{
	public const int ExtendDays = 7;

	public const int EstateGridSize = 4;

	private const int EstateGridArrayLength = 12;

	private readonly List<EstateInfo> _estates = new List<EstateInfo>();

	private EstateInfo[,] _estateGrids;

	private Point2 _tileOffset;

	private float _estateGridsUpdateAt;

	private double _estateGridsValidSince;

	private float _estateGridsInvalidatedAt;

	private EstateLicenses? _lastLicenses;

	private bool _pioneerPointPaid;

	private PioneerGradeInfo? _pioneerGradeInfo;

	private string _estateIdForHideLines;

	public EstateInfo CurrentEstate { get; private set; }

	public PioneerGradeInfo PioneerGradeInfo
	{
		get
		{
			PioneerGradeInfo? pioneerGradeInfo = _pioneerGradeInfo;
			if (pioneerGradeInfo.HasValue)
			{
				return _pioneerGradeInfo.Value;
			}
			return default(PioneerGradeInfo);
		}
	}

	public PersonalIslandInfo? PersonalIslandInfo { get; private set; }

	public static Shared.Estate.AccessRights RightsOnPreset { get; private set; }

	public bool DisableEstateOwnerPopup { get; set; }

	public event Action<EstateLicense> EstateDeclared;

	public event Action EstateGridUpdated;

	public event Action<EstateInfo> CurrentEstateChanged;

	public event Action<PioneerGradeInfo> PioneerGradeInfoUpdated;

	public event Action<int, int> PioneerGradeLevelChanged;

	static EstateSystem()
	{
		RightsOnPreset = Shared.Estate.AccessRights.Enter | Shared.Estate.AccessRights.UseFacility | Shared.Estate.AccessRights.Give;
	}

	private void Awake()
	{
		Connections.Frontend.On<EstateGrids>(OnEstateGrid);
		Connections.Frontend.On<EstateLicense>(OnEstateLicenseChanged);
		Connections.Frontend.On(delegate(PersonalIslandInfo msg, PacketHeader header)
		{
			PersonalIslandInfo = msg;
		});
		Connections.Frontend.On(delegate(PioneerGradeInfo msg, PacketHeader header)
		{
			PioneerGradeInfo? pioneerGradeInfo = _pioneerGradeInfo;
			if (pioneerGradeInfo.HasValue)
			{
				int grade = _pioneerGradeInfo.Value.Grade;
				if (grade != msg.Grade && this.PioneerGradeLevelChanged != null)
				{
					this.PioneerGradeLevelChanged(grade, msg.Grade);
				}
			}
			_pioneerGradeInfo = msg;
			_pioneerPointPaid = msg.IsPaid();
			if (this.PioneerGradeInfoUpdated != null)
			{
				this.PioneerGradeInfoUpdated(PioneerGradeInfo);
			}
		});
		InitEstateGrids();
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (!GameManager.IsPrologueMode)
			{
				PlayerBehavior.LocalPlayer.TileChanged += LocalPlayer_TileChanged;
			}
		};
		Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			Connections.Frontend.Send(default(GetPioneerGradeInfo));
		});
	}

	private void LocalPlayer_TileChanged(Point2 prev, Point2 current)
	{
		RefreshCurrentEstate(current);
	}

	private void OnEnterEstate([NotNull] EstateInfo estate)
	{
		if (estate.License.Type != OwnerType.System)
		{
			if (_estateIdForHideLines != estate.Id)
			{
				estate.ShowEstateLines();
			}
			if (!DisableEstateOwnerPopup)
			{
				UIManager.Popup.EstateOwnerWidget.Show(estate);
			}
		}
	}

	private static void OnLeaveEstate([NotNull] EstateInfo estate)
	{
		if (estate.License.Type != OwnerType.System)
		{
			estate.HideEstateLines();
			UIManager.Popup.EstateOwnerWidget.Hide();
		}
	}

	private void Update()
	{
		float time = Time.time;
		if (_estateGridsUpdateAt > 0f && _estateGridsUpdateAt < time)
		{
			UpdateEstateInfos();
		}
		if (_estateGridsInvalidatedAt > 0f && _estateGridsInvalidatedAt < time)
		{
			RefershInvalidateEstateInfo();
		}
		bool flag = PioneerGradeInfo.IsPaid();
		if (_pioneerPointPaid != flag)
		{
			_pioneerPointPaid = flag;
			if (this.PioneerGradeInfoUpdated != null)
			{
				this.PioneerGradeInfoUpdated(PioneerGradeInfo);
			}
		}
	}

	private void InitEstateGrids()
	{
		_estateGrids = new EstateInfo[12, 12];
	}

	public void UpdateEstateInfos()
	{
		double? num = null;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		for (int i = 0; i < _estateGrids.GetLength(0); i++)
		{
			for (int j = 0; j < _estateGrids.GetLength(1); j++)
			{
				_estateGrids[i, j] = null;
			}
		}
		MapContext mapContext = Singleton<MapContext>.Instance();
		Point2 point = _tileOffset / 4;
		for (int num2 = _estates.Count - 1; num2 >= 0; num2--)
		{
			EstateInfo estateInfo = _estates[num2];
			if (IsValidEstateInfo(estateInfo))
			{
				num = GetEarlyTime(predictedServerTime, num, estateInfo.License.ActivatedAt, estateInfo.License.ExpiresAt, estateInfo.License.ProtectedUntil);
				for (int k = 0; k < estateInfo.Units.Count; k++)
				{
					Point2 point2 = estateInfo.Units[k] - point;
					if (point2.x >= 0 && point2.y >= 0 && point2.x < _estateGrids.GetLength(0) && point2.y < _estateGrids.GetLength(1))
					{
						_estateGrids[point2.x, point2.y] = estateInfo;
					}
				}
				for (int num3 = estateInfo.Units.Count - 1; num3 >= 0; num3--)
				{
					Point2 point3 = estateInfo.Units[num3] / 4;
					mapContext.RefreshChunk(point3.x, point3.y);
				}
				estateInfo.RefreshEstateArea(estateInfo.Id != _estateIdForHideLines);
			}
			else
			{
				estateInfo.Dispose();
				_estates.RemoveAt(num2);
			}
		}
		if (num.HasValue)
		{
			_estateGridsUpdateAt = Time.time + (float)(num.Value - predictedServerTime);
		}
		else
		{
			_estateGridsUpdateAt = 0f;
		}
		CalcNextInvalidateAtEstateInfo();
		RefreshCurrentEstate(PlayerBehavior.LocalPlayer.CurrentTile);
		if (this.EstateGridUpdated != null)
		{
			this.EstateGridUpdated();
		}
	}

	public void SetVisibleEstateLines(string estateId, bool visible)
	{
		for (int i = 0; i < _estates.Count; i++)
		{
			EstateInfo estateInfo = _estates[i];
			if (estateInfo.Id == estateId)
			{
				if (visible)
				{
					estateInfo.ShowEstateLines();
				}
				else
				{
					estateInfo.HideEstateLines();
				}
			}
		}
		_estateIdForHideLines = ((!visible) ? estateId : null);
	}

	private void RefreshCurrentEstate(Point2 current)
	{
		EstateInfo estateInfo = GetEstateInfo(current);
		if (!(GetEstateId(CurrentEstate) == GetEstateId(estateInfo)))
		{
			EstateInfo currentEstate = CurrentEstate;
			CurrentEstate = estateInfo;
			if (currentEstate != null)
			{
				OnLeaveEstate(currentEstate);
			}
			if (CurrentEstate != null)
			{
				OnEnterEstate(CurrentEstate);
			}
			if (this.CurrentEstateChanged != null)
			{
				this.CurrentEstateChanged(CurrentEstate);
			}
		}
	}

	private static string GetEstateId(EstateInfo estate)
	{
		return estate?.Id;
	}

	private void RefershInvalidateEstateInfo()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		for (int i = 0; i < _estates.Count; i++)
		{
			EstateInfo estateInfo = _estates[i];
			if (estateInfo.License.CycleEndsAt.HasValue && estateInfo.License.CycleEndsAt.Value > _estateGridsValidSince && estateInfo.License.CycleEndsAt.Value < predictedServerTime)
			{
				Connections.Frontend.Send(new GetEstateLicenseById
				{
					EstateId = estateInfo.Id
				});
			}
		}
		CalcNextInvalidateAtEstateInfo();
	}

	private void CalcNextInvalidateAtEstateInfo()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double? num = null;
		for (int i = 0; i < _estates.Count; i++)
		{
			EstateInfo estateInfo = _estates[i];
			num = GetEarlyTime(predictedServerTime, num, estateInfo.License.CycleEndsAt);
		}
		_estateGridsValidSince = Connections.Frontend.GetPredictedServerTime();
		if (num.HasValue)
		{
			_estateGridsInvalidatedAt = Time.time + (float)(num.Value - predictedServerTime);
		}
		else
		{
			_estateGridsInvalidatedAt = 0f;
		}
	}

	public IEnumerable<EstateInfo> GetEstates()
	{
		return _estates;
	}

	private static double? GetEarlyTime(double now, params double?[] times)
	{
		double? result = null;
		for (int i = 0; i < times.Length; i++)
		{
			double? num = times[i];
			if (num.HasValue && num.Value > now)
			{
				result = ((!result.HasValue) ? num.Value : Math.Min(result.Value, num.Value));
			}
		}
		return result;
	}

	public void OnEstateGrid(EstateGrids msg, PacketHeader header)
	{
		Point2 centerChunkCoords = Singleton<TerrainBase>.Instance().CenterChunkCoords;
		_tileOffset = Singleton<GridAreaViewer>.Instance().GetTileOffset();
		MapContext mapContext = Singleton<MapContext>.Instance();
		int i = 0;
		for (int size = KUtility.GetSize(msg.Chunks); i < size; i++)
		{
			Point2 point = msg.Chunks[i];
			mapContext.RefreshChunk(point.x, point.y);
			for (int j = 0; j < _estates.Count; j++)
			{
				EstateInfo estateInfo = _estates[j];
				for (int num = estateInfo.Units.Count - 1; num >= 0; num--)
				{
					Point2 point2 = estateInfo.Units[num] / 4;
					bool flag = point2 == point;
					if (!flag)
					{
						Point2 point3 = point2 - centerChunkCoords;
						flag = Mathf.Abs(point3.x) > 1 || Mathf.Abs(point3.y) > 1;
					}
					if (flag)
					{
						estateInfo.RemoveAt(num);
					}
				}
			}
		}
		foreach (KeyValuePair<Point2, string> cell in msg.Cells)
		{
			string value = cell.Value;
			EstateInfo estateInfo2 = GetEstateInfo(value);
			if (estateInfo2 == null)
			{
				estateInfo2 = new EstateInfo(value);
				_estates.Add(estateInfo2);
			}
			estateInfo2.AddUnit(cell.Key);
		}
		int k = 0;
		for (int size2 = KUtility.GetSize(msg.EstateLicenses); k < size2; k++)
		{
			SetLicense(msg.EstateLicenses[k]);
		}
		UpdateEstateInfos();
	}

	private void OnEstateLicenseChanged(EstateLicense msg, PacketHeader header)
	{
		SetLicense(msg);
	}

	private void SetLicense(EstateLicense license)
	{
		EstateInfo estateInfo = GetEstateInfo(license.EstateId);
		if (estateInfo != null)
		{
			estateInfo.Set(license);
			_estateGridsUpdateAt = Time.time;
		}
	}

	private EstateInfo GetEstateInfo(string id)
	{
		for (int i = 0; i < _estates.Count; i++)
		{
			if (_estates[i].Id == id)
			{
				return _estates[i];
			}
		}
		return null;
	}

	public static bool TryGetEstateInfo(Point2 tile, out EstateInfo info)
	{
		EstateSystem estateSystem = GameSystem<EstateSystem>.Instance();
		Point2 tileOffset = estateSystem._tileOffset;
		int num = tile.x - tileOffset.x;
		int num2 = tile.y - tileOffset.y;
		if (num < 0 || num2 < 0 || num >= 48 || num2 >= 48)
		{
			info = null;
			return false;
		}
		num /= 4;
		num2 /= 4;
		info = estateSystem._estateGrids[num, num2];
		return true;
	}

	public static EstateInfo GetEstateInfo(Point2 tile)
	{
		TryGetEstateInfo(tile, out var info);
		return info;
	}

	private static bool IsValidEstateInfo(EstateInfo info)
	{
		if (string.IsNullOrEmpty(info.Id) || info.Id != info.License.EstateId || info.Units.Count == 0)
		{
			return false;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double activatedAt = info.License.ActivatedAt;
		double? expiresAt = info.License.ExpiresAt;
		double num = ((!expiresAt.HasValue) ? 0.0 : expiresAt.Value);
		if (activatedAt <= 0.0 || activatedAt < predictedServerTime)
		{
			if (!(num <= 0.0))
			{
				return predictedServerTime < num;
			}
			return true;
		}
		return false;
	}

	public static void GetEstateLicenses([NotNull] Action<EstateLicenses> onResult)
	{
		Connections.Frontend.Send(default(GetEstateLicenses)).On(delegate(EstateLicenses msg, PacketHeader header)
		{
			GameSystem<EstateSystem>.Instance().UpdateLicenses(msg);
			onResult(msg);
		});
	}

	public static void GetPersonalRegionInfo([NotNull] Action<PersonalRegionInfo> onResult)
	{
		Connections.Frontend.Send(default(GetPersonalRegionInfo)).On(delegate(PersonalRegionInfo msg, PacketHeader header)
		{
			onResult(msg);
		});
	}

	private void UpdateLicenses(EstateLicenses licenses)
	{
		_lastLicenses = licenses;
		UpdateLicense(licenses.PersonalEstate);
		UpdateLicense(licenses.UrbanEstate);
		UpdateLicense(licenses.ClanEstate);
	}

	private void UpdateLicense(EstateLicense? license)
	{
		if (license.HasValue)
		{
			GetEstateInfo(license.Value.EstateId)?.Set(license.Value);
		}
	}

	public static void SetEstateLicense(string estateId, Messages.AccessRights rights, Action onSuccess)
	{
		Connections.Frontend.Send(new SetEstateLicense
		{
			EstateId = estateId,
			AccessRights = rights
		}).On<OK>(delegate
		{
			if (onSuccess != null)
			{
				onSuccess();
			}
		});
	}

	public static void ExpandEstate(string id, Point2 cell, Action<EstateLicense> onSuccess)
	{
		Connections.Frontend.Send(new ExpandEstate
		{
			EstateId = id,
			Cell = cell
		}).On(delegate(EstateLicense msg, PacketHeader header)
		{
			if (onSuccess != null)
			{
				onSuccess(msg);
			}
		});
	}

	public static void ShrinkEstate(string id, Point2 cell, Action<EstateLicense> onSuccess)
	{
		Connections.Frontend.Send(new ShrinkEstate
		{
			EstateId = id,
			Cell = cell
		}).On(delegate(EstateLicense msg, PacketHeader header)
		{
			if (onSuccess != null)
			{
				onSuccess(msg);
			}
		});
	}

	public static void RemoveEstate(string id)
	{
		Connections.Frontend.Send(new RemoveEstate
		{
			EstateId = id
		});
	}

	public static void ExtendEstate(string id, long cost, Action<EstateLicense> onSuccess)
	{
		Connections.Frontend.Send(new ExtendEstateActivation
		{
			EstateId = id,
			Cost = (int)cost
		}).On(delegate(EstateLicense msg, PacketHeader header)
		{
			if (onSuccess != null)
			{
				onSuccess(msg);
			}
		});
	}

	public void DeclareEstate(OwnerType ownerType, Point2 cell, Action<EstateLicense> onSuccess)
	{
		Connections.Frontend.Send(new DeclareEstate
		{
			OwnerType = ownerType,
			Cell = cell
		}).On(delegate(EstateLicense msg, PacketHeader header)
		{
			if (this.EstateDeclared != null)
			{
				this.EstateDeclared(msg);
			}
			if (onSuccess != null)
			{
				onSuccess(msg);
			}
		});
	}

	public static bool CanVisitEstate()
	{
		if (GameManager.Region.IsAfterSafeHouse())
		{
			return PlayerBehavior.LocalPlayer.Driver.IsWarpAllowed;
		}
		return false;
	}

	public static void VisitEstate(OwnerType ownerType, string id, Money? cost = null)
	{
		if (!CanVisitEstate())
		{
			UIManager.MessageBox.Show(T._("개인섬부터 이용할 수 있습니다."));
			return;
		}
		Region region = default(Region);
		if (ownerType == OwnerType.ClanWarphole && ClanSystem.IsMyClan(id))
		{
			EstateLicenses? lastLicenses = GameSystem<EstateSystem>.Instance()._lastLicenses;
			if (lastLicenses.HasValue)
			{
				lastLicenses.Value.TryGetClanCargoWarpholeRegion(out region);
			}
		}
		MapSystem.WarpTo to = OwnerTypeToWarpTo(ownerType);
		GameSystem<MapSystem>.Instance().TryWarp(() => Connections.Frontend.Send(new VisitEstate
		{
			OwnerType = ownerType,
			OwnerId = id,
			Cost = cost,
			RegionId = ((ownerType != OwnerType.PersonalPlayer) ? null : Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(id).PersonalRegionId)
		}), to, region);
	}

	public static void ReturnToEstate(OwnerType type)
	{
		MapSystem.WarpTo to = OwnerTypeToWarpTo(type);
		GameSystem<MapSystem>.Instance().TryWarp(() => Connections.Frontend.Send(new ReturnToEstate
		{
			OwnerType = type
		}), to);
	}

	private static MapSystem.WarpTo OwnerTypeToWarpTo(OwnerType ownerType)
	{
		return ownerType switch
		{
			OwnerType.Player => MapSystem.WarpTo.UrbanEstate, 
			OwnerType.ClanEstate => MapSystem.WarpTo.ClanEstate, 
			OwnerType.ClanWarphole => MapSystem.WarpTo.ClanWarphole, 
			OwnerType.PersonalPlayer => MapSystem.WarpTo.PersonalEstate, 
			_ => MapSystem.WarpTo.Warp, 
		};
	}

	public static void CargoWarpholeTaxToClanFund(Action<ClanCargoWarphole> onSuccess)
	{
		Connections.Frontend.Send(default(CargoWarpholeTaxToClanFund)).On(delegate(ClanCargoWarphole msg, PacketHeader header)
		{
			if (onSuccess != null)
			{
				onSuccess(msg);
			}
		});
	}

	public static void SetCargoWarpholeTaxRate(float rate)
	{
		Connections.Frontend.Send(new SetCargoWarpholeTaxRate
		{
			Rate = rate
		});
	}

	public static bool IsAdmin(EstateInfo estate)
	{
		if (estate == null)
		{
			return false;
		}
		switch (estate.License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			return GameManager.PlayerId == estate.License.OwnerId;
		case OwnerType.ClanEstate:
		case OwnerType.ClanWarphole:
			if (ClanSystem.IsMyClan(estate.License.OwnerId))
			{
				Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
				if (playerClan != null && playerClan.TryGetRole(PlayerBehavior.LocalPlayer.Clan.RoleId, out var role))
				{
					return role.IsSuperuser();
				}
			}
			break;
		}
		return false;
	}

	public static void SetArtifactAccess([NotNull] Artifact artifact, ArtifactAccess access, Action<bool> onResult)
	{
		Connections.Frontend.Send(new SetArtifactAccess
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile,
			Access = access
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public static void UseItemsForPioneerPoint([NotNull] Artifact artifact, string[] itemIds, Action<bool> onResult)
	{
		Connections.Frontend.Send(new UseItemsForPioneerPoint
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile,
			ItemIds = itemIds
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}
}
