using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Durango.Cutscene;
using Durango.Logic;
using Durango.Network;
using Durango.Prologue;
using Durango.Render.Effect;
using Durango.Render.Screen;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using EasyConsole;
using Homans.Console;
using Messages;
using Shared.Battle;
using Shared.Quest;
using Shared.System;
using Shared.Teleport;
using UnityEngine;

namespace Durango.Development;

public class Commands : Singleton<Commands>
{
	public class ClientCheatDispatcher
	{
		public delegate bool DispatchHandler(string[] arguments);

		private readonly Dictionary<string, DispatchHandler> dispatchers = new Dictionary<string, DispatchHandler>();

		public void RegisterCommand(string command, DispatchHandler dispatcher)
		{
			dispatchers[command] = dispatcher;
		}

		public bool Dispatch(string cheat)
		{
			string[] array = cheat.Split(' ');
			if (array.Length < 1)
			{
				return true;
			}
			if (dispatchers.TryGetValue(array[0], out var value))
			{
				return value(array);
			}
			return false;
		}
	}

	public const string CheatClientAttackRange = "ar";

	public const string CheatClientSpawnPet = "pet";

	public const string CheatClientCreateGhost50 = "cg50";

	public const string CheatClientCreateGhost100 = "cg100";

	public const string CheatClientSpawnHotAirBalloon = "shab";

	private const float ratioCameraZoomDefault = 0.5f;

	private const float ratioCameraZoomIn = -4f;

	private ConsoleGUI _consoleGui;

	private readonly ClientCheatDispatcher _clientCheatDispatcher = new ClientCheatDispatcher();

	private readonly Dictionary<string, string[]> _macroCheats = new Dictionary<string, string[]>();

	private ulong _petId = 666uL;

	[Help("Usage: cheat CHEAT\nSend a cheat message to the server")]
	public void Cheat(string cheat)
	{
		if (_macroCheats.TryGetValue(cheat, out var value))
		{
			for (int i = 0; i < value.Length; i++)
			{
				DispatchCheat(value[i]);
			}
		}
		else
		{
			DispatchCheat(cheat);
		}
	}

	public bool GetAttackRangeState()
	{
		return CombatSystem.AttackAlertEnabled;
	}

	public void OpenCheatDocument()
	{
		Application.OpenURL("https://confluence.nexon.com/x/hLflB");
	}

