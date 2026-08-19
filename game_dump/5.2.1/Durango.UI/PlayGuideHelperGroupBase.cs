using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Durango.Logic.PlayGuide;
using Durango.Network;
using Durango.Terrain;
using Durango.UI.PlayGuide.ClickTarget;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Etc;
using Shared.Region;
using Shared.System;
using UnityEngine;

namespace Durango.UI;

public class PlayGuideHelperGroupBase : UIBase
{
	private class Cache
	{
		public readonly GuideEvent GuideEvent;

		public Locator Locator;

		public Point2? HelperTile;

		public int[] Immovable;

		public Cache(GuideEvent guideEvent)
		{
			GuideEvent = guideEvent;
		}
	}

	private class HelperCache : IEnumerable<KeyValuePair<HelperTarget, Cache>>, IEnumerable
	{
		private readonly Dictionary<HelperTarget, Cache> _dict = new Dictionary<HelperTarget, Cache>();

		public IEnumerator<KeyValuePair<HelperTarget, Cache>> GetEnumerator()
		{
			return _dict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		[CanBeNull]
		public Cache Get([NotNull] HelperTarget target)
		{
			return _dict.Get(target);
		}

		[NotNull]
		public Cache GetOrCreate([NotNull] GuideEvent guideEvent, [NotNull] HelperTarget target)
		{
			Cache cache = Get(target);
			if (cache == null)
			{
				cache = new Cache(guideEvent);
				_dict[target] = cache;
			}
			return cache;
		}

		public void Remove([NotNull] HelperTarget target)
		{
			_dict.Remove(target);
		}
	}

	[CompilerGenerated]
	private sealed class _003CDisplayGuidePopup_003Ed__14 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string title;

		public string comment;

		public SpotlightTarget spotlight;

