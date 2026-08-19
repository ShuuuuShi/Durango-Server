using System;
using System.Collections.Generic;
using Estate;
using ExploreData;
using InteractionData;
using ItemSystem;
using K1Network;
using L10N;
using Messages;
using Player;
using Shared.Economy;
using Shared.Estate;
using Shared.Region;
using Shared.System;
using TerrainData;
using TimerData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class InteractionGroup : UIBase
{
	[SerializeField]
	private InteractionMenuControl _interactionMenu;

	private RenderTexture _renderTarget;

	private Texture2D _pixelPicker;

	private InteractionObject _selectedObject;

	private Func<GameObject, bool> _filterFunc;

	private List<GameObject> _bufferList = new List<GameObject>();

	public InteractionMenuControl InteractionMenu => _interactionMenu;

	private RenderTexture RenderTarget
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_0021: Expected O, but got Unknown
			RenderTexture result;
			if ((Object)(object)_renderTarget == (Object)null)
			{
				RenderTexture val = new RenderTexture(1, 1, 0);
				RenderTexture val2 = val;
				_renderTarget = val;
				result = val2;
			}
			else
			{
				result = _renderTarget;
			}
			return result;
		}
	}

	private Texture2D PixelPicker
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_0020: Expected O, but got Unknown
			Texture2D result;
			if ((Object)(object)_pixelPicker == (Object)null)
			{
				Texture2D val = new Texture2D(1, 1);
				Texture2D val2 = val;
				_pixelPicker = val;
				result = val2;
			}
			else
			{
				result = _pixelPicker;
			}
			return result;
		}
	}

	private void Start()
	{
		AddInteractionHandler();
		GameSystem<InteractionSystem>.Instance().RegisterContextActionFinder(ContextActionFinder);
		InteractionMenuControl interactionMenu = _interactionMenu;
		interactionMenu.OnClickInteractionMenu = (Action<InteractionMenuData>)Delegate.Combine(interactionMenu.OnClickInteractionMenu, new Action<InteractionMenuData>(OnClickInteractionMenu));
		InteractionMenuControl interactionMenu2 = _interactionMenu;
		interactionMenu2.OnGatheringQueueClick = (Action<string>)Delegate.Combine(interactionMenu2.OnGatheringQueueClick, (Action<string>)delegate(string id)
		{
			GameSystem<GatheringSystem>.Instance().RemoveGatheringQueue(id);
		});
		PlayerBehavior.LocalPlayer.SurvivalGaugeInitialized += delegate(CharacterBehavior character)
		{
			if (!character.IsAlive)
			{
				ShowPlayerDeadInteractionMenu();
			}
		};
		GameSystem<InteractionSystem>.Instance().InteractionTargetSelected += OnSelectInteractionTarget;
		KSingleton<PlayerController>.Instance().MoveStarted += OnStartMove;
		KSingleton<PlayerController>.Instance().OnPickObject += OnPickObject;
		GameSystem<TimerSystem>.Instance().StartSubjectProgress += OnStartSubjectProgress;
	}

	private void OnEnable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onClick, new UICamera.VoidDelegate(OnClickScreen));
	}

	private void OnDisable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Remove(UICamera.onClick, new UICamera.VoidDelegate(OnClickScreen));
	}

	private void OnClickScreen(GameObject obj)
	{
		if (!PlayerBehavior.LocalPlayer.IsAlive && !NGUITools.IsChild(((Component)KSingleton<UIManager>.Instance().UIRoot).transform, obj.transform))
		{
			if (GameSystem<InteractionSystem>.Instance().Target == null)
			{
				ShowPlayerDeadInteractionMenu();
			}
			else
			{
				GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
			}
		}
	}

	private void OnStartSubjectProgress(string subject)
	{
		if (!(subject == "collect"))
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		}
	}

	private void OnStartMove()
	{
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
	}

	private void OnSelectInteractionTarget(InteractionObject obj)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		MarkingTarget(obj);
		if (obj == null)
		{
			_interactionMenu.Hide();
			if ((Object)(object)PlayerBehavior.LocalPlayer != (Object)null && KSingleton<CameraController>.HasInstance())
			{
				KSingleton<CameraController>.Instance().ResetCameraTarget();
			}
			return;
		}
		_interactionMenu.Show();
		InteractionObject.Type objectType = obj.ObjectType;
		if (objectType == InteractionObject.Type.Animal || objectType == InteractionObject.Type.Prop)
		{
			KSingleton<CameraController>.Instance().SetCameraTargetPos(obj.Position);
		}
	}

	private void MarkingCancel()
	{
		if (_selectedObject != null && (Object)(object)_selectedObject.Target != (Object)null && _selectedObject.Target.activeSelf)
		{
			_selectedObject.Target.SendMessage("OnSelected", (object)false, (SendMessageOptions)1);
		}
	}

	private void MarkingTarget(InteractionObject obj)
	{
		MarkingCancel();
		_selectedObject = obj;
		if (_selectedObject != null && (Object)(object)_selectedObject.Target != (Object)null && _selectedObject.Target.activeSelf)
		{
			_selectedObject.Target.SendMessage("OnSelected", (object)true, (SendMessageOptions)1);
		}
	}

	public void ShowPlayerDeadInteractionMenu()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Clear();
		if (!GameSystem<StatisticsSystem>.Instance().IsNewbie)
		{
			menuList.Add(new InteractionMenuData(Shared.System.Interaction.SetReviveReward));
		}
		if (UIManager.FindScript<WorldMapGroup>().HasOneOrMoreWarpHoles())
		{
			menuList.Add(new InteractionMenuData(Shared.System.Interaction.ReviveAtWarphole));
		}
		menuList.Add(new InteractionMenuData(Shared.System.Interaction.Revive));
		menuList.Name = string.Empty;
		GameSystem<InteractionSystem>.Instance().SetSelfMenuList(menuList);
	}

	private void OnClickInteractionMenu(InteractionMenuData menu)
	{
		GameSystem<InteractionSystem>.Instance().SelectInteractionMenu(menu);
		if (!InteractionMenuData.IsKeepInteractionMenuAction(menu))
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		}
	}

	public void SetInteractionFilter(Func<GameObject, bool> filter)
	{
		_filterFunc = filter;
	}

	private GameObject GetInteractionObject(GameObject o)
	{
		int layer = o.layer;
		GameObject result = null;
		if (layer == LayerHelper.DefaultLayer)
		{
			result = InteractionSystem.MovableInteractionObjectFilter(o);
		}
		else if (layer == LayerHelper.PropLayer)
		{
			result = InteractionSystem.PropInteractionObjectFilter(o);
		}
		return result;
	}

	private void OnPickObject(Ray ray, PlayerController.TouchEvent touch, ref bool result)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Visible || PlayerBehavior.LocalPlayer.IsMoving || PlayerBehavior.LocalPlayer.IsCombatMode || touch.IsTouchBegan)
		{
			return;
		}
		Vector2 val = touch.CurrentPos - touch.BeginPos;
		float sqrMagnitude = ((Vector2)(ref val)).sqrMagnitude;
		float dragThreshold = KSingleton<PlayerController>.Instance().DragThreshold;
		if (sqrMagnitude > dragThreshold * dragThreshold)
		{
			return;
		}
		GameObject val2 = null;
		float num = float.MaxValue;
		int count;
		RaycastHit[] array = KCollisionUtility.RayCast(ray, float.PositiveInfinity, LayerMask.op_Implicit(LayerHelper.InteractionMask), out count);
		GameObject val3 = ((_selectedObject != null) ? _selectedObject.Target : null);
		bool flag = false;
		float num2 = float.MaxValue;
		Vector2 val12 = default(Vector2);
		for (int i = 0; i < count; i++)
		{
			RaycastHit val4 = array[i];
			Transform val5 = ((!((Object)(object)((RaycastHit)(ref val4)).collider == (Object)null)) ? ((Component)((RaycastHit)(ref val4)).collider).transform : ((RaycastHit)(ref val4)).transform);
			GameObject interactionObject = GetInteractionObject(((Component)val5).gameObject);
			if ((Object)(object)interactionObject == (Object)null)
			{
				continue;
			}
			tk2dSprite component = interactionObject.GetComponent<tk2dSprite>();
			if ((Object)(object)component != (Object)null)
			{
				tk2dSpriteCollectionData collection = component.Collection;
				tk2dSpriteDefinition currentSpriteDef = component.GetCurrentSpriteDef();
				Collider collider = ((RaycastHit)(ref val4)).collider;
				BoxCollider val6 = (BoxCollider)(object)((collider is BoxCollider) ? collider : null);
				if ((Object)(object)val6 == (Object)null)
				{
					continue;
				}
				Vector2 val7 = Vector2.op_Implicit(interactionObject.transform.InverseTransformPoint(((RaycastHit)(ref val4)).point));
				Vector2 val8 = Vector2.op_Implicit(val6.center - val6.size * 0.5f);
				val7 -= val8;
				val7.x /= val6.size.x;
				val7.y /= val6.size.y;
				Vector2 val9 = (currentSpriteDef.uvs[1] - currentSpriteDef.uvs[0]) * val7.x;
				Vector2 val10 = (currentSpriteDef.uvs[2] - currentSpriteDef.uvs[0]) * val7.y;
				val7 = currentSpriteDef.uvs[0] + val9 + val10;
				Material val11 = collection.materials[currentSpriteDef.materialId];
				val12.x = 1f / (float)val11.mainTexture.width;
				val12.y = 1f / (float)val11.mainTexture.height;
				RenderTexture renderTarget = RenderTarget;
				RenderTexture active = RenderTexture.active;
				RenderTexture.active = renderTarget;
				renderTarget.MarkRestoreExpected();
				GL.Clear(true, true, Color.clear);
				val11.SetPass(0);
				DrawQuads(new Rect(val7, val12), new Rect(0f, 0f, 1f, 1f));
				Texture2D pixelPicker = PixelPicker;
				pixelPicker.ReadPixels(new Rect(0f, 0f, 1f, 1f), 0, 0);
				pixelPicker.Apply();
				RenderTexture.active = active;
				if (Color32.op_Implicit(pixelPicker.GetPixel(0, 0)).a == 0)
				{
					continue;
				}
				if ((Object)(object)interactionObject == (Object)(object)val3)
				{
					flag = true;
					continue;
				}
				if (!(component.color.a < 1f))
				{
					if (!(((RaycastHit)(ref val4)).distance < num2))
					{
						continue;
					}
					num2 = ((RaycastHit)(ref val4)).distance;
				}
			}
			else if ((Object)(object)interactionObject == (Object)(object)val3)
			{
				flag = true;
				continue;
			}
			if (_filterFunc != null && !_filterFunc(interactionObject))
			{
				continue;
			}
			float distance = InteractionObject.GetDistance(interactionObject);
			if (!(distance > 2000f))
			{
				Vector3 interactionPosition = KUtility.GetInteractionPosition(interactionObject, ignoreY: false);
				Vector3 val13 = MainCamera.WorldToScreenPos(interactionPosition);
				val13.z = 0f;
				Vector3 val14 = Vector2.op_Implicit(touch.CurrentPos);
				Vector3 val15 = val13 - val14;
				float sqrMagnitude2 = ((Vector3)(ref val15)).sqrMagnitude;
				if (!(num < distance))
				{
					val2 = interactionObject;
					num = sqrMagnitude2;
				}
			}
		}
		if ((Object)(object)val2 != (Object)null)
		{
			InteractionObject interactionTarget = new InteractionObject(val2);
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(interactionTarget);
		}
		else if (flag || _selectedObject != null)
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		}
		result = true;
	}

	private void DrawQuads(Rect uv, Rect vert)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Begin(7);
		GL.TexCoord(new Vector3(((Rect)(ref uv)).x, ((Rect)(ref uv)).y, 0f));
		GL.Vertex(new Vector3(((Rect)(ref vert)).x, ((Rect)(ref vert)).y, 0f));
		GL.TexCoord(new Vector3(((Rect)(ref uv)).xMax, ((Rect)(ref uv)).y, 0f));
		GL.Vertex(new Vector3(((Rect)(ref vert)).xMax, ((Rect)(ref vert)).y, 0f));
		GL.TexCoord(new Vector3(((Rect)(ref uv)).xMax, ((Rect)(ref uv)).yMax, 0f));
		GL.Vertex(new Vector3(((Rect)(ref vert)).xMax, ((Rect)(ref vert)).yMax, 0f));
		GL.TexCoord(new Vector3(((Rect)(ref uv)).x, ((Rect)(ref uv)).yMax, 0f));
		GL.Vertex(new Vector3(((Rect)(ref vert)).x, ((Rect)(ref vert)).yMax, 0f));
		GL.End();
		GL.PopMatrix();
	}

	private void FindInteractionableObjects(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		float num = 2000f;
		InteractionSystem.GetNearObjectsInternal(pos, _bufferList, LayerMask.op_Implicit(LayerHelper.InteractionMask), num, GetInteractionObject);
		for (int i = 0; i < _bufferList.Count; i++)
		{
			Vector3 position = _bufferList[i].transform.position;
			Vector3 val = position - pos;
			float magnitude = ((Vector3)(ref val)).magnitude;
			_bufferList[i].SendMessage("OnGlitter", (object)(magnitude / num * 1.5f), (SendMessageOptions)1);
		}
	}

	private void AddInteractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Plant, delegate(InteractionObject target)
		{
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			PopupItemSelector popupItemSelector = UIManager.Popup.Tooltip<PopupItemSelector>();
			popupItemSelector.Set(callbackItem: delegate(ItemData item)
			{
				if (item != null)
				{
					BuildSystem.FarmingAction("plant", target.GetTargetComponent<Artifact>(), item, delegate(Messages.Timer msg, PacketHeader header)
					{
						IconProgressGauge iconProgressGauge2 = TimerData.Timer.Play<IconProgressGauge>(new TimerData.Timer("plant", msg.Duration));
						iconProgressGauge2.SetIcon(item.Icon);
						KSingleton<PlayerController>.Instance().Motion(string.Format("Farm.{0}", "plant"), msg.Duration);
					});
				}
			}, filterFunc: (ItemData data) => data.HasTag("plantable"));
			popupItemSelector.SetTitle(Shared.System.Interaction.Plant.GetName());
			popupItemSelector.AutoPosition = false;
			popupItemSelector.Show(3600f);
			UIWidget rootAnchor3 = UIManager.GetRootAnchor(AnchorType.Base);
			Vector3 position3 = rootAnchor3.GetPosition(0f, 0.5f);
			position3.x += 10f;
			popupItemSelector.Widget.SetPosition(position3, 0f, 0.5f);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Fertilize, delegate(InteractionObject target)
		{
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			Artifact artifact3 = target.GetTargetComponent<Artifact>();
			if (artifact3.ArtifactState.Farming.HasValue)
			{
				Farming value2 = artifact3.ArtifactState.Farming.Value;
				int requiredFertilizer = value2.RequiredFertilizer - value2.Fertilizer;
				PopupItemSelector selector2 = UIManager.Popup.Tooltip<PopupItemSelector>();
				selector2.Set(callbackList: delegate(IList<ItemData> items)
				{
					BuildSystem.FertilizePlant(artifact3, items);
				}, callbackItemsChanged: delegate(IList<ItemData> items)
				{
					float num3 = 0f;
					for (int i = 0; i < items.Count; i++)
					{
						PerformanceData performanceData = items[i].GetPerformanceData("fertilizer");
						if (performanceData != null && performanceData.num_attrs.TryGetValue("fertilizer", out var value3))
						{
							num3 += value3;
						}
					}
					string arg = string.Format((!(num3 > (float)requiredFertilizer)) ? "{0}" : "<alert>{0}</alert>", (int)num3);
					selector2.SetTitle($"{Shared.System.Interaction.Fertilize.GetName()}  {arg}/{requiredFertilizer}");
				}, filterFunc: (ItemData data) => data.HasTag("fertilizer"), selectableCount: 10);
				selector2.SetTitle($"{Shared.System.Interaction.Fertilize.GetName()}  0/{requiredFertilizer}");
				selector2.AutoPosition = false;
				selector2.Show(3600f);
				UIWidget rootAnchor2 = UIManager.GetRootAnchor(AnchorType.Base);
				Vector3 position2 = rootAnchor2.GetPosition(0f, 0.5f);
				position2.x += 10f;
				selector2.Widget.SetPosition(position2, 0f, 0.5f);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Watering, delegate(InteractionObject target)
		{
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			Artifact artifact2 = target.GetTargetComponent<Artifact>();
			if (artifact2.ArtifactState.Farming.HasValue)
			{
				Farming value = artifact2.ArtifactState.Farming.Value;
				int requireWater = value.Water.Value - value.Water.Key;
				PopupItemSelector selector = UIManager.Popup.Tooltip<PopupItemSelector>();
				selector.Set(callbackList: delegate(IList<ItemData> items)
				{
					BuildSystem.WaterPlant(artifact2, items);
				}, callbackItemsChanged: delegate(IList<ItemData> items)
				{
					selector.SetTitle($"{Shared.System.Interaction.Watering.GetName()}  {items.Count}/{requireWater}");
				}, filterFunc: (ItemData data) => data.HasTag("water"), selectableCount: requireWater);
				selector.SetTitle($"{Shared.System.Interaction.Watering.GetName()}  0/{requireWater}");
				selector.AutoPosition = false;
				selector.Show(3600f);
				UIWidget rootAnchor = UIManager.GetRootAnchor(AnchorType.Base);
				Vector3 position = rootAnchor.GetPosition(0f, 0.5f);
				position.x += 10f;
				selector.Widget.SetPosition(position, 0f, 0.5f);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.GrowRapidly, delegate(InteractionObject target)
		{
			Artifact artifact = target.GetTargetComponent<Artifact>();
			if (!((Object)(object)artifact == (Object)null) && artifact.ArtifactState.Farming.HasValue)
			{
				Gauge rapidGrowthCost = artifact.ArtifactState.Farming.Value.RapidGrowthCost;
				if (rapidGrowthCost != null)
				{
					int amount = (int)rapidGrowthCost.Get();
					string text2 = T._("빠른 성장에는 {0:가} 필요합니다.\n현재 보유량 {1}\n<alert>성공률에는 영향을 주지 않습니다.</alert>");
					GameSystem<InventorySystem>.Instance().PlayerInventory.ShowPayConfirm(amount, Currency.Gem, text2, delegate(bool action)
					{
						if (action)
						{
							BuildSystem.GrowRapidly(artifact);
						}
					});
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Uproot, delegate(InteractionObject target)
		{
			BuildSystem.FarmingAction("uproot", target.GetTargetComponent<Artifact>(), (ItemData)null, (Connection.MessageHandler<Messages.Timer>)delegate(Messages.Timer msg, PacketHeader header)
			{
				IconProgressGauge iconProgressGauge = TimerData.Timer.Play<IconProgressGauge>(new TimerData.Timer("uproot", msg.Duration));
				iconProgressGauge.SetIcon("tool_bare_hands");
				KSingleton<PlayerController>.Instance().Motion(string.Format("Farm.{0}", "uproot"), msg.Duration);
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.CloseGate, delegate(InteractionObject target)
		{
			BuildSystem.ArtifactAction("close_gate", target.GetTargetComponent<Artifact>());
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.OpenGate, delegate(InteractionObject target)
		{
			BuildSystem.ArtifactAction("open_gate", target.GetTargetComponent<Artifact>());
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Rest, delegate(InteractionObject target)
		{
			BuildSystem.Rest(target.GetTargetComponent<Artifact>());
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Revive, delegate
		{
			string text = ((!GameSystem<StatisticsSystem>.Instance().IsNewbie) ? T.N_("집으로 <em>귀환</em>해 부활하시겠습니까?\n죽은 곳에 아이템 일부가 떨어집니다.") : T.N_("<em>귀환</em> 하시겠습니까?"));
			UIManager.MessageBox.Show(T._(text), delegate(bool ok)
			{
				if (ok)
				{
					KSingleton<PlayerController>.Instance().ResurrectionRequest();
				}
			});
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.ReviveAtWarphole, delegate
		{
			UIManager.FindScript<WorldMapGroup>().OpenForRevive();
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Take, delegate(InteractionObject target)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			ulong id = InteractionSystem.CurrentMenu.Id;
			InventorySystem.TakeOutItems(target.EntityId, new Point2(target.Tile), id);
			InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
			if (menuList.Count > 1)
			{
				((MonoBehaviour)GameSystem<InteractionSystem>.Instance()).CancelInvoke("SendTouchMsg");
				((MonoBehaviour)GameSystem<InteractionSystem>.Instance()).Invoke("SendTouchMsg", 0.1f);
			}
			else
			{
				menuList.Reset();
				menuList.Apply();
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.GetProfile, delegate(InteractionObject target)
		{
			ulong num2 = 0uL;
			PlayerBehavior targetComponent2 = target.GetTargetComponent<PlayerBehavior>();
			if (!((Object)(object)targetComponent2 == (Object)null))
			{
				num2 = targetComponent2.EntityId;
			}
			if (num2 != 0L)
			{
				KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(num2, delegate(Player.PlayerInfo info)
				{
					if (info.Valid)
					{
						ProfileTooltip profileTooltip = UIManager.Popup.Tooltip<ProfileTooltip>();
						profileTooltip.Set(info);
						profileTooltip.Show(3600f);
					}
				});
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Whisper, delegate(InteractionObject target)
		{
			ulong num = 0uL;
			PlayerBehavior targetComponent = target.GetTargetComponent<PlayerBehavior>();
			if (!((Object)(object)targetComponent == (Object)null))
			{
				num = targetComponent.EntityId;
			}
			if (num != 0L)
			{
				ChattingGroup chattingGroup = UIManager.FindScript<ChattingGroup>();
				chattingGroup.Open(num);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.SearchWarphole, Action_search_warphole);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.WashBody, Action_wash_body);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.DrawWater, Action_draw_water);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.DrinkWater, Action_drink_water);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.InteractionArtifact, Action_interaction_artifact);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.DeclareWar, Action_declare_war);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.ManagerEstateLicense, Action_manager_estate_license);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.ExtendEstateUnit, Action_extend_estate_unit);
	}

	private static void Action_search_warphole(InteractionObject target)
	{
		Connections.Frontend.Send(default(SearchWarpholes)).On(delegate(SearchedWarpholes msg, PacketHeader _)
		{
			UIManager.FindScript<ContextActionGroup>().RefreshSearchWarpholeCooltime(msg.SearchedAt);
			if (!PlayerBehavior.LocalPlayer.IsRiding)
			{
				KSingleton<PlayerController>.Instance().Motion("Warp_Find", 5f, 1f, forceTransition: true);
			}
			KSingleton<DetectWarpHoleUI>.Instance().ShowScanner(msg.Results);
		});
	}

	private static void Action_wash_body(InteractionObject target)
	{
		Connections.Frontend.Send(default(WashBody)).On(delegate(Messages.Timer msg, PacketHeader _)
		{
			TimerSystem.SetGaugeAndPlayMotion(msg.Duration, IconMap.Get(InteractionData.Interaction.WashBody), "Water_Wash");
			DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Wash, T._("강물"));
		});
	}

	private static void Action_draw_water(InteractionObject target)
	{
		List<ItemData> list = GameSystem<InventorySystem>.Instance().FilteringByTag("container");
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		for (int i = 0; i < list.Count; i++)
		{
			ItemData itemData = list[i];
			InteractionMenuData data = new InteractionMenuData(Shared.System.Interaction.DrawWater);
			data.Id = itemData.Id;
			data.Name = itemData.Name;
			data.Icon = itemData.Icon;
			menuList.Add(data);
		}
		if (menuList.Count > 0)
		{
			GameSystem<InteractionSystem>.Instance().SetSelfMenuList(menuList);
		}
		else
		{
			UIManager.SystemMsg(T._("담을 수 있는 용기가 없습니다"));
		}
	}

	private static void Action_drink_water(InteractionObject target)
	{
		Connections.Frontend.Send(default(DrinkWater)).On(delegate(Messages.Timer msg, PacketHeader _)
		{
			TimerSystem.SetGaugeAndPlayMotion(msg.Duration, IconMap.Get(InteractionData.Interaction.DrinkWater), "Barehand_DrinkRiver");
			DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Drink, T._("강물"));
		});
	}

	private void Action_interaction_artifact(InteractionObject target)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		TileObject tileObject = TerrainA6.GetTileObject(new Point2(TerrainA6.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition)), warning: false);
		if (tileObject != null && !((Object)(object)tileObject.Artifact == (Object)null))
		{
			InteractionObject interactionTarget = new InteractionObject(((Component)tileObject.Artifact).gameObject);
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(interactionTarget);
		}
	}

	private static void Action_declare_war(InteractionObject target)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		GameSystem<InteractionSystem>.Instance().DeclareWar(new Point2(TerrainA6.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition)));
	}

	private static void Action_manager_estate_license(InteractionObject target)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		EstateManagerGroup estateManagerGroup = UIManager.FindScript<EstateManagerGroup>();
		if (!((Object)(object)estateManagerGroup == (Object)null))
		{
			TileObject tileObject = TerrainA6.GetTileObject(new Point2(TerrainA6.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition)));
			if (tileObject != null)
			{
				estateManagerGroup.OpenPermissionManager(tileObject.EstateId);
			}
		}
	}

	private static void Action_extend_estate_unit(InteractionObject target)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		EstateManagerGroup estateManagerGroup = UIManager.FindScript<EstateManagerGroup>();
		if (!((Object)(object)estateManagerGroup == (Object)null))
		{
			TileObject tileObject = TerrainA6.GetTileObject(new Point2(TerrainA6.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition)));
			if (tileObject != null)
			{
				estateManagerGroup.OpenEstateExtendUI(tileObject.EstateId);
			}
		}
	}

	private static void ContextActionFinder(ref List<InteractionData.Interaction> result)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		TerrainData.Biome biome = localPlayer.GetBiome();
		TerrainWater.WaterDepthLevel waterDepthLevel = localPlayer.WaterDepthLevel;
		if (localPlayer.Floor == 0 && waterDepthLevel <= TerrainWater.WaterDepthLevel.Waist)
		{
			if (TerrainA6.IsWater(biome))
			{
				ExploreData.Region region = KSingleton<GameManager>.Instance().Region;
				if (region != null && region.Role() != Role.Tutorial)
				{
					result.Add(InteractionData.Interaction.SearchWarphole);
					GameSystem<InteractionSystem>.Instance().UpdateSearchWarpholeCooltime();
				}
				result.Add(InteractionData.Interaction.WashBody);
				result.Add(InteractionData.Interaction.DrawWater);
			}
			if (TerrainA6.IsDrinkable(biome))
			{
				result.Add(InteractionData.Interaction.DrinkWater);
			}
		}
		TileObject tileObject = TerrainA6.GetTileObject(new Point2(TerrainA6.ClientPositionToTilePosition(localPlayer.CurrentPosition)), warning: false);
		if (tileObject != null)
		{
			if ((Object)(object)tileObject.Artifact != (Object)null && tileObject.Artifact.InteractionDisabled)
			{
				result.Add(InteractionData.Interaction.InteractionArtifact);
			}
			if (tileObject.EstateId != 0L)
			{
				Estate.EstateInfo estateInfo = GameSystem<EstateSystem>.Instance().GetEstateInfo(tileObject.EstateId);
				if (estateInfo != null && estateInfo.IsValid())
				{
					if (estateInfo.Owner == localPlayer.EntityId)
					{
						result.Add(InteractionData.Interaction.ManagerEstateLicense);
						result.Add(InteractionData.Interaction.ExtendEstateUnit);
					}
					else if ((estateInfo.OwnerType == OwnerType.ClanCapture || estateInfo.OwnerType == OwnerType.ClanEstate) && localPlayer.Clan.ClanId != 0L)
					{
						Member clan = localPlayer.Clan;
						if (clan.ClanId == estateInfo.Owner)
						{
							if (clan.RoleId == 0)
							{
								result.Add(InteractionData.Interaction.ManagerEstateLicense);
							}
						}
						else if (!estateInfo.OnWar() && Singleton<Constants>.Instance.war.enable)
						{
							result.Add(InteractionData.Interaction.DeclareWar);
						}
					}
				}
			}
		}
		Driver driver = localPlayer.Driver;
		if ((Object)(object)driver.Vehicle != (Object)null)
		{
			result.Add((!driver.IsRiding) ? InteractionData.Interaction.Mount : InteractionData.Interaction.Unmount);
		}
	}
}
