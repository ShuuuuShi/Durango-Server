using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using APNGLib;
using Building;
using Durango.Logic.Clusters;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.MotionInfo;
using Durango.Network;
using Durango.Player;
using Durango.Player.Animation;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.InGame;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using Shared.MessageBoard;
using Shared.Teleport;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Interactions;

public class ArtifactInteractions
{
	public const string PlayerAttachmentName = "Attachment_Player";

	private PredictTimer _sprinkleTimer;

	private PredictTimer _fertilizePlantTimer;

	private PredictTimer _waterPlantTimer;

	public void Init()
	{
		_sprinkleTimer = new PredictTimer(GameManager.PlayerId, Interaction.Sprinkle.ToString());
		GameSystem<InteractionSystem>.Instance().MenuList.RegisterTimer(Interaction.Sprinkle, _sprinkleTimer, null);
		_sprinkleTimer.Ended += delegate
		{
			Durango.Utils.Singleton<GridAreaViewer>.Instance().Hide();
		};
		AddInteractionHandler();
		_fertilizePlantTimer = new PredictTimer(GameManager.PlayerId, "FertilizePlant");
		_fertilizePlantTimer.Started += delegate(PredictTimer timer)
		{
			Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer.Timer).AddIcon(IconMap.Get(Interaction.Fertilize));
			timer.SetMotion("Farming_Gather");
		};
		_waterPlantTimer = new PredictTimer(GameManager.PlayerId, "WaterPlant");
		_waterPlantTimer.Started += delegate(PredictTimer timer)
		{
			Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer.Timer).AddIcon(IconMap.Get(Interaction.Watering));
			timer.SetMotion("Farming_Water");
		};
	}

	private void AddInteractionHandler()
	{
		InteractionSystem interactionSystem = GameSystem<InteractionSystem>.Instance();
		interactionSystem.AddInteractionHandler(Interaction.BuildArtifact, Register(BuildArtifact));
		interactionSystem.AddInteractionHandler(Interaction.CompleteArtifact, Register(CompleteArtifact));
		interactionSystem.AddInteractionHandler(Interaction.DestructArtifact, Register(DestructArtifact));
		interactionSystem.AddInteractionHandler(Interaction.RemodelArtifact, Register(RemodelArtifact));
		interactionSystem.AddInteractionHandler(Interaction.ScribbleText, Register(ScribbleText));
		interactionSystem.AddInteractionHandler(Interaction.ScribbleDrawing, Register(ScribbleDrawing));
		interactionSystem.AddInteractionHandler(Interaction.NameArtifact, Register(NameArtifact));
		interactionSystem.AddInteractionHandler(Interaction.RenameArtifact, Register(RenameArtifact));
		interactionSystem.AddInteractionHandler(Interaction.Fire, Register(Fire));
		interactionSystem.AddInteractionHandler(Interaction.Extinguish, Register(Extinguish));
		interactionSystem.AddInteractionHandler(Interaction.SkipPostprocess, Register(SkipPostprocess));
		interactionSystem.AddInteractionHandler(Interaction.Capsulate, Register(Capsulate));
		interactionSystem.AddInteractionHandler(Interaction.RepairArtifactImmediately, Register(RepairArtifactImmediately));
		interactionSystem.AddInteractionHandler(Interaction.Rest, Register(Rest));
		interactionSystem.AddInteractionHandler(Interaction.Wash, Register(Wash));
		interactionSystem.AddInteractionHandler(Interaction.Sprinkle, Register(Sprinkle));
		interactionSystem.AddInteractionHandler(Interaction.OpenTechSupport, Register(OpenTechSupport));
		interactionSystem.AddInteractionHandler(Interaction.ManagePioneerGrade, Register(ManagePioneerGrade));
		interactionSystem.AddInteractionHandler(Interaction.Plant, Register(Plant));
		interactionSystem.AddInteractionHandler(Interaction.Uproot, Register(Uproot));
		interactionSystem.AddInteractionHandler(Interaction.Fertilize, Register(Fertilize));
		interactionSystem.AddInteractionHandler(Interaction.Watering, Register(Watering));
		interactionSystem.AddInteractionHandler(Interaction.GrowRapidly, Register(GrowRapidly));
		interactionSystem.AddInteractionHandler(Interaction.Invest, Register(Invest));
		interactionSystem.AddInteractionHandler(Interaction.ClanResearch, Register(ClanResearch));
		interactionSystem.AddInteractionHandler(Interaction.Accelerate, Register(Accelerate));
		interactionSystem.AddInteractionHandler(Interaction.ParticipateAcceleration, Register(ParticipateAcceleration));
		interactionSystem.AddInteractionHandler(Interaction.ReceiveAccelerationRewards, Register(ReceiveAccelerationRewards));
		interactionSystem.AddInteractionHandler(Interaction.ExtendFloor, Register(ArtifactExtendFloor));
		interactionSystem.AddInteractionHandler(Interaction.ExtendFloorWithRoof, Register(delegate(Artifact artifact)
		{
			ArtifactExtendFloor(artifact, withRoof: true);
		}));
		interactionSystem.AddInteractionHandler(Interaction.ExtendFloorWithoutRoof, Register(delegate(Artifact artifact)
		{
			ArtifactExtendFloor(artifact, withRoof: false);
		}));
		interactionSystem.AddInteractionHandler(Interaction.ToUpstair, delegate
		{
			ChangeFloor(1);
		});
		interactionSystem.AddInteractionHandler(Interaction.ToDownstair, delegate
		{
			ChangeFloor(-1);
		});
		interactionSystem.AddInteractionHandler(Interaction.ActivatePersonalRegionWarphole, Register(delegate(Artifact artifact)
		{
			Connections.Frontend.Send(new ActivePersonalRegionWarphole
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
		}));
		interactionSystem.AddInteractionHandler(Interaction.WarpToPersonalRegion, Register(delegate(Artifact artifact)
		{
			GameSystem<MapSystem>.Instance().TryWarp(() => Connections.Frontend.Send(new WarpToPersonalRegion
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			}), MapSystem.WarpTo.PersonalEstate);
		}));
		interactionSystem.AddInteractionHandler(Interaction.WarpToUrbanRegion, Register(delegate(Artifact artifact)
		{
			GameSystem<MapSystem>.Instance().TryWarp(() => Connections.Frontend.Send(new WarpToUrbanRegion
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			}), MapSystem.WarpTo.UrbanEstate);
		}));
		interactionSystem.AddInteractionHandler(Interaction.ChargeEffect, Register(ChargeEffect));
		interactionSystem.AddInteractionHandler(Interaction.TakeEffect, Register(delegate(Artifact artifact)
		{
			Connections.Frontend.Send(new TakeEffect
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
		}));
		interactionSystem.AddInteractionHandler(Interaction.SetAsHome, delegate(InteractionObject o)
		{
			SetAsHome(o.EntityId, new Point2(o.Tile));
		});
		interactionSystem.AddInteractionHandler(Interaction.CloseGate, Register(delegate(Artifact artifact)
		{
			Connections.Frontend.Send(new CloseGate
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
		}));
		interactionSystem.AddInteractionHandler(Interaction.OpenGate, Register(delegate(Artifact artifact)
		{
			Connections.Frontend.Send(new OpenGate
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
		}));
		interactionSystem.AddInteractionHandler(Interaction.SendReport, Register(SendReport));
		interactionSystem.AddInteractionHandler(Interaction.TurnOnMusic, Register(delegate(Artifact artifact)
		{
			Connections.Frontend.Send(new TurnOnMusic
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
			SoundManager.PlayEvent("ui_jukebox_turnon");
		}));
		interactionSystem.AddInteractionHandler(Interaction.TurnOffMusic, Register(delegate(Artifact artifact)
		{
			Connections.Frontend.Send(new TurnOffMusic
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
			SoundManager.PlayEvent("ui_jukebox_turnoff");
		}));
		interactionSystem.AddInteractionHandler(Interaction.ChangeMannequinHead, Register(delegate(Artifact artifact)
		{
			ChangeMannequin(artifact, isBody: false);
		}));
		interactionSystem.AddInteractionHandler(Interaction.ChangeMannequinBody, Register(delegate(Artifact artifact)
		{
			ChangeMannequin(artifact, isBody: true);
		}));
		interactionSystem.AddInteractionHandler(Interaction.TakeOffMannequinHead, Register(delegate(Artifact artifact)
		{
			TakeOffMannequin(artifact, isBody: false);
		}));
		interactionSystem.AddInteractionHandler(Interaction.TakeOffMannequinBody, Register(delegate(Artifact artifact)
		{
			TakeOffMannequin(artifact, isBody: true);
		}));
		interactionSystem.AddInteractionHandler(Interaction.ChangeDecoration, Register(delegate(Artifact artifact)
		{
			Connections.Frontend.Send(new Messages.Display
			{
				EntityId = artifact.EntityId
			});
		}));
	}

	private static InteractionSystem.InteractionHandler Register(Action<Artifact> func)
	{
		return delegate(InteractionObject obj)
		{
			Artifact targetComponent = obj.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				func(targetComponent);
			}
		};
	}

	private static void BuildArtifact([NotNull] Artifact artifact)
	{
		GameSystem<BuildSystem>.Instance().InteractionBuildArtifact(artifact);
	}

	private static void CompleteArtifact([NotNull] Artifact artifact)
	{
		Connections.Frontend.Send(new CompleteArtifact
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		});
	}

	private static void DestructArtifact([NotNull] Artifact artifact)
	{
		if (artifact.IsAvailableInterior())
		{
			string mainText = ((!artifact.HasInterior()) ? T._("<em>{0}</em>{0:-을} 제거하시겠습니까?", artifact.LocalizedName) : T._("{0:을} 제거하면 <em>안에 있는 물건들도 파괴됩니다.</em> 정말 제거하시겠습니까?", artifact.LocalizedName));
			UIManager.MessageBox.Show(mainText, delegate(bool ok)
			{
				if (ok)
				{
					SendDestructArtifact(artifact);
				}
			}, T._("네"), T._("아니오"));
		}
		else if (artifact.ArtifactState.DomesticCage.HasValue && KUtility.GetSize(artifact.ArtifactState.DomesticCage.Value.Reins) > 0)
		{
			UIManager.MessageBox.Show(T._("{0:을} 파괴하시겠습니까?", artifact.LocalizedName), T._("<alert>[icon=icon_make_alert] {0} 안의 동물들도 같이 사라집니다. 정말 제거하시겠습니까?</alert>", artifact.LocalizedName), delegate(bool ok)
			{
				if (ok)
				{
					SendDestructArtifact(artifact);
				}
			}, T._("네"), T._("아니오"));
		}
		else
		{
			SendDestructArtifact(artifact);
		}
	}

	private static void RemodelArtifact([NotNull] Artifact artifact)
	{
		if (GameManager.ClusterMode == Mode.Online)
		{
			Building.Blueprint blueprint = artifact.Blueprint;
			if (blueprint != null && blueprint.Available)
			{
				if (artifact.GetArtifactComponent<ModularArtifact>() != null)
				{
					UIManager.Popup.Tooltip<SelectRemodelingPartPopup>().Show(artifact);
				}
			}
			else
			{
				GameSystem<SkillSystem>.Instance().SkillNeeded(blueprint?.GetOwnerSkill());
			}
		}
		else
		{
			Building.Blueprint blueprint2 = artifact.Blueprint;
			if (blueprint2 != null && blueprint2.Available)
			{
				UIManager.Popup.Tooltip<SelectModularPartTexturePopup>().Show(artifact);
			}
		}
	}

	private static void SendDestructArtifact([NotNull] Artifact artifact)
	{
		if (GameManager.ClusterMode == Mode.Online)
		{
			DestructArtifact destructArtifact = default(DestructArtifact);
			destructArtifact.EntityId = artifact.EntityId;
			destructArtifact.Tile = artifact.WorldTile;
			DestructArtifact msg = destructArtifact;
			Connections.Frontend.Send(msg).On(delegate(Destructing destructing, PacketHeader _)
			{
				OnDestructedReplied(artifact, destructing);
			});
			return;
		}
		Artifact[] interiors = artifact.GetInteriors();
		if (interiors != null)
		{
			HashSet<string> hashSet = new HashSet<string>();
			Artifact[] array = interiors;
			foreach (Artifact artifact2 in array)
			{
				if (!(artifact2 == null))
				{
					hashSet.Add(artifact2.EntityId);
				}
			}
			foreach (string item in hashSet)
			{
				Connections.Frontend.Send(new DestructArtifact
				{
					EntityId = item
				});
			}
		}
		DestructArtifact destructArtifact2 = default(DestructArtifact);
		destructArtifact2.EntityId = artifact.EntityId;
		destructArtifact2.Tile = artifact.WorldTile;
		DestructArtifact msg2 = destructArtifact2;
		Connections.Frontend.Send(msg2);
	}

	private static void SkipPostprocess([NotNull] Artifact artifact)
	{
		int cost = GetSkipPostprocessCost(artifact);
		if (cost < 0)
		{
			return;
		}
		UIManager.MessageBox.ShowPayConfirm(cost, Currency.Gem, T._("즉시 완료하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				Connections.Frontend.Send(new SkipPostprocess
				{
					EntityId = artifact.EntityId,
					Tile = artifact.WorldTile,
					Cost = cost
				});
			}
		});
	}

	private static int GetSkipPostprocessCost([NotNull] Artifact artifact)
	{
		if (Yaml.Util.Singleton<CashYaml>.Instance == null)
		{
			return -1;
		}
		Durango.Logic.Timer.Timer timer = ((artifact.PostProcessTimer != null) ? artifact.PostProcessTimer : null);
		if (timer == null || timer.IsStop)
		{
			return -1;
		}
		float remain = timer.Remain;
		float num = 0f;
		float num2 = 1f;
		float num3 = 0f;
		float num4 = 1f;
		for (int i = 0; i < Yaml.Util.Singleton<CashYaml>.Instance.instant_construction.Length; i++)
		{
			int[] obj = Yaml.Util.Singleton<CashYaml>.Instance.instant_construction[i];
			num = num3;
			num2 = num4;
			num3 = obj[0];
			num4 = obj[1];
			if (remain < num3)
			{
				break;
			}
		}
		float num5 = (remain - num) / (num3 - num);
		return (int)(num2 + (num4 - num2) * num5);
	}

	private static void Capsulate([NotNull] Artifact artifact)
	{
		Connections.Frontend.Send(new GetCapsulatingCost
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		}).On(delegate(Messages.Cost msg, PacketHeader _)
		{
			if (!(artifact == null))
			{
				UIManager.MessageBox.ShowPayConfirm(msg.Amount, msg.Currency, T._("<em>{0}</em>{0:-을} 포장하시겠습니까?", artifact.LocalizedName), delegate(bool ok)
				{
					if (ok)
					{
						DoCapsulateArtifact(artifact);
					}
				});
			}
		});
	}

	private static void DoCapsulateArtifact([NotNull] Artifact artifact)
	{
		Connections.Frontend.Send(new CapsulateArtifact
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		}).On(delegate(Messages.Timer timerMsg, PacketHeader header)
		{
			MotionMap.Instance().GetBuildMotion("Capsulate", null, out var motion, out var equip);
			TimerSystem.SetGaugeAndPlayMotion(timerMsg.Duration, artifact.Blueprint.Icon, motion, "Capsulate", equip);
		});
	}

	private static void OnDestructedReplied([CanBeNull] Artifact artifact, Destructing msg)
	{
		IconProgressGauge iconProgressGauge = Durango.Logic.Timer.Timer.Play<IconProgressGauge>(new Durango.Logic.Timer.Timer("destruct", msg.Duration));
		string icon = IconMap.Get(Interaction.DestructArtifact);
		Color color = InteractionMenuData.InteractionMenuColor(Interaction.DestructArtifact);
		iconProgressGauge.AddIcon(icon, color);
		byte toolType = msg.ToolType;
		LocalMotionUpdater motionUpdater = PlayerController.MotionUpdater;
		motionUpdater.Motion(toolType switch
		{
			1 => "Onehand_Destroy", 
			2 => "Twohand_Destroy", 
			_ => "Barehand_Destroy", 
		}, iconProgressGauge.RemainTime());
	}

	public static Action SetInteractionMotion([NotNull] Artifact artifact, Interaction interaction, bool overrideIdleMotion = false, bool attachedMotionOnly = false)
	{
		Vector3 prePosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		GameObject gameObject = artifact.gameObject;
		Transform transform = FindAvailableAttachment(interaction, gameObject);
		if (transform != null)
		{
			SnapToAttachment(transform);
		}
		else if (attachedMotionOnly)
		{
			UIManager.SystemMsg(T._("인원이 초과되어 사용할 수 없습니다."));
			return null;
		}
		string attachmentType = GetAttachmentType(transform);
		string interactionMotion = MotionMap.Instance().GetInteractionMotion(interaction, artifact.BlueprintId, attachmentType);
		LocalMotionUpdater motionUpdater = PlayerController.MotionUpdater;
		string motion = interactionMotion;
		bool overrideIdleMotion2 = overrideIdleMotion;
		motionUpdater.Motion(motion, 0f, 1f, forceTransition: false, overrideIdleMotion2);
		byte floor = (byte)artifact.Floor.GetValueOrDefault();
		return delegate
		{
			Durango.Utils.Singleton<PlayerController>.Instance().Teleport(prePosition, floor, TeleportType.Unknown, instance: true);
			PlayerController.MotionUpdater.RefreshMotion(null, force: false, clearReservations: true);
		};
	}

	private static void Rest([NotNull] Artifact artifact)
	{
		SetInteractionMotion(artifact, Interaction.Rest, overrideIdleMotion: true);
		Connections.Frontend.Send(new RestOn
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		});
	}

	private static void Wash([NotNull] Artifact artifact)
	{
		Action onCancelAction = SetInteractionMotion(artifact, Interaction.Wash, overrideIdleMotion: true, attachedMotionOnly: true);
		Connections.Frontend.Send(new Wash
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		}).On(delegate(Messages.Timer timerMsg, PacketHeader header)
		{
			TimerSystem.SetGaugeAndPlayMotion(timerMsg.Duration, IconMap.Get(Interaction.WashBody), null, "WashBody");
		}).Rest(delegate(Packet packet)
		{
			if (!Packet.IsSuccess(packet) && onCancelAction != null)
			{
				onCancelAction();
			}
		});
	}

	public static Transform FindAvailableAttachment(Interaction action, GameObject target)
	{
		Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>();
		componentsInChildren.Shuffle();
		string text = ((action != Interaction.GetStatusEffect) ? action.ToString() : "Use");
		Transform result = null;
		string value = "Attachment_Player_" + text;
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			string name = transform.name;
			Vector3 position = transform.position;
			switch (action)
			{
			case Interaction.Rest:
			case Interaction.GetStatusEffect:
				if (name.StartsWith(value) && !HasPlayerDoingAction(position, (string clip) => clip.ContainsIgnoreCase("sit") || clip.ContainsIgnoreCase("sleep") || clip.ContainsIgnoreCase("furniture")))
				{
					return transform;
				}
				break;
			case Interaction.Wash:
				if (name.StartsWith(value) && !HasPlayerDoingAction(position, (string clip) => clip.ContainsIgnoreCase("wash")))
				{
					return transform;
				}
				break;
			}
			if (name == "Attachment_Player")
			{
				result = transform;
			}
		}
		return result;
	}

	private static bool HasPlayerDoingAction(Vector3 pos, Func<string, bool> isActionClip)
	{
		return Durango.Utils.Singleton<PlayerManager>.Instance().GetPlayer(delegate(PlayerBehavior target)
		{
			if ((pos - target.MeshObjectTransform.position).sqrMagnitude > 100f)
			{
				return false;
			}
			PlayerAnimationClipInfo currentPlayerClipInfo = target.CurrentPlayerClipInfo;
			return currentPlayerClipInfo != null && currentPlayerClipInfo.Clip != null && isActionClip != null && isActionClip(currentPlayerClipInfo.Clip);
		}) != null;
	}

	private static string GetAttachmentType(Transform attachment)
	{
		if (attachment == null)
		{
			return null;
		}
		string[] array = attachment.name.Split(new char[1] { '_' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return string.Empty;
		}
		string text = array[array.Length - 1];
		if (int.TryParse(text, out var _))
		{
			return text;
		}
		return string.Empty;
	}

	public static void SnapToAttachment(Transform attachment)
	{
		Durango.Utils.Singleton<PlayerController>.Instance().SnapToTarget(attachment);
	}

	private static void Fire([NotNull] Artifact artifact)
	{
		Connections.Frontend.Send(new FireBurnable
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		});
	}

	private static void Extinguish([NotNull] Artifact artifact)
	{
		Connections.Frontend.Send(new ExtinguishBurnable
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		});
	}

	private static void ScribbleText([NotNull] Artifact messageBoard)
	{
		if (!messageBoard.BuildCompleted)
		{
			return;
		}
		TextInputPopup textInput = UIManager.Popup.Tooltip<TextInputPopup>();
		textInput.Show(delegate(string message)
		{
			UIManager.MessageBox.Show(T._("완성하시겠습니까?"), delegate(bool ok)
			{
				if (!ok)
				{
					textInput.Show();
				}
				else
				{
					SendScribble(messageBoard, Drawing.Text, Encoding.UTF8.GetBytes(message));
				}
			});
		}, T._("표지판에 남길 글을 적어주세요."), null, isMultiline: true);
	}

	private static void ScribbleDrawing([NotNull] Artifact messageBoard)
	{
		if (!messageBoard.BuildCompleted)
		{
			return;
		}
		DrawPixelGroup canvasUI = UIManager.FindScript<DrawPixelGroup>();
		Scribblable scribblable = messageBoard.Blueprint.Scribblable;
		if (scribblable == null || !scribblable.Canvas)
		{
			return;
		}
		Point2 canvasSize = scribblable.CanvasSize;
		int limitFrame = scribblable.LimitFrame;
		int num = messageBoard.GetTag("color_scribble")?.Level ?? 1;
		canvasUI.Set(canvasSize.x, canvasSize.y, messageBoard.EntityId, limitFrame, $"color_board_L{num:00}.raw", T._("{0:lv:} {1}", messageBoard.ArtifactState.Level, messageBoard.LocalizedName), T._("정말 그리기를 종료하시겠습니까?\n완성하지 않은 그림은 임시 저장됩니다."), delegate(List<Texture2D> list, Action<bool> confirmed)
		{
			UIManager.MessageBox.Show(T._("완성하시겠습니까?"), delegate(bool ok)
			{
				if (confirmed != null)
				{
					confirmed(ok);
				}
				if (ok)
				{
					byte[] array;
					if (list.Count == 1)
					{
						array = list[0].EncodeToPNG();
					}
					else
					{
						Stream stream = APNGAssembler.ToPNG(list, 1f).ToStream();
						array = new byte[stream.Length];
						stream.Read(array, 0, array.Length);
					}
					SendScribble(messageBoard, Drawing.Canvas, array);
					canvasUI.Close();
				}
			});
		});
		string text = T._("그림은 1회만 완성할 수 있습니다.");
		canvasUI.SetWarningText("<alert>[icon=icon_make_alert]" + text + "</alert>");
	}

	private static void SendScribble([CanBeNull] Artifact artifact, Drawing type, byte[] data)
	{
		if (!(artifact == null))
		{
			Connections.Frontend.Send(new Scribble
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile,
				Type = type,
				Data = data
			});
		}
	}

	private static void NameArtifact([NotNull] Artifact artifact)
	{
		if (artifact.BuildCompleted)
		{
			UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string message)
			{
				Connections.Frontend.Send(new Rename
				{
					EntityId = artifact.EntityId,
					Tile = artifact.WorldTile,
					Name = message,
					PrevName = null,
					IsFirstRename = true
				});
			}, T._("이름을 입력해주세요."), null, isMultiline: true);
		}
	}

	private static void RenameArtifact([NotNull] Artifact artifact)
	{
		if (!artifact.BuildCompleted)
		{
			return;
		}
		Yaml.Cost cost = Yaml.Util.Singleton<CostsYaml>.Instance.ArtifactRename;
		string comment = T._("<em>{0}</em>의 이름을 바꿉니다.", artifact.LocalizedName);
		string previousName = artifact.ArtifactState.ChangedName;
		UIManager.MessageBox.ShowCostConfirm(cost, comment, null, delegate(bool ok)
		{
			if (ok)
			{
				Wallet wallet = InventorySystem.Wallet;
				cost.Payable(wallet, out var byVoucher, out var byCurrency);
				if (byVoucher || byCurrency)
				{
					UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string message)
					{
						Connections.Frontend.Send(new Rename
						{
							EntityId = artifact.EntityId,
							Tile = artifact.WorldTile,
							Name = message,
							PrevName = previousName,
							IsFirstRename = false
						});
					}, T._("바꿀 이름을 입력해주세요."), null, isMultiline: true);
				}
				else
				{
					UIManager.SystemMsg(T._("{0:이} 필요합니다.", cost.CostToString(wallet)));
				}
			}
		});
	}

	private static void RepairArtifactImmediately([NotNull] Artifact artifact)
	{
		if (!artifact.BuildCompleted)
		{
			return;
		}
		if (artifact.ArtifactState.IsRepairing())
		{
			Gauge repairImmediateCost = artifact.ArtifactState.RepairImmediateCost;
			if (repairImmediateCost == null)
			{
				return;
			}
			int currentCost = (int)repairImmediateCost.Get();
			UIManager.MessageBox.ShowPayConfirm(currentCost, Currency.Gem, T._("수리를 즉시 완료하시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					RepairSystem.RepairImmediate(artifact.EntityId, artifact.WorldTile, new Messages.Cost
					{
						Currency = Currency.Gem,
						Amount = currentCost
					});
				}
			});
			return;
		}
		MessageBox messageBox = UIManager.MessageBox;
		int repairRequirementPerformance = Yaml.Util.Singleton<Constants>.Instance.Repair.GetRepairRequirementPerformance(artifact.Blueprint.RepairRequirement, artifact.ArtifactState.Level);
		messageBox.ShowPayConfirm(repairRequirementPerformance, Currency.Gem, T._("워프젬으로 즉시 수리하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				RepairSystem.RepairArtifact(artifact.EntityId, artifact.WorldTile, null, null);
			}
		});
	}

	private static void Plant([NotNull] Artifact farm)
	{
		PopupItemSelector popupItemSelector = UIManager.Popup.Tooltip<PopupItemSelector>();
		popupItemSelector.Filter((ItemData data) => data.HasTag("plantable")).OnConfirmed(delegate(ItemData item)
		{
			if (item != null)
			{
				UIManager.MessageBox.ShowLockConfirm(item, delegate
				{
					Connections.Frontend.Send(new PlantSeed
					{
						EntityId = farm.EntityId,
						Tile = farm.WorldTile,
						SeedItemId = item.Id
					}).On(delegate(Messages.Timer msg, PacketHeader header)
					{
						Durango.Logic.Timer.Timer.Play<IconProgressGauge>(new Durango.Logic.Timer.Timer("plant", msg.Duration)).AddIcon(item.Icon);
						PlayerController.MotionUpdater.Motion("Farming_Gather", msg.Duration);
					});
				});
			}
		}).Title(Interaction.Plant.GetName());
		popupItemSelector.Show(3600f);
	}

	private void Sprinkle([NotNull] Artifact sprinkler)
	{
		_sprinkleTimer.Play(10f);
		Transform transform = FindAvailableAttachment(Interaction.Sprinkle, sprinkler.gameObject);
		if (transform == null)
		{
			UIManager.SystemMsg(T._("인원이 초과되어 사용할 수 없습니다."));
			return;
		}
		SnapToAttachment(transform);
		Connections.Frontend.Send(new Sprinkle
		{
			EntityId = sprinkler.EntityId,
			Tile = sprinkler.WorldTile
		}).On(delegate(SprinkledInfo msg, PacketHeader header)
		{
			SimpleTileArea simpleTileArea = new SimpleTileArea
			{
				Tile = sprinkler.WorldTile,
				Offsets = sprinkler.Blueprint.GetEffectTiles(sprinkler.Rotation),
				TileColorFunc = delegate(Point2 tile, out Color color)
				{
					if (msg.FertilizedTiles != null && msg.FertilizedTiles.Any((Point2 o) => o == tile))
					{
						color = Sprinklable.FertilizingTileColor;
					}
					else
					{
						color = Sprinklable.WateringTileColor;
					}
					return true;
				}
			};
			Durango.Utils.Singleton<GridAreaViewer>.Instance().Show(new GridAreaBase[1] { simpleTileArea }, GridAreaViewer.LayerType.Upper, tweenAlpha: true);
		}).On(delegate(Messages.Timer msg, PacketHeader header)
		{
			_sprinkleTimer.Play(msg.Duration);
			PlayerController.MotionUpdater.Motion("Farming_sprinkler", msg.Duration);
		})
			.On<OK>(delegate
			{
				_sprinkleTimer.Stop();
			})
			.Rest(delegate
			{
				_sprinkleTimer.Stop();
			});
	}

	private static void OpenTechSupport([NotNull] Artifact artifact)
	{
		if (InteractionSystem.CurrentMenu.Disabled)
		{
			TechSupportEstimatePageWidget.ShowShutdownWarningMsg();
		}
		else
		{
			UIManager.FindScript<TechSupportGroup>().Open(artifact);
		}
	}

	private void ManagePioneerGrade([NotNull] Artifact artifact)
	{
		UIManager.FindScript<PioneerGradeManageGroup>().Open(artifact);
	}

	private static void Uproot([NotNull] Artifact farm)
	{
		Connections.Frontend.Send(new UprootPlant
		{
			EntityId = farm.EntityId,
			Tile = farm.WorldTile
		}).On(delegate(Messages.Timer msg, PacketHeader header)
		{
			Durango.Logic.Timer.Timer.Play<IconProgressGauge>(new Durango.Logic.Timer.Timer("uproot", msg.Duration)).AddIcon("tool_bare_hands");
			PlayerController.MotionUpdater.Motion("Farming_Gather", msg.Duration);
		});
	}

	private void Fertilize([NotNull] Artifact artifact)
	{
		if (!artifact.ArtifactState.Farming.HasValue)
		{
			return;
		}
		Farming value = artifact.ArtifactState.Farming.Value;
		float requiredFertilizer = (float)value.RequiredFertilizer - value.FertilizerAmount;
		string arg = ((!string.IsNullOrEmpty(value.AppliedCropBooster)) ? TagData.GetTagNameWithLevel(value.AppliedCropBooster, value.BoosterLevel) : T._("없음"));
		PopupItemSelector selector = UIManager.Popup.Tooltip<PopupItemSelector>();
		selector.Filter((ItemData data) => data.HasTag("fertilizer")).SelectableCount(-1).OnConfirmed(delegate(IList<ItemData> items)
		{
			SendFertilizePlant(artifact, items);
		})
			.OnChanged(delegate(IList<ItemData> items)
			{
				float num = 0f;
				for (int i = 0; i < items.Count; i++)
				{
					Performance? performanceData = items[i].GetPerformanceData("fertilizer");
					if (performanceData.HasValue && performanceData.Value.Nums != null && performanceData.Value.Nums.TryGetValue("fertilizer", out var value2))
					{
						num += value2;
					}
				}
				string fertilizerText = string.Format((!(num > requiredFertilizer)) ? "{0}" : "<alert>{0}</alert>", (int)num);
				selector.Title($"{Interaction.Fertilize.GetName()}  {fertilizerText}/{requiredFertilizer:F1}").HelpText(T._("퇴비 효과"));
				selector.AttachLoadingRingToHelperLabel();
				Connections.Frontend.Send(new GetExpectedCropBooster
				{
					EntityId = artifact.EntityId,
					Tile = artifact.WorldTile,
					ItemIds = Durango.Logic.Item.Util.ItemsToIds(items)
				}).On(delegate(ExpectedCropBooster msg, PacketHeader header)
				{
					string arg2 = ((!string.IsNullOrEmpty(msg.CropBooster)) ? TagData.GetTagNameWithLevel(msg.CropBooster, msg.BoosterLevel) : T._("없음"));
					selector.Title($"{Interaction.Fertilize.GetName()}  {fertilizerText}/{requiredFertilizer}").HelpText(string.Format("{0} <em>{1}</em>", T._("퇴비 효과"), arg2));
					selector.DetachLoadingRingFromHelperLabel();
				}).Rest(delegate
				{
					selector.DetachLoadingRingFromHelperLabel();
				});
			})
			.Title($"{Interaction.Fertilize.GetName()}  0/{requiredFertilizer}")
			.HelpText(string.Format("{0} <em>{1}</em>", T._("퇴비 효과"), arg));
		selector.Show(3600f);
	}

	private void SendFertilizePlant([NotNull] Artifact farm, IList<ItemData> items)
	{
		if (KUtility.GetSize(items) == 0)
		{
			return;
		}
		UIManager.MessageBox.ShowLockConfirm(items, delegate(string[] itemIds)
		{
			FertilizePlant fertilizePlant = default(FertilizePlant);
			fertilizePlant.EntityId = farm.EntityId;
			fertilizePlant.Tile = farm.WorldTile;
			fertilizePlant.ItemIds = itemIds;
			FertilizePlant msg = fertilizePlant;
			_fertilizePlantTimer.Play(Connections.Frontend.Ping + 10f);
			bool confirming = false;
			Connections.Frontend.Send(msg).On(delegate(Messages.Timer timerMsg, PacketHeader header)
			{
				_fertilizePlantTimer.Play(timerMsg.Duration);
			}).On(delegate(EnergyWarning warningMsg, PacketHeader header)
			{
				_fertilizePlantTimer.Pause();
				confirming = true;
				LowEnergyWarning.Show(warningMsg, header, delegate(LowEnergyWarning.Result result)
				{
					if (result == LowEnergyWarning.Result.IgnoreWarning)
					{
						_fertilizePlantTimer.Play(Connections.Frontend.Ping + 10f);
					}
					else
					{
						_fertilizePlantTimer.Stop();
					}
					confirming = false;
				});
			})
				.Rest(delegate
				{
					if (confirming)
					{
						LowEnergyWarning.Hide();
						confirming = false;
					}
					_fertilizePlantTimer.Stop();
				});
		});
	}

	private void Watering([NotNull] Artifact artifact)
	{
		if (artifact.ArtifactState.Farming.HasValue)
		{
			Farming value = artifact.ArtifactState.Farming.Value;
			float requireWater = value.Water.y - value.Water.x;
			PopupItemSelector selector = UIManager.Popup.Tooltip<PopupItemSelector>();
			selector.Filter((ItemData data) => data.HasTag("water")).SelectableCount(Mathf.CeilToInt(requireWater)).OnConfirmed(delegate(IList<ItemData> items)
			{
				SendWaterPlant(artifact, items);
			})
				.OnChanged(delegate(IList<ItemData> items)
				{
					selector.Title($"{Interaction.Watering.GetName()}  {items.Count}/{requireWater}");
				})
				.Title($"{Interaction.Watering.GetName()}  0/{requireWater}");
			selector.Show(3600f);
		}
	}

	private void SendWaterPlant([NotNull] Artifact farm, IList<ItemData> items)
	{
		if (KUtility.GetSize(items) == 0)
		{
			return;
		}
		UIManager.MessageBox.ShowLockConfirm(items, delegate(string[] itemIds)
		{
			WaterPlant waterPlant = default(WaterPlant);
			waterPlant.EntityId = farm.EntityId;
			waterPlant.Tile = farm.WorldTile;
			waterPlant.ItemIds = itemIds;
			WaterPlant msg = waterPlant;
			_waterPlantTimer.Play(Connections.Frontend.Ping + 10f);
			bool confirming = false;
			Connections.Frontend.Send(msg).On(delegate(Messages.Timer timerMsg, PacketHeader header)
			{
				_waterPlantTimer.Play(timerMsg.Duration);
			}).On(delegate(EnergyWarning warningMsg, PacketHeader header)
			{
				_waterPlantTimer.Pause();
				confirming = true;
				LowEnergyWarning.Show(warningMsg, header, delegate(LowEnergyWarning.Result result)
				{
					if (result == LowEnergyWarning.Result.IgnoreWarning)
					{
						_waterPlantTimer.Play(Connections.Frontend.Ping + 10f);
					}
					else
					{
						_waterPlantTimer.Stop();
					}
					confirming = false;
				});
			})
				.Rest(delegate
				{
					if (confirming)
					{
						LowEnergyWarning.Hide();
						confirming = false;
					}
					_waterPlantTimer.Stop();
				});
		});
	}

	private static void GrowRapidly([NotNull] Artifact artifact)
	{
		if (!artifact.ArtifactState.Farming.HasValue)
		{
			return;
		}
		Gauge rapidGrowthCost = artifact.ArtifactState.Farming.Value.RapidGrowthCost;
		if (rapidGrowthCost == null)
		{
			return;
		}
		long cost = (long)rapidGrowthCost.Get();
		string comment = T._("<em>{0}</em>{0:-을} 즉시 성장시키시겠습니까?\n<alert>성공률에는 영향을 주지 않습니다.</alert>", artifact.ArtifactState.Farming.Value.PlantName);
		UIManager.MessageBox.ShowPayConfirm(cost, Currency.Gem, comment, delegate(bool ok)
		{
			if (ok)
			{
				SendGrowRapidly(artifact);
			}
		});
	}

	private static void SendGrowRapidly([NotNull] Artifact farm)
	{
		Vector3 center = farm.Center;
		Connections.Frontend.Send(new GrowRapidly
		{
			EntityId = farm.EntityId,
			Tile = farm.WorldTile
		}).On<OK>(delegate
		{
			BuildSystem.PlayBuildFinished(center);
		});
	}

	private static void Invest([NotNull] Artifact artifact)
	{
		Messages.Crack? crack = artifact.ArtifactState.Crack;
		if (!crack.HasValue)
		{
			return;
		}
		string text = string.Empty;
		int size = KUtility.GetSize(crack.Value.PotentialBiocoms);
		if (size > 1)
		{
			text = T._("{0:l:<em>{}</em>|, } 중 1", crack.Value.PotentialBiocoms.ToList());
		}
		else if (size == 1)
		{
			text = T._("<em>{0}</em>", crack.Value.PotentialBiocoms[0]);
		}
		MessageBox messageBox = UIManager.MessageBox;
		messageBox.AddKeyValueInfo(T._("예상 워프 자원"), text);
		messageBox.ShowPayConfirmWithVoucher(crack.Value.RequiredInvestment, Yaml.Util.Singleton<Constants>.Instance.Crack.VoucherId, T._("크레이터에 [icon={0}]<em>자원유도석</em>을 넣으면 자연물이 워프됩니다.", Yaml.Util.Singleton<Constants>.Instance.Crack.VoucherId), T._("[icon={0}]<em>자원유도석</em>은 <ref>ui://Quest, 퀘스트</ref> 또는 <ref>ui://Event, 출석부</ref>를 통해 획득 가능합니다."), delegate(bool ok)
		{
			if (ok)
			{
				int currency = crack.Value.RequiredInvestment;
				if (currency > 0)
				{
					long num = InventorySystem.Wallet.GetVoucherCount(Yaml.Util.Singleton<Constants>.Instance.Crack.VoucherId);
					if (currency > num)
					{
						UIManager.SystemMsg(T._("자원유도석이 부족합니다."));
					}
					else
					{
						Durango.Utils.Singleton<PlayerController>.Instance().MoveToTarget(artifact.gameObject, delegate
						{
							Connections.Frontend.Send(new InvestToCrack
							{
								EntityId = artifact.EntityId,
								Tile = artifact.WorldTile,
								Amount = currency
							}).On(delegate(Messages.Timer msg, PacketHeader _)
							{
								TimerSystem.SetGaugeAndPlayMotion(msg.Duration, IconMap.Get(Interaction.Invest), "Craft_Sit", "Craft");
							}).On<OK>(delegate
							{
								UIManager.Alarm.ShowNotify(T._("크레이터가 열렸습니다. 잠시 후 자연물이 워프되어 옵니다."), "act_warphole", major: true);
							});
						});
					}
				}
			}
		}, T._("넣기"));
	}

	private static void ClanResearch([NotNull] Artifact artifact)
	{
		InteractionMenuData currentMenu = InteractionSystem.CurrentMenu;
		string id = currentMenu.Id;
		string entityId = artifact.EntityId;
		GameSystem<ResearchSystem>.Instance().GetClanResearchList(delegate(ClanResearchList list)
		{
			bool flag = false;
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			int i = 0;
			for (int size = KUtility.GetSize(list.ResearchList); i < size; i++)
			{
				Messages.ClanResearch clanResearch = list.ResearchList[i];
				if (list.ResearchList[i].LabEntityId == entityId && predictedServerTime < clanResearch.Until)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				UIManager.SystemMsg(T._("이미 연구가 진행 중입니다."));
			}
			else
			{
				UIManager.Popup.Tooltip<ClanResearchPopup>().Show(artifact.EntityId, artifact.WorldTile, id);
			}
		}, ignoreCache: false);
	}

	private static void ChargeEffect([NotNull] Artifact artifact)
	{
		PopupItemSelector popupItemSelector = UIManager.Popup.Tooltip<PopupItemSelector>();
		popupItemSelector.Filter((ItemData data) => data.HasTag("incense_store")).SelectableCount(1).OnConfirmed(delegate(ItemData item)
		{
			if (item != null)
			{
				Connections.Frontend.Send(new ChargeEffect
				{
					EntityId = artifact.EntityId,
					Tile = artifact.WorldTile,
					ItemId = item.Id
				});
			}
		})
			.Title(T._("향 태우기"));
		popupItemSelector.Show(3600f);
	}

	private static void SetAsHome(string entityId, Point2 tile)
	{
		SetAsHome setAsHome = default(SetAsHome);
		setAsHome.EntityId = entityId;
		setAsHome.Tile = tile;
		SetAsHome msg = setAsHome;
		Connections.Frontend.Send(msg).On<OK>(delegate
		{
			UIManager.SystemMsg(T._("이제 지도 보기에서 '귀환 지점으로'를 누르면 이곳으로 돌아옵니다."));
		});
	}

	private static void SendReport([NotNull] Artifact artifact)
	{
		bool scribbles = false;
		bool clanWarehouse = false;
		string entityId;
		if (artifact.ArtifactState.Scribble.HasValue)
		{
			scribbles = true;
			entityId = artifact.ArtifactState.Scribble.Value.Scribbler;
		}
		else if (artifact.name == "clan_warehouse")
		{
			clanWarehouse = true;
			entityId = artifact.FounderId;
		}
		else
		{
			if (artifact.ArtifactState.ChangedName == null)
			{
				return;
			}
			entityId = artifact.FounderId;
		}
		Durango.Utils.Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(Durango.Player.PlayerInfo info)
		{
			if (info.Valid)
			{
				SendReportPopup sendReportPopup = UIManager.Popup.Tooltip<SendReportPopup>();
				if (scribbles)
				{
					sendReportPopup.SetForScribbles(info.Name, artifact);
				}
				else
				{
					sendReportPopup.SetForArtifactName(info.Name, artifact, clanWarehouse);
				}
				sendReportPopup.Show();
			}
			else
			{
				UIManager.SystemMsg(T._("건물에서 플레이어의 정보를 가져오는데 실패하였습니다."));
			}
		});
	}

	private static void DoWarpAccelerate(Messages.Cost cost, [NotNull] Action action)
	{
		MessageBox messageBox = UIManager.MessageBox;
		Pair<int, int> warpMatterAcquisition = GameSystem<WarpAcceleratorSystem>.Instance().GetWarpMatterAcquisition();
		messageBox.SetCurrencyInfo(Currency.WarpMatter);
		messageBox.SetLowerText(T._("워프 가속 1 단계에서 실패하면 보상을 받을 수 없습니다."));
		messageBox.AddKeyValueInfo(string.Format("<weak>{0}</weak>", T._("이번 주 획득 가능")), string.Format("<weak>{0}</weak>", T._("<em>{0:N0}</em>/{1:N0} 개 남음", warpMatterAcquisition.Item1, warpMatterAcquisition.Item2)));
		string text = T._("각 단계 성공 시 [c][preset=warp_matter_particle:1.5][/c] <em>워프 물질</em> 획득 가능");
		if (GameSystem<WarpAcceleratorSystem>.Instance().GetMyWarpAcceleratorInfo().HasValue)
		{
			text += string.Format("\n<alert><alert_icon/> {0}</alert>", T._("다른 워프 가속 활동에 참여하면 진행중인 이전 활동 기록은 사라집니다."));
		}
		messageBox.ShowPayConfirm(cost.Amount, cost.Currency, T._("<em>워프 가속기</em> 주변에 모여드는 동물들을 처치하세요."), text, delegate(bool ok)
		{
			if (ok)
			{
				action();
			}
		}, T._("참가"));
	}

	private static void DoWarpAccelerate([NotNull] Action action)
	{
		Connections.Frontend.Send(default(GetWarpAcceleratorCost)).On(delegate(Messages.Cost cost, PacketHeader header)
		{
			DoWarpAccelerate(cost, action);
		});
	}

	private static void ChangeMannequin([NotNull] Artifact artifact, bool isBody)
	{
		PopupItemSelector popupItemSelector = UIManager.Popup.Tooltip<PopupItemSelector>();
		popupItemSelector.Filter((ItemData item) => !item.IsEquipments && (isBody ? (item.HasAttribute("slot", "body") || item.HasAttribute("slot", "hoody")) : item.HasAttribute("slot", "head"))).OnConfirmed(delegate(ItemData item)
		{
			if (item != null)
			{
				UIManager.MessageBox.ShowLockConfirm(item, delegate
				{
					TakeOffMannequin(artifact, isBody, delegate(bool result)
					{
						if (result)
						{
							Connections.Frontend.Send(new ChangeMannequinDisplay
							{
								EntityId = artifact.EntityId,
								Tile = artifact.WorldTile,
								Slot = ((!isBody) ? "head" : "body"),
								ItemId = item.Id
							}).All(delegate(Packet packet)
							{
								if (Packet.IsSuccess(packet))
								{
									BuildSystem.PlayBuildFinished(artifact.Center);
								}
							});
						}
					});
				});
			}
		}).Title((!isBody) ? T._("모자 고르기") : T._("옷 고르기"));
		popupItemSelector.Show(3600f);
	}

	private static void TakeOffMannequin([NotNull] Artifact artifact, bool isBody, Action<bool> onResult = null)
	{
		Messages.Item? touchedMannequinItem = GetTouchedMannequinItem(artifact, isBody);
		if (!touchedMannequinItem.HasValue)
		{
			onResult?.Invoke(obj: true);
			return;
		}
		InventorySystem.TakeOutItems(artifact.EntityId, artifact.WorldTile, new string[1] { touchedMannequinItem.Value.Id }, onResult);
	}

	private static Messages.Item? GetTouchedMannequinItem(Artifact artifact, bool isBody)
	{
		Touched lastTouched = GameSystem<InteractionSystem>.Instance().LastTouched;
		if (!(lastTouched.EntityId != artifact.EntityId))
		{
			Messages.Mannequin? mannequin = lastTouched.Mannequin;
			if (mannequin.HasValue)
			{
				if (isBody)
				{
					return lastTouched.Mannequin.Value.Body;
				}
				return lastTouched.Mannequin.Value.Head;
			}
		}
		return null;
	}

	private static void Accelerate([NotNull] Artifact artifact)
	{
		DoWarpAccelerate(delegate
		{
			Accelerate accelerate = default(Accelerate);
			accelerate.EntityId = artifact.EntityId;
			accelerate.Tile = artifact.WorldTile;
			Accelerate msg = accelerate;
			Connections.Frontend.Send(msg);
		});
	}

	private static void ParticipateAcceleration([NotNull] Artifact artifact)
	{
		DoWarpAccelerate(delegate
		{
			Connections.Frontend.Send(new ParticipateAcceleration
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
		});
	}

	private static void ReceiveAccelerationRewards([NotNull] Artifact artifact)
	{
		Connections.Frontend.Send(new ReceiveAcceleratorRewards
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		});
	}

	private static void ArtifactExtendFloor([NotNull] Artifact artifact)
	{
		if (Yaml.Util.Singleton<Constants>.Instance.ArtifactFloor.MaxStories <= artifact.Stories.Value.GetValueOrDefault())
		{
			UIManager.SystemMsg(T._("더 이상 증축할 수 없습니다."));
			return;
		}
		UIManager.FindScript<InteractionGroup>().InteractionMenu.ShowSubmenus(Interaction.ExtendFloor, Interaction.ExtendFloorWithRoof, Interaction.ExtendFloorWithoutRoof);
	}

	private static void ArtifactExtendFloor([NotNull] Artifact artifact, bool withRoof)
	{
		Yaml.Cost artifactExtendFloor = Yaml.Util.Singleton<CostsYaml>.Instance.ArtifactExtendFloor;
		artifactExtendFloor.SetAmountParams(new KeyValuePair<string, object>("current_stories", artifact.Stories.Value.GetValueOrDefault()));
		long cost = ((GameManager.ClusterMode != 0) ? 0 : artifactExtendFloor.GetAmount());
		Currency currency = artifactExtendFloor.Currency;
		UIManager.MessageBox.ShowPayConfirm(cost, currency, T._("증축하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				Connections.Frontend.Send(new ExtendFloor
				{
					EntityId = artifact.EntityId,
					Tile = artifact.WorldTile,
					WithRoof = withRoof
				});
			}
		});
	}

	private static void ChangeFloor(int offset)
	{
		TileObject tileObject = PlayerBehavior.LocalPlayer.GetTileObject();
		if (tileObject == null || tileObject.Artifact == null)
		{
			return;
		}
		Artifact artifact = tileObject.Artifact;
		if (artifact.Stories == null)
		{
			return;
		}
		int num = PlayerBehavior.LocalPlayer.Floor.Value + offset;
		if (num >= 0)
		{
			int? value = artifact.Stories.Value;
			if (!value.HasValue || num < value.GetValueOrDefault())
			{
				Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
				currentPosition.y = (float)(num * 200) + LocalMoveOperator.GetWorldHeight(currentPosition, (byte)num, 0f).GetValueOrDefault();
				PlayerController.MoveOperator.Teleport(currentPosition, (byte)num);
			}
		}
	}
}