		public Transform target;

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
		public _003CDisplayGuidePopup_003Ed__14(int _003C_003E1__state)
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
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				GuideTooltip guideTooltip = UIManager.Popup.Tooltip<GuideTooltip>();
				guideTooltip.Set(title, comment);
				guideTooltip.CommentWidth = spotlight.comment_width;
				guideTooltip.Direction = spotlight.direction;
				Vector2 offset = new Vector2(spotlight.x_offset, spotlight.y_offset);
				guideTooltip.DragLock = true;
				guideTooltip.Show(target.gameObject, offset, 3600f);
				guideTooltip.ModifyDrawPanel(target);
				guideTooltip.LockSkip(GuideTooltip.SpotlightGuideUnskippableTime);
				SoundManager.PlayEvent("ui_guide_pop_up");
				guideTooltip.AddOnFinished(GameSystem<PlayGuideSystem>.Instance().OnGuideMsgFinished);
				return false;
			}
			}
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

	[SerializeField]
	private UIPanel _clickTargetPanel;

	[SerializeField]
	private GameObject _clickTarget;

	[SerializeField]
	private SoundEventType _clickTargetSound;

	private readonly List<Locator> _locators = new List<Locator>();

	private readonly HelperCache _helperCache = new HelperCache();

	private Locator Locator => _locators.LastOrDefault();

	private void Start()
	{
		SoundManager.PrepareEvent(_clickTargetSound);
		_clickTarget.SetActive(value: false);
		GameSystem<PlayGuideSystem>.Instance().HelperTargetApplied += PlayGuideSystem_HelperTargetApplied;
		GameSystem<PlayGuideSystem>.Instance().HelperTargetRemoved += PlayGuideSystem_HelperTargetRemoved;
		GameSystem<PlayGuideSystem>.Instance().EventChanged += delegate(GuideEvent prev, GuideEvent cur)
		{
			ApplySpotlightTarget(cur);
			if (!string.IsNullOrEmpty(cur.CardNews))
			{
				CardNewsPopup cardNewsPopup = UIManager.Popup.Tooltip<CardNewsPopup>();
				if (cardNewsPopup.Load(cur.CardNews))
				{
					cardNewsPopup.Show();
				}
			}
		};
		Singleton<ArtifactManager>.Instance().Added += delegate(Artifact artifact)
		{
			foreach (KeyValuePair<HelperTarget, Cache> item in _helperCache)
			{
				if (item.Value.Immovable != null && item.Value.Immovable.Contains(artifact.EntityType))
				{
					SetArrowHelperTarget(item.Value.GuideEvent, item.Key);
				}
			}
		};
		Connections.Frontend.On(delegate(NearestPOI msg, PacketHeader _)
		{
			foreach (KeyValuePair<HelperTarget, Cache> item2 in _helperCache)
			{
				if (item2.Key.type == "nearest_poi")
				{
					OnHelperTileChanged(item2.Key, msg.Tile);
					break;
				}
			}
		});
	}

	private void PlayGuideSystem_HelperTargetApplied(GuideEvent guideEvent)
	{
		for (int i = 0; i < KUtility.GetSize(guideEvent.HelperTargets); i++)
		{
			HelperTarget helperTarget = guideEvent.HelperTargets[i];
			if (IsArrowHelperTarget(helperTarget.type))
			{
				SetArrowHelperTarget(guideEvent, helperTarget);
				continue;
			}
			string type = helperTarget.type;
			if (type != null && type == "click")
			{
				Cache orCreate = _helperCache.GetOrCreate(guideEvent, helperTarget);
				if (orCreate.Locator == null)
				{
					orCreate.Locator = Factory.Create(helperTarget.id, helperTarget.click_targets);
				}
				EnableClickTarget(orCreate.Locator);
			}
		}
	}

	private void SetArrowHelperTarget(GuideEvent guideEvent, HelperTarget helper)
	{
		Vector3 vector = CalcArrowHelperTarget(guideEvent, helper);
		if (vector == Vector3.zero)
		{
			vector = TileToClientPosition(helper.tile);
		}
		if (vector != Vector3.zero)
		{
			SetNavigateTarget(guideEvent, vector);
		}
	}

	private void PlayGuideSystem_HelperTargetRemoved(GuideEvent guideEvent)
	{
		for (int i = 0; i < KUtility.GetSize(guideEvent.HelperTargets); i++)
		{
			HelperTarget helperTarget = guideEvent.HelperTargets[i];
			if (IsArrowHelperTarget(helperTarget.type))
			{
				ClearNavigateTaget(guideEvent);
			}
			else
			{
				string type = helperTarget.type;
				if (type != null && type == "click")
				{
					Cache cache = _helperCache.Get(helperTarget);
					if (cache != null)
					{
						DisableClickTarget(cache.Locator);
					}
				}
			}
			_helperCache.Remove(helperTarget);
		}
	}

	private void ApplySpotlightTarget(GuideEvent guideEvent)
	{
		if (guideEvent.SpotlightTarget == null)
		{
			return;
		}
		SpotlightTarget spotlightTarget = guideEvent.SpotlightTarget;
		Transform transform = Singleton<UIManager>.Instance().FindTransform(spotlightTarget.id);
		if (!(transform == null))
		{
			string text = T._(spotlightTarget.title);
			string text2 = T._(spotlightTarget.comment);
			if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(text2))
			{
				StartCoroutine(DisplayGuidePopup(text, text2, spotlightTarget, transform));
			}
		}
	}

	private static IEnumerator DisplayGuidePopup(string title, string comment, SpotlightTarget spotlight, Transform target)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDisplayGuidePopup_003Ed__14(0)
		{
			title = title,
			comment = comment,
			spotlight = spotlight,
			target = target
		};
	}

	protected void LateUpdate()
	{
		Locator locator = Locator;
		if (locator == null)
		{
			_clickTarget.SetActive(value: false);
			base.enabled = false;
			return;
		}
		locator.Process();
		_clickTarget.transform.localPosition = GetCurrentClickTargetPos();
		_clickTarget.transform.localRotation = Quaternion.Euler(0f, 0f, locator.Rotate());
		if (_clickTargetPanel.gameObject.layer != locator.PanelLayer)
		{
			NGUITools.SetLayer(_clickTargetPanel.gameObject, locator.PanelLayer);
		}
		_clickTargetPanel.depth = locator.PanelDepth;
		bool activeSelf = _clickTarget.activeSelf;
		bool flag = locator.IsVisible();
		if (GameSystem<TimerSystem>.Instance().HasTimerExceptPostProcess())
		{
			flag = false;
		}
		_clickTarget.SetActive(flag);
		if (!activeSelf && flag)
		{
			OnBeginVisible();
		}
	}

	protected virtual void OnBeginVisible()
	{
		SoundManager.PlayEvent(_clickTargetSound);
	}

	private void EnableClickTarget([NotNull] Locator locator)
	{
		DisableClickTarget(locator);
		_locators.Add(locator);
		base.enabled = true;
	}

	private void DisableClickTarget(Locator locator)
	{
		_locators.Remove(locator);
	}

	private Vector3 GetCurrentClickTargetPos()
	{
		Locator locator = Locator;
		Vector3 nGUIPosition = locator.GetNGUIPosition();
		Vector2 offset = locator.GetOffset();
		return nGUIPosition + new Vector3(offset.x, offset.y);
	}

	private static bool IsArrowHelperTarget(string type)
	{
		switch (type)
		{
		case "biome":
		case "natural":
		case "immovable":
		case "tile":
		case "nearest_poi":
		case "entity_type":
			return true;
		default:
			return false;
		}
	}

	private Vector3 CalcArrowHelperTarget(GuideEvent guideEvent, HelperTarget helper)
	{
		string type = helper.type;
		string id = helper.id;
		switch (type)
		{
		case "biome":
		{
			Biome[] biomes = DataHelper.ParseBiome(id);
			return Singleton<TerrainBase>.Instance().GetNearestBiome(biomes, PlayerBehavior.LocalPlayer.CurrentPosition);
		}
		case "natural":
		{
			int[] entityTypes = DataHelper.ParseEntityTypes(id);
			return Singleton<TerrainBase>.Instance().GetNearestNaturalObject(entityTypes, PlayerBehavior.LocalPlayer.CurrentPosition);
		}
		case "immovable":
		{
			Cache orCreate3 = _helperCache.GetOrCreate(guideEvent, helper);
			if (orCreate3.Immovable == null)
			{
				orCreate3.Immovable = DataHelper.ParseEntityTypes(id);
			}
			ImmovableBase nearestImmovable = Durango.Logic.PlayGuide.Util.GetNearestImmovable(orCreate3.Immovable, 9600f);
			if (nearestImmovable != null)
			{
				return nearestImmovable.InteractionPosition;
			}
			return Vector3.zero;
		}
		case "nearest_poi":
		{
			Shared.System.PointOfInterest type2 = id.ToEnum(Shared.System.PointOfInterest.Port);
			Cache orCreate2 = _helperCache.GetOrCreate(guideEvent, helper);
			Connections.Frontend.Send(new RequestNearestPOI
			{
				Tile = PlayerBehavior.LocalPlayer.CurrentTile,
				Type = type2
			});
			return GetHelperTileClientPosition(orCreate2.HelperTile);
		}
		case "entity_type":
		{
			Cache orCreate = _helperCache.GetOrCreate(guideEvent, helper);
			Connections.Frontend.Send(new FindTargetEntityPosition
			{
				EntityType = (ushort)id.ToInt(),
				ReasonFindTarget = ReasonFindTarget.PlayGuide
			}).On(delegate(TargetEntityPosition msg, PacketHeader header)
			{
				OnHelperTileChanged(helper, msg.Tile);
			});
			return GetHelperTileClientPosition(orCreate.HelperTile);
		}
		default:
			return Vector3.zero;
		}
	}

	private void OnHelperTileChanged(HelperTarget helper, Point2? tile)
	{
		Cache cache = _helperCache.Get(helper);
		if (cache != null)
		{
			cache.HelperTile = tile;
			Vector3 helperTileClientPosition = GetHelperTileClientPosition(tile);
			SetNavigateTarget(cache.GuideEvent, helperTileClientPosition);
		}
	}

	private static Vector3 GetHelperTileClientPosition(Point2? helperTile)
	{
		if (helperTile.HasValue)
		{
			return Durango.Terrain.Util.TilePositionToClientPosition(helperTile.Value, tileCenter: true);
		}
		return Vector3.zero;
	}

	private static void SetNavigateTarget(GuideEvent guide, Vector3 pos)
	{
		UIManager.FindScript<NavigateGroup>().Point.SetTarget(guide.Name, new PointTargetController.Arguments
		{
			Position = pos,
			Icon = guide.NPCType.ToDoIcon()
		});
	}

	private static void ClearNavigateTaget(GuideEvent guide)
	{
		UIManager.FindScript<NavigateGroup>().Point.ClearTarget(guide.Name);
	}

	private static Vector2 StringToTile(string tile)
	{
		if (string.IsNullOrEmpty(tile))
		{
			return Vector2.zero;
		}
		string[] array = tile.Split(',');
		if (array.Length != 2)
		{
			return Vector2.zero;
		}
		return new Vector2(array[0].ToInt(), array[1].ToInt());
	}

	private static Vector3 TileToClientPosition(string tile)
	{
		Vector2 vector = StringToTile(tile);
		if (vector != Vector2.zero)
		{
			return Durango.Terrain.Util.TilePositionToClientPosition(vector, tileCenter: true);
		}
		return Vector3.zero;
	}
}