	private void Start()
	{
		_consoleGui = UnityEngine.Object.FindObjectOfType<ConsoleGUI>();
		_clientCheatDispatcher.RegisterCommand("guide", (string[] arguments) => ClientCheatCompletePlayGuide(arguments));
		_clientCheatDispatcher.RegisterCommand("gui", (string[] arguments) => ClientCheatCompletePlayGuide(all: false));
		_clientCheatDispatcher.RegisterCommand("ga", (string[] arguments) => ClientCheatCompletePlayGuide(all: true));
		_clientCheatDispatcher.RegisterCommand("quick", (string[] arguments) => arguments.Length < 3 || !(arguments[1] == "move") || ClientCheatQuickMove(arguments[2]));
		_clientCheatDispatcher.RegisterCommand("qm", (string[] arguments) => arguments.Length < 2 || ClientCheatQuickMove(arguments[1]));
		_clientCheatDispatcher.RegisterCommand("mm", (string[] arguments) => arguments.Length < 3 || ClientCheatMoveMinimapCoordinate(arguments[1], arguments[2]));
		_clientCheatDispatcher.RegisterCommand("um", (string[] arguments) => ClientCheatUnlockAllMenuItems());
		_clientCheatDispatcher.RegisterCommand("ar", (string[] arguments) => ClientCheatToggleAttackRange());
		_clientCheatDispatcher.RegisterCommand("pet", (string[] arguments) => ClientCheatSpawnPet(arguments));
		_clientCheatDispatcher.RegisterCommand("cg50", (string[] arguments) => ClientCheatCreateGhost(50));
		_clientCheatDispatcher.RegisterCommand("cg100", (string[] arguments) => ClientCheatCreateGhost(100));
		_clientCheatDispatcher.RegisterCommand("shab", (string[] arguments) => ClientCheatSpawnHotAirBalloon());
		_clientCheatDispatcher.RegisterCommand("pos", delegate
		{
			Util.ClientPositionToWorldPosition(PlayerBehavior.LocalPlayer.CurrentPosition);
			Util.ClientPositionToTilePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
			return true;
		});
		_clientCheatDispatcher.RegisterCommand("get_quest_state", delegate(string[] arguments)
		{
			if (arguments.Length <= 1)
			{
				return true;
			}
			List<string> list = new List<string>();
			list.AddRange(arguments);
			list.RemoveAt(0);
			Connections.Frontend.Send(new GetQuestState
			{
				QuestIds = list.ToArray()
			}).On(delegate(Messages.QuestState msg, PacketHeader _)
			{
				foreach (KeyValuePair<string, Shared.Quest.QuestState> state in msg.States)
				{
					_ = state;
				}
			});
			return true;
		});
		_clientCheatDispatcher.RegisterCommand("dn", (string[] arguments) => ClientCheatDownloadPersonalIsland());
		_clientCheatDispatcher.RegisterCommand("tdn", (string[] arguments) => ClientCheatDownloadTerrainDatas());
		foreach (KeyValuePair<string, string[]> item in Json.ReadFromFile<Dictionary<string, string[]>>("cheat_macro_definition"))
		{
			string[] array = item.Key.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				_macroCheats[array[i]] = item.Value;
			}
		}
		EasyConsole.Console.Instance.RegisterParser(typeof(string), ParseStringArgument);
		EasyConsole.Console.Instance.RegisterParser(typeof(string[]), ParseStringArguments);
		EasyConsole.Console.Instance.RegisterCommand("cheat", this, "Cheat");
		EasyConsole.Console.Instance.RegisterCommand("c", this, "Cheat");
		EasyConsole.Console.Instance.RegisterCommand("req", this, "Request");
		EasyConsole.Console.Instance.RegisterCommand("mo", this, "Motion");
		EasyConsole.Console.Instance.RegisterCommand("eff", this, "Effect");
		EasyConsole.Console.Instance.RegisterCommand("play_aeffect", this, "PlayAttackedEffect");
		EasyConsole.Console.Instance.RegisterCommand("close", this, "ForceClose");
		EasyConsole.Console.Instance.RegisterCommand("radio_close", this, "ForceClose_Radiotower");
		EasyConsole.Console.Instance.RegisterCommand("so", this, "ScreenOrientaion");
		EasyConsole.Console.Instance.RegisterCommand("p2", this, "ProloguePhase2");
		EasyConsole.Console.Instance.RegisterCommand("p3", this, "ProloguePhase3");
		EasyConsole.Console.Instance.RegisterCommand("prbc", this, "PrologueResetBattleCounter");
		EasyConsole.Console.Instance.RegisterCommand("p4", this, "ProloguePhase4");
		EasyConsole.Console.Instance.RegisterCommand("p5", this, "PrologueSkip");
		EasyConsole.Console.Instance.RegisterCommand("cc", this, "ColorCorrection");
		EasyConsole.Console.Instance.RegisterCommand("fake_atv", this, "FakeActiveActions");
		EasyConsole.Console.Instance.RegisterCommand("guide_begin_flow", this, "BeginFlow");
		_clientCheatDispatcher.RegisterCommand("req_poi", (string[] arguments) => RequestPOI(arguments));
		EasyConsole.Console.Instance.RegisterCommand("ceq", this, "CheckEventQuests");
		EasyConsole.Console.Instance.RegisterCommand("war", this, "ShowWarpRushAllRankings");
		EasyConsole.Console.Instance.RegisterCommand("u", this, "OpenUri");
		EasyConsole.Console.Instance.RegisterCommand("uri", this, "OpenUri");
		EasyConsole.Console.Instance.RegisterCommand("hu", this, "HelpUri");
		EasyConsole.Console.Instance.RegisterCommand("help_uri", this, "HelpUri");
		EasyConsole.Console.Instance.RegisterCommand("cutscene", this, "PlayCutScene");
		_clientCheatDispatcher.RegisterCommand("fsp", delegate(string[] arguments)
		{
			GameSystem<FactionSystem>.Instance().SendMessage("SetFactionPeriod", arguments);
			return true;
		});
		EasyConsole.Console.Instance.RegisterCommand("web", this, "OpenWebBrowser");
		EasyConsole.Console.Instance.RegisterCommand("help", this, "OpenCheatDocument");
	}

	private void DispatchCheat(string cheat)
	{
		EasyConsole.Console.Instance.Print("Called with value " + cheat);
		if (!_clientCheatDispatcher.Dispatch(cheat))
		{
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = cheat
			}).On(delegate(Info msg, PacketHeader _)
			{
				InfoReceived(msg);
			}).On(delegate(Messages.Error msg, PacketHeader _)
			{
				ErrorReceived(msg);
			});
		}
	}

	private void ColorCorrection(float value)
	{
		Singleton<CustomColorCorrectionEffect>.Instance().Time = value;
	}

	[Help("Usage: req METHOD [DATA]")]
	private void Request(string methodAndData)
	{
		Cheat("req " + methodAndData);
	}

	private void Motion(string[] value)
	{
		float result = 0f;
		int num = ((value != null) ? value.Length : 0);
		if (float.TryParse(value[num - 1], out result))
		{
			num--;
		}
		if (num != 0)
		{
			PlayerController.MotionUpdater.Motion(value[0], result, 1f, forceTransition: true);
		}
	}

	private void Effect(string value)
	{
		ObjectManager.PlayParticle(PlayerBehavior.LocalPlayer.EntityId, value);
	}

	private void CheckEventQuests()
	{
		GameSystem<QuestSystem>.Instance().SendMessage("CheckEventQuests");
	}

	private void ShowWarpRushAllRankings()
	{
		WarpRushRanking warpRushRanking = UnityEngine.Object.FindObjectOfType<WarpRushRanking>();
		if (!(warpRushRanking == null))
		{
			warpRushRanking.SendMessage("ShowAllRevisions");
		}
	}

	private void OpenUri(string uri)
	{
		_consoleGui.IsOpen = false;
		KUtility.DelayedCall(this, delegate
		{
			if (uri.StartsWith("ui://"))
			{
				uri = uri.Substring("ui://".Length);
			}
			Singleton<UIManager>.Instance().OpenUri(uri);
		}, 0.1f);
	}

	private void HelpUri()
	{
		_consoleGui.IsOpen = false;
		IEnumerable<string> source = Singleton<UIManager>.Instance().CollectUri();
		string[] array = source.ToArray();
		StringSelector stringSelector = UIManager.Popup.Tooltip<StringSelector>();
		stringSelector.Set(array, delegate(int index)
		{
			string text = array[index];
			if (!text.Contains('{'))
			{
				Singleton<UIManager>.Instance().OpenUri(text);
			}
		});
		stringSelector.Show();
	}

	private void OpenWebBrowser()
	{
		_consoleGui.IsOpen = false;
		KUtility.DelayedCall(this, delegate
		{
			TestWebBrowserGroup testWebBrowserGroup = UIManager.FindScript<TestWebBrowserGroup>();
			if (!(testWebBrowserGroup == null))
			{
				testWebBrowserGroup.Open();
			}
		}, 0.1f);
	}

	private void PlayCutScene(string type)
	{
		if (type.ToInt() == 0)
		{
			RandomBoxScene.Load(delegate
			{
			}, RandomBoxScene.BoxType.X10);
		}
		_consoleGui.IsOpen = false;
	}

	private void InfoReceived(Info info)
	{
		EasyConsole.Console.Instance.Print("==> " + info.Text);
	}

	private void ErrorReceived(Messages.Error error)
	{
		EasyConsole.Console.Instance.Print("==X " + error.TypeName + ": " + error.Text);
	}

	private bool ParseStringArgument(string line, out object obj)
	{
		obj = line.Replace("\0", " ");
		return true;
	}

	private bool ParseStringArguments(string line, out object obj)
	{
		string[] array = line.Split(new string[3] { "\0", " ", "," }, StringSplitOptions.RemoveEmptyEntries);
		obj = array;
		return true;
	}

	private bool ParsePosition(string line, out object obj)
	{
		string[] array = line.Split(',');
		if (array.Length != 2)
		{
			obj = null;
			return false;
		}
		if (!int.TryParse(array[0], out var result))
		{
			obj = null;
			return false;
		}
		if (!int.TryParse(array[1], out var result2))
		{
			obj = null;
			return false;
		}
		Position position = default(Position);
		position.x = result;
		position.y = result2;
		obj = position;
		return true;
	}

	private void PlayAttackedEffect(string[] args)
	{
		int result = 0;
		if (args.Length != 0)
		{
			int.TryParse(args[0], out result);
		}
		int result2 = 0;
		if (args.Length > 1)
		{
			int.TryParse(args[1], out result2);
		}
		Singleton<DamageEffectManager>.Instance().PlayEffectSet((AttackType)result, (DamageEffectManager.Result)result2, PlayerBehavior.LocalPlayer.CurrentPosition, isAttackerLocalPlayer: true);
	}

	private bool ClientCheatCompletePlayGuide(string[] arguments)
	{
		return ClientCheatCompletePlayGuide(arguments.Length >= 2 && arguments[1] == "all");
	}

	private bool ClientCheatCompletePlayGuide(bool all)
	{
		if (all)
		{
			GameSystem<PlayGuideSystem>.Instance().CompleteAllEvents();
		}
		else
		{
			GameSystem<PlayGuideSystem>.Instance().CompleteCurrentEvent();
		}
		return true;
	}

	private bool ClientCheatQuickMove(string valueText)
	{
		if (int.TryParse(valueText, out var result) && result > 0)
		{
			Singleton<PlayerController>.Instance().CheatMoveSpeedMultiply = result;
		}
		return true;
	}

	private bool ClientCheatMoveMinimapCoordinate(string strX, string strY)
	{
		float x = Convert.ToSingle(strX);
		float y = Convert.ToSingle(strY);
		Vector2 vector = Util.WorldPositionToTilePosition(MapPositionParser.HumaneTileToPosition(new Vector2(x, y)));
		int tileX = (int)vector.x;
		int tileY = (int)vector.y;
		_consoleGui.IsOpen = false;
		UIManager.MessageBox.Show($"Do you want to teleport to ({tileX},{tileY})", delegate(bool ok)
		{
			if (ok)
			{
				Connections.Frontend.Send(new Cheat
				{
					_Cheat = $"m {tileX} {tileY}"
				});
			}
		});
		return true;
	}

	private bool ClientCheatUnlockAllMenuItems()
	{
		if (!GameSystem<MenuSystem>.HasInstance())
		{
			return false;
		}
		foreach (MenuType value in Enum.GetValues(typeof(MenuType)))
		{
			GameSystem<MenuSystem>.Instance().EnableMenu(value, enable: true, checkHidden: false);
		}
		return true;
	}

	private bool ClientCheatToggleAttackRange()
	{
		CombatSystem.AttackAlertEnabled = !CombatSystem.AttackAlertEnabled;
		return true;
	}

	private bool ClientCheatSpawnPet(string[] arguments)
	{
		Dictionary<string, ushort> dictionary = new Dictionary<string, ushort>();
		dictionary.Add("tri", 2003);
		dictionary.Add("gal", 2093);
		dictionary.Add("sty", 2094);
		ushort value = dictionary.First().Value;
		if (arguments.Length >= 2)
		{
			dictionary.TryGetValue(arguments[1], out value);
		}
		_petId++;
		return true;
	}

	private bool ClientCheatCreateGhost(int instanceCount)
	{
		return false;
	}

	private void ForceClose()
	{
		Connections.Frontend.Close();
	}

	private void ForceClose_Radiotower()
	{
		Connections.Radiotower.Close();
	}

	private void ScreenOrientaion()
	{
	}

	private void ProloguePhase2()
	{
		Singleton<PlayerController>.Instance().Teleport(new Vector3(-10700f, 0f, 0f), TeleportType.Unknown, instance: true);
		PlayerBehavior.LocalPlayer.SetWeaponData(new WeaponDisplayInfo
		{
			Projectile = null,
			DetonateDelay = null,
			ProjectileSpeed = null,
			WeaponFramework = "barehand"
		});
		Singleton<PrologueManager>.Instance().DoPhase2();
	}

	private void ProloguePhase3()
	{
		Singleton<PrologueManager>.Instance().DoPhase2(skipTunnelEffect: true);
		GameSystem<InputSystem>.Instance().MoveLock = false;
		Singleton<PrologueManager>.Instance().DoGetAxe();
		Singleton<PlayerController>.Instance().Teleport(new Vector3(-1592f, 0f, 0f), TeleportType.Unknown, instance: true);
	}

	private void PrologueResetBattleCounter(string count)
	{
	}

	private void ProloguePhase4()
	{
		Singleton<PrologueManager>.Instance().DoPhase2(skipTunnelEffect: true);
		GameSystem<InputSystem>.Instance().MoveLock = false;
		Singleton<PrologueManager>.Instance().DoGetAxe();
		Singleton<PlayerController>.Instance().Teleport(new Vector3(-165f, 0f, 0f), TeleportType.Unknown, instance: true);
		Singleton<PrologueManager>.Instance().DelayedCall(Singleton<TrainTrexController>.Instance().PlayTrexCutScene, 1f);
	}

	private void PrologueSkip()
	{
		Singleton<PrologueManager>.Instance().SkipPrologue();
	}

	private void FakeActiveActions()
	{
		string[] array = new string[41]
		{
			"onehand_default_a", "onehand_default_b", "onehand_default_c", "twohand_default_a", "twohand_default_b", "twohand_default_c", "twohand_lance_default_a", "twohand_lance_default_b", "twohand_lance_default_c", "onehand_smash",
			"twohand_smash", "onehand_stab", "barehand_combination", "onehand_dash", "twohand_dash", "twohand_lance_dash", "onehand_dodge", "twohand_guard", "twohand_lance_guard", "ranged_sling_default",
			"ranged_bow_default_a", "ranged_bow_default_b", "ranged_bow_default_c", "ranged_crossbow_default", "ranged_sling_hard_throwing", "ranged_bow_aimedshot", "ranged_crossbow_aimedshot", "ranged_sling_quickshot", "ranged_bow_quickshot", "ranged_crossbow_quickshot",
			"ranged_sling_dodge", "ranged_bow_dodge", "ranged_crossbow_dodge", "barehand_default_a", "barehand_default_b", "barehand_default_c", "barehand_smash", "barehand_combination", "barehand_kick_a", "barehand_kick_b",
			"barehand_dodge"
		};
		foreach (string text in array)
		{
			Cheat("fatv " + text);
		}
	}

	private void BeginFlow(string flowName)
	{
		GameSystem<PlayGuideSystem>.Instance().BeginFlow(flowName);
	}

	private bool RequestPOI(string[] args)
	{
		if (args.Length != 2)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Array values = Enum.GetValues(typeof(Shared.System.PointOfInterest));
			stringBuilder.AppendLine("Usage: req_poi <poi_type>");
			for (int i = 0; i < values.Length; i++)
			{
				Shared.System.PointOfInterest pointOfInterest = (Shared.System.PointOfInterest)values.GetValue(i);
				if (pointOfInterest != Shared.System.PointOfInterest.Invalid)
				{
					stringBuilder.AppendFormat(" {0}: {1}\n", (int)pointOfInterest, pointOfInterest);
				}
			}
			return true;
		}
		Shared.System.PointOfInterest type = args[1].ToEnum(Shared.System.PointOfInterest.Port);
		Connections.Frontend.Send(new RequestNearestPOI
		{
			Tile = PlayerBehavior.LocalPlayer.CurrentTile,
			Type = type
		});
		return true;
	}

	private bool ClientCheatSpawnHotAirBalloon()
	{
		Singleton<AssetBundleManager>.Instance().RequestAsset("Models/Prop/vehicle/hot_airballoon/hot_airballoon_01.prefab", typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
			if (!(asset == null) && !(localPlayer == null))
			{
				UnityEngine.Object.Instantiate(asset, Maths.GetRandomSurroundingPos(localPlayer.transform.position, 500f), Quaternion.identity);
			}
		});
		return true;
	}

	private bool ClientCheatDownloadPersonalIsland()
	{
		Connections.Frontend.Send(new RequestDumpedPersonalIsland
		{
			PlayerEntityId = PlayerBehavior.LocalPlayer.EntityId
		}).On(delegate(DumpedPersonalIsland msg, PacketHeader _)
		{
			Json.Read<DumpedPersonalIsland>(Json.Write(msg, indented: true));
		});
		return true;
	}

	private bool ClientCheatDownloadTerrainDatas()
	{
		string text = "pe10gr_1";
		Dictionary<string, byte[]> dictionary = DumpedIslandUtils.DownloadDumpedDatas("https://s3-ap-northeast-1.amazonaws.com/k1server-dumped-islands.k.nexon.com/common/terrains/terrains_" + text);
		_ = dictionary["whole.biomes"];
		byte[] bytes = dictionary["1,1"];
		new ChunkData().LoadFromBytes(bytes);
		return true;
	}
}
