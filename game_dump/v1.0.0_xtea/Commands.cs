using System;
using System.Collections.Generic;
using System.Linq;
using Homans.Console;
using ItemSystem;
using K1Network;
using MenuData;
using Messages;
using Shared.Battle;
using Shared.System;
using UnityEngine;

public class Commands : KSingleton<Commands>
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
			DispatchHandler value;
			return dispatchers.TryGetValue(array[0], out value) && value(array);
		}
	}

	public const string CheatClientCameraZoom = "camerazoom";

	public const string CheatClientDamageMeter = "dm";

	public const string CheatClientAttackRange = "ar";

	public const string CheatClientSpawnPet = "pet";

	private const float ratioCameraZoomDefault = 0.5f;

	private const float ratioCameraZoomIn = -4f;

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

	public bool GetCameraZoomModeState()
	{
		return KSingleton<MainCamera>.Instance().MinZoom != 0.5f;
	}

	public bool GetDamageMeterState()
	{
		return CombatSystem.EnableDamageLog;
	}

	public bool GetAttackRangeState()
	{
		return CombatSystem.EnableAttackAlert;
	}

	private void Start()
	{
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Expected O, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Expected O, but got Unknown
		_clientCheatDispatcher.RegisterCommand("guide", (string[] arguments) => ClientCheatCompletePlayGuide(arguments));
		_clientCheatDispatcher.RegisterCommand("gui", (string[] arguments) => ClientCheatCompletePlayGuide(all: false));
		_clientCheatDispatcher.RegisterCommand("ga", (string[] arguments) => ClientCheatCompletePlayGuide(all: true));
		_clientCheatDispatcher.RegisterCommand("lpt", (string[] arguments) => LocalPushTest());
		_clientCheatDispatcher.RegisterCommand("quick", (string[] arguments) => arguments.Length < 3 || !(arguments[1] == "move") || ClientCheatQuickMove(arguments[2]));
		_clientCheatDispatcher.RegisterCommand("qm", (string[] arguments) => arguments.Length < 2 || ClientCheatQuickMove(arguments[1]));
		_clientCheatDispatcher.RegisterCommand("mm", (string[] arguments) => arguments.Length < 3 || ClientCheatMoveMinimapCoordinate(arguments[1], arguments[2]));
		_clientCheatDispatcher.RegisterCommand("um", (string[] arguments) => ClientCheatUnlockAllMenuItems());
		_clientCheatDispatcher.RegisterCommand("camerazoom", (string[] arguments) => ClientCheatToggleCameraZoomMode());
		_clientCheatDispatcher.RegisterCommand("dm", (string[] arguments) => ClientCheatToggleDamageMeter());
		_clientCheatDispatcher.RegisterCommand("ar", (string[] arguments) => ClientCheatToggleAttackRange());
		_clientCheatDispatcher.RegisterCommand("pet", (string[] arguments) => ClientCheatSpawnPet(arguments));
		Dictionary<string, string[]> dictionary = KUtility.ParseJsonFile<Dictionary<string, string[]>>("cheat_macro_definition");
		foreach (KeyValuePair<string, string[]> item in dictionary)
		{
			string[] array = item.Key.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				_macroCheats[array[i]] = item.Value;
			}
		}
		Console.Instance.RegisterParser(typeof(string), new ParserCallback(ParseStringArgument));
		Console.Instance.RegisterParser(typeof(string[]), new ParserCallback(ParseStringArguments));
		Console.Instance.RegisterCommand("cheat", (object)this, "Cheat");
		Console.Instance.RegisterCommand("c", (object)this, "Cheat");
		Console.Instance.RegisterCommand("req", (object)this, "Request");
		Console.Instance.RegisterCommand("mo", (object)this, "Motion");
		Console.Instance.RegisterCommand("eff", (object)this, "Effect");
		Console.Instance.RegisterCommand("play_aeffect", (object)this, "PlayAttackedEffect");
		Console.Instance.RegisterCommand("close", (object)this, "ForceClose");
		Console.Instance.RegisterCommand("so", (object)this, "ScreenOrientaion");
		Console.Instance.RegisterCommand("p2", (object)this, "ProloguePhase2");
		Console.Instance.RegisterCommand("p3", (object)this, "ProloguePhase3");
		Console.Instance.RegisterCommand("prbc", (object)this, "PrologueResetBattleCounter");
		Console.Instance.RegisterCommand("p4", (object)this, "ProloguePhase4");
		Console.Instance.RegisterCommand("p5", (object)this, "PrologueSkip");
		Console.Instance.RegisterCommand("cc", (object)this, "ColorCorrection");
		Console.Instance.RegisterCommand("fake_atv", (object)this, "FakeActiveActions");
		Console.Instance.RegisterCommand("guide_begin_flow", (object)this, "BeginFlow");
		_clientCheatDispatcher.RegisterCommand("req_poi", (string[] arguments) => RequestPOI(arguments));
	}

	private void DispatchCheat(string cheat)
	{
		Console.Instance.Print("Called with value " + cheat);
		if (!_clientCheatDispatcher.Dispatch(cheat))
		{
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = cheat
			}).On(delegate(Info msg, PacketHeader _)
			{
				InfoReceived(msg);
			}).On(delegate(Error msg, PacketHeader _)
			{
				ErrorReceived(msg);
			});
		}
	}

	private void ColorCorrection(float value)
	{
		KSingleton<CustomColorCorrectionEffect>.Instance().Time = value;
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
			KSingleton<PlayerController>.Instance().Motion(value[0], result);
		}
	}

	private void Effect(string[] value)
	{
		float result = 0f;
		int num = ((value != null) ? value.Length : 0);
		if (num == 0)
		{
			return;
		}
		if (num >= 2)
		{
			float.TryParse(value[1], out result);
		}
		if (num < 1)
		{
			return;
		}
		try
		{
			KSingleton<PlayerController>.Instance().ParticleEffect(value[0], result);
		}
		catch
		{
		}
	}

	private void InfoReceived(Info info)
	{
		Console.Instance.Print("==> " + info.Text);
	}

	private void ErrorReceived(Error error)
	{
		Console.Instance.Print("==X " + error.TypeName + ": " + error.Text);
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
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		int result = 0;
		if (args.Length > 0)
		{
			int.TryParse(args[0], out result);
		}
		int result2 = 0;
		if (args.Length > 1)
		{
			int.TryParse(args[1], out result2);
		}
		KSingleton<DamageEffectManager>.Instance().PlayEffectSet((AttackType)result, (DamageEffectManager.Result)result2, PlayerBehavior.LocalPlayer.CurrentPosition, isAttackerLocalPlayer: true);
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
			KSingleton<PlayerController>.Instance().CheatMoveSpeedMultiply = result;
		}
		return true;
	}

	private bool ClientCheatMoveMinimapCoordinate(string strX, string strY)
	{
		float num = Convert.ToSingle(strX) - 0.5f;
		float num2 = Convert.ToSingle(strY) + 0.5f;
		float num3 = Mathf.Cos((float)Math.PI / 4f);
		float num4 = Mathf.Sin((float)Math.PI / 4f);
		float num5 = (float)TerrainMeta.TileCount * 0.5f;
		int tileX = (int)(num * num3 + num2 * num4 + num5);
		int tileY = (int)((0f - num) * num4 + num2 * num3 + num5);
		ConsoleGUI consoleGUI = Object.FindObjectOfType<ConsoleGUI>();
		consoleGUI.IsOpen = false;
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

	private bool ClientCheatToggleCameraZoomMode()
	{
		KSingleton<MainCamera>.Instance().MinZoom = ((!GetCameraZoomModeState()) ? (-4f) : 0.5f);
		return true;
	}

	private bool ClientCheatToggleDamageMeter()
	{
		CombatSystem.EnableDamageLog = !CombatSystem.EnableDamageLog;
		return true;
	}

	private bool ClientCheatUnlockAllMenuItems()
	{
		if (!GameSystem<MenuSystem>.HasInstance())
		{
			return false;
		}
		Array values = Enum.GetValues(typeof(MenuType));
		foreach (int item in values)
		{
			GameSystem<MenuSystem>.Instance().EnableMenu((MenuType)item, enable: true);
		}
		return true;
	}

	private bool ClientCheatToggleAttackRange()
	{
		CombatSystem.EnableAttackAlert = !CombatSystem.EnableAttackAlert;
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
		KSingleton<AnimalManager>.Instance().MakeVehicle(new Messages.Rider
		{
			VehicleId = _petId,
			VehicleEntityType = value
		}, ((Component)PlayerBehavior.LocalPlayer).gameObject);
		_petId++;
		return true;
	}

	private bool LocalPushTest()
	{
		KSingleton<GameManager>.Instance().PushNotification.LocalPushAfter(PushNotification.Type.Debug, "Local Push Test", string.Empty, 5);
		return true;
	}

	private void ForceClose()
	{
		Connections.Frontend.Close();
	}

	private void ScreenOrientaion()
	{
	}

	private void ProloguePhase2()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<PlayerController>.Instance().Teleport(new Vector3(-10700f, 0f, 0f));
		PerformanceData performanceData = GameSystem<EquipSystem>.Instance().Barehands.GetPerformanceData("weapon");
		performanceData.str_attrs.TryGetValue("weapon_framework", out var value);
		PlayerBehavior.LocalPlayer.SetWeaponData(new WeaponDisplayInfo
		{
			Projectile = null,
			DetonateDelay = null,
			ProjectileSpeed = null,
			WeaponFramework = value
		});
		KSingleton<PrologueManager>.Instance().DoPhase2();
	}

	private void ProloguePhase3()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<PrologueManager>.Instance().DoPhase2(skipTunnelEffect: true);
		KSingleton<PlayerController>.Instance().MoveLock = false;
		KSingleton<PrologueManager>.Instance().DoGetAxe();
		KSingleton<PlayerController>.Instance().Teleport(new Vector3(-1592f, 0f, 0f));
	}

	private void PrologueResetBattleCounter(string count)
	{
		PrologueManager.PlayerBattleAi.DebugResetBattleCounter(Convert.ToInt32(count));
	}

	private void ProloguePhase4()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<PrologueManager>.Instance().DoPhase2(skipTunnelEffect: true);
		KSingleton<PlayerController>.Instance().MoveLock = false;
		KSingleton<PrologueManager>.Instance().DoGetAxe();
		KSingleton<PlayerController>.Instance().Teleport(new Vector3(-165f, 0f, 0f));
		KSingleton<PrologueManager>.Instance().DelayedCall(KSingleton<TrainTrexController>.Instance().PlayTrexCutScene, 1f);
	}

	private void PrologueSkip()
	{
		KSingleton<PrologueManager>.Instance().SkipPrologue();
	}

	private void FakeActiveActions()
	{
		string[] array = new string[68]
		{
			"onehand_sword_default_a", "onehand_sword_default_b", "onehand_sword_default_c", "onehand_axe_default_a", "onehand_axe_default_b", "onehand_axe_default_c", "onehand_blunt_default_a", "onehand_blunt_default_b", "onehand_blunt_default_c", "twohand_sword_default_a",
			"twohand_sword_default_b", "twohand_sword_default_c", "twohand_axe_default_a", "twohand_axe_default_b", "twohand_axe_default_c", "twohand_blunt_default_a", "twohand_blunt_default_b", "twohand_blunt_default_c", "twohand_lance_default_a", "twohand_lance_default_b",
			"twohand_lance_default_c", "onehand_sword_smash", "onehand_axe_smash", "onehand_blunt_smash", "twohand_sword_smash", "twohand_axe_smash", "twohand_blunt_smash", "twohand_lance_smash", "onehand_sword_stab", "onehand_axe_stab",
			"onehand_blunt_stab", "onehand_sword_combination", "onehand_axe_combination", "onehand_blunt_combination", "onehand_sword_dash", "onehand_axe_dash", "onehand_blunt_dash", "twohand_sword_dash", "twohand_axe_dash", "twohand_blunt_dash",
			"twohand_lance_dash", "onehand_sword_dodge", "onehand_axe_dodge", "onehand_blunt_dodge", "twohand_sword_guard", "twohand_axe_guard", "twohand_blunt_guard", "twohand_lance_guard", "ranged_sling_default", "ranged_bow_default",
			"ranged_crossbow_default", "ranged_sling_smash", "ranged_bow_aimedshot", "ranged_crossbow_aimedshot", "ranged_sling_quickshot", "ranged_bow_quickshot", "ranged_crossbow_quickshot", "ranged_sling_dodge", "ranged_bow_dodge", "ranged_crossbow_dodge",
			"barehand_default_a", "barehand_default_b", "barehand_default_c", "barehand_smash", "barehand_combination", "barehand_kick_a", "barehand_kick_b", "barehand_dodge"
		};
		string[] array2 = array;
		foreach (string arg in array2)
		{
			Cheat($"fatv {arg}");
		}
	}

	private void BeginFlow(string flowName)
	{
		GameSystem<PlayGuideSystem>.Instance().BeginFlow(flowName);
	}

	private bool RequestPOI(string[] args)
	{
		if (args.Count() != 2)
		{
			return true;
		}
		Shared.System.PointOfInterest type = (Shared.System.PointOfInterest)(int)Enum.Parse(typeof(Shared.System.PointOfInterest), args[1], ignoreCase: true);
		Connections.Frontend.Send(new RequestNearestPOI
		{
			Tile = PlayerBehavior.LocalPlayer.CurrentTile,
			Type = type
		});
		return true;
	}
}
