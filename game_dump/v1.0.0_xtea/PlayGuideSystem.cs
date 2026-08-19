using System;
using System.Collections.Generic;
using EncyclopediaData;
using ItemSystem;
using JetBrains.Annotations;
using K1Network;
using L10N;
using Messages;
using Newtonsoft.Json.Linq;
using PlayGuide;
using Shared.Region;
using Shared.System;
using TerrainData;
using UnityEngine;

public class PlayGuideSystem : GameSystem<PlayGuideSystem>
{
	public enum GuideConfig
	{
		StorageKey,
		EventFile,
		FlowFile
	}

	public class GuideStorageData
	{
		public Dictionary<string, List<string>> CompletedEvents;

		public Vector2 LastDogPOITile;

		public DogGuideState LastDogGuideState;
	}

	private const string NormalFlow = "normal_flow";

	private static readonly GuideEvent BeginEvent = new GuideEvent
	{
		Name = "begin",
		ToDoCollection = null
	};

	private static readonly GuideEvent BlankEvent = new GuideEvent
	{
		Name = "blank",
		ToDoCollection = null
	};

	private static readonly GuideStorageData InitGuideStorageData = new GuideStorageData
	{
		CompletedEvents = new Dictionary<string, List<string>>(),
		LastDogPOITile = Vector2.zero,
		LastDogGuideState = DogGuideState.Intro
	};

	private static Role _prevGuideRole = Role.Tutorial;

	private readonly Dictionary<string, GuideEvent> _eventDictionary = new Dictionary<string, GuideEvent>();

	private readonly Dictionary<string, FlowContainer> _flowDict = new Dictionary<string, FlowContainer>();

	private readonly List<FlowCondition> _remainFlowConditions = new List<FlowCondition>();

	private readonly List<FlowStack> _flowStacks = new List<FlowStack>();

	private GuideEvent _currentEvent = BeginEvent;

	private readonly Queue<GuideEvent> _delayedEventQueue = new Queue<GuideEvent>();

	private readonly List<string> _completedEvents = new List<string>();

	private Role _currentGuideRole = Role.Tutorial;

	private Point2 _lastNearestPoi = Point2.zero;

	public bool IsGuideBegin { get; private set; }

	public CustomCommand Command { get; private set; }

	public bool Initialized { get; private set; }

	private bool Enabled => !AllCompleted && _eventDictionary.Count > 0;

	private bool AllCompleted
	{
		get
		{
			for (int i = 0; i < _flowStacks.Count; i++)
			{
				FlowStack flowStack = _flowStacks[i];
				if (!flowStack.Recoder.IsFinished())
				{
					return false;
				}
			}
			return _remainFlowConditions.Count == 0;
		}
	}

	public static bool UseLocalGuideProgress => false;

	public bool PauseUpdate { get; set; }

	public event Action<IList<string>, GuideEvent> PostEventSet;

	public event Action ReturnFromUnstable;

	public event Action<string, string> ExternalEventOccured;

	public event Action InstantGuideCompleted;

	private FlowContainer FindFlowContainer(string flowName)
	{
		FlowContainer flowContainer = _flowDict.Get(flowName);
		if (flowContainer == null)
		{
		}
		return flowContainer;
	}

	private FlowStack FindFlowStack(string flowName)
	{
		for (int i = 0; i < _flowStacks.Count; i++)
		{
			if (_flowStacks[i].Name == flowName)
			{
				return _flowStacks[i];
			}
		}
		return null;
	}

	private FlowStack AddFlowStack(string flowName)
	{
		FlowStack flowStack = CreateFlowStack(flowName);
		if (flowStack == null)
		{
			return null;
		}
		_flowStacks.Add(flowStack);
		return flowStack;
	}

	private FlowStack CreateFlowStack(string flowName)
	{
		FlowContainer flowContainer = FindFlowContainer(flowName);
		if (flowContainer == null)
		{
			return null;
		}
		FlowStack flowStack = new FlowStack();
		flowStack.Name = flowName;
		flowStack.Stack = new Stack<FlowStackItem>();
		flowStack.Recoder = new GuideRecoder();
		FlowStackItem item = new FlowStackItem(flowContainer);
		flowStack.Stack.Push(item);
		return flowStack;
	}

	public GuideEvent GetCurrentEvent()
	{
		return _currentEvent;
	}

	public int GetFlowStackCount()
	{
		return _flowStacks.Count;
	}

	public FlowStack GetFlowStack(int index)
	{
		return _flowStacks[index];
	}

	private void ClearAll()
	{
		GameSystem<ToDoListSystem>.Instance().RemoveAll();
		if (this.PostEventSet != null)
		{
			this.PostEventSet(null, null);
		}
		RemoveHelperTargets(_currentEvent);
		PauseUpdate = false;
		((MonoBehaviour)this).StopAllCoroutines();
		_eventDictionary.Clear();
		_flowDict.Clear();
		Command.ClearAll();
		int count = _remainFlowConditions.Count;
		for (int i = 0; i < count; i++)
		{
			_remainFlowConditions[i].TryUnregister();
		}
		_remainFlowConditions.Clear();
		_flowStacks.Clear();
		_currentEvent = BeginEvent;
		_delayedEventQueue.Clear();
		_completedEvents.Clear();
		IsGuideBegin = false;
		_prevGuideRole = ((!KSingleton<GameManager>.Instance().IsEmigrated) ? Role.Tutorial : _currentGuideRole);
	}

	private void Awake()
	{
		Command = new CustomCommand(this);
		Connections.Frontend.On(delegate(NearestPOI msg, PacketHeader _)
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			Point2? tile = msg.Tile;
			if (tile.HasValue)
			{
				Point2? tile2 = msg.Tile;
				_lastNearestPoi = tile2.Value;
				if (KUtility.GetSize(_currentEvent.HelperTargets) > 0 && _currentEvent.HelperTargets[0].type == "nearest_poi")
				{
					KSingleton<UIManager>.Instance().PlayGuideHelper.SetArrowTarget(TerrainA6.TilePositionToClientPosition(_lastNearestPoi));
				}
			}
		});
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (!GameManager.IsPrologueMode)
			{
				if (_currentGuideRole == Role.Tutorial)
				{
					Command.ReservedSpawnDogAfterMainSceneLoaded();
				}
				if (Enabled && KSingleton<TerrainA6>.HasInstance())
				{
					KSingleton<TerrainA6>.Instance().LoadingChunksFinished += TerrainA6_OnLoadingChunksFinished;
				}
			}
		};
		KSingleton<GameManager>.Instance().MainSceneClosed += delegate
		{
			if (!GameManager.IsPrologueMode)
			{
				_prevGuideRole = ((!KSingleton<GameManager>.Instance().IsEmigrated) ? Role.Tutorial : _currentGuideRole);
			}
		};
		GameSystem<ToDoListSystem>.Instance().Added += ToDoListSystem_Added;
	}

	private void ToDoListSystem_Added(ToDoCollection toDoCollection, bool immediately)
	{
		if (_currentEvent.ToDoCollection == toDoCollection)
		{
			RefreshHelperTarget();
		}
	}

	private void Update()
	{
		if (IsGuideBegin)
		{
			for (int num = _remainFlowConditions.Count - 1; num >= 0; num--)
			{
				_remainFlowConditions[num]?.Process();
			}
		}
	}

	public void ReloadAll()
	{
		ResetGuideProgressSaved();
		Initialize(_currentGuideRole, null);
		TerrainA6_OnLoadingChunksFinished();
	}

	private void LoadPlayGuideFlow()
	{
		string guideConfig = GetGuideConfig(GuideConfig.FlowFile);
		JObject jObject = KUtility.ParseJsonFile<JObject>(guideConfig);
		if (jObject == null)
		{
			Debug.LogError((object)("Deserialize PlayGuideFlow failed: " + guideConfig));
		}
		else
		{
			LoadPlayGuideFlowJson(jObject, _flowDict, _remainFlowConditions);
		}
	}

	public static void LoadPlayGuideFlowJson(JObject flowJsons, Dictionary<string, FlowContainer> dict, List<FlowCondition> conditions)
	{
		dict.Clear();
		conditions.Clear();
		foreach (KeyValuePair<string, JToken> flowJson in flowJsons)
		{
			JArray list = flowJson.Value["flow"] as JArray;
			FlowContainer flowContainer = new FlowContainer();
			FlowData.ParseFlow(list, flowContainer);
			dict.Add(flowJson.Key, flowContainer);
			FlowCondition flowCondition = LoadFlowCondition(flowJson);
			if (flowCondition != null)
			{
				conditions.Add(flowCondition);
			}
		}
	}

	private static FlowCondition LoadFlowCondition(KeyValuePair<string, JToken> flow)
	{
		JToken jToken = flow.Value["type"];
		if (jToken == null || jToken.Type != JTokenType.String)
		{
			return null;
		}
		string param = string.Empty;
		JToken jToken2 = flow.Value["param"];
		if (jToken2 != null && jToken2.Type == JTokenType.String)
		{
			param = (string)jToken2;
		}
		return FlowConditionFactory.Create((string)jToken, param, flow.Key);
	}

	private void LoadPlayGuideEvent()
	{
		_eventDictionary.Clear();
		string guideConfig = GetGuideConfig(GuideConfig.EventFile);
		Dictionary<string, GuideEventJson> dictionary = KUtility.ParseJsonFile<Dictionary<string, GuideEventJson>>(guideConfig);
		if (dictionary == null)
		{
			Debug.LogError((object)("Deserialize PlayGuideEvent failed: " + guideConfig));
			return;
		}
		NPCType prevNPCType = NPCType.TheFirm;
		foreach (KeyValuePair<string, GuideEventJson> item in dictionary)
		{
			if (!string.IsNullOrEmpty(item.Key))
			{
				GuideEvent guideEvent = GuideEvent.Create(item.Key, item.Value, prevNPCType);
				prevNPCType = guideEvent.NPCType;
				_eventDictionary.Add(guideEvent.Name, guideEvent);
			}
		}
		_eventDictionary.Add(BlankEvent.Name, BlankEvent);
	}

	private void ResetGuideProgressSaved()
	{
		PlayerPrefs.SetString(GetGuideConfig(GuideConfig.StorageKey), string.Empty);
	}

	private void LoadGuideProgress(string flowName, IList<string> flows)
	{
		FlowStack flowStack = AddFlowStack(flowName);
		if (flowStack == null)
		{
			return;
		}
		GuideRecoder recoder = flowStack.Recoder;
		recoder.Load(flows);
		recoder.IsRecordingEnabled = _currentGuideRole != Role.Tutorial || flowName != "normal_flow";
		string text = recoder.MoveNext();
		string text2 = string.Empty;
		while (text != null && !recoder.IsRecordingEnabled && !(text2 == "blank"))
		{
			text2 = DoMoveNextFlow(flowStack, recoder, forceCondition: true);
		}
		string text3 = null;
		while (text != null)
		{
			if (text3 != null)
			{
				_completedEvents.Add(text3);
			}
			string text4 = DoMoveNextFlow(flowStack, null, forceCondition: true, text == "true");
			if (!(text4 == text))
			{
				recoder.RemoveRemains();
				recoder.Record(text4);
				break;
			}
			text3 = text4;
			text = recoder.MoveNext();
		}
	}

	private void SaveGuideProgress()
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		for (int i = 0; i < _flowStacks.Count; i++)
		{
			FlowStack flowStack = _flowStacks[i];
			string name = flowStack.Name;
			if (!dictionary.ContainsKey(name))
			{
				dictionary.Add(name, flowStack.Recoder.GetFlows());
			}
		}
		GuideStorageData guideStorageData = new GuideStorageData();
		guideStorageData.CompletedEvents = dictionary;
		GuideStorageData guideStorageData2 = guideStorageData;
		Command.SaveDogGuideProgress(guideStorageData2);
		if (UseLocalGuideProgress)
		{
			string text = KUtility.SerializeJson(guideStorageData2);
			PlayerPrefs.SetString(GetGuideConfig(GuideConfig.StorageKey), text);
			PlayerPrefs.Save();
		}
		else
		{
			Connections.Frontend.Send(SetStorageItem(GetGuideConfig(GuideConfig.StorageKey), guideStorageData2));
		}
	}

	private static SetStorageItem SetStorageItem<TK>(string key, TK value)
	{
		SetStorageItem result = default(SetStorageItem);
		result.Key = key;
		result.Value = KUtility.SerializeJsonToBytes(value);
		return result;
	}

	private static GuideStorageData LoadGuideStorageData(string key, Dictionary<string, byte[]> storage)
	{
		if (storage != null && storage.TryGetValue(key, out var value) && value.Length != 0)
		{
			return KUtility.ParseJson<GuideStorageData>(value);
		}
		return null;
	}

	public void Initialize(Role type, Dictionary<string, byte[]> storage)
	{
		ClearAll();
		Initialized = true;
		if (type != Role.Invalid && type != 0)
		{
			_currentGuideRole = type;
			LoadPlayGuideFlow();
			LoadPlayGuideEvent();
			GuideStorageData guideStorageData = LoadGuideStorageData(GetGuideConfig(GuideConfig.StorageKey), storage);
			if (UseLocalGuideProgress)
			{
				string @string = PlayerPrefs.GetString(GetGuideConfig(GuideConfig.StorageKey));
				guideStorageData = KUtility.ParseJson<GuideStorageData>(@string);
			}
			if (guideStorageData == null)
			{
				guideStorageData = InitGuideStorageData;
			}
			Dictionary<string, List<string>> completedEvents = guideStorageData.CompletedEvents;
			LoadGuideProgress("normal_flow", completedEvents.Get("normal_flow"));
			for (int i = 0; i < _remainFlowConditions.Count; i++)
			{
				string name = _remainFlowConditions[i].Name;
				LoadGuideProgress(name, completedEvents.Get(name));
			}
			Command.LoadDogGuideProgress(guideStorageData);
		}
	}

	public static string GetGuideConfig(GuideConfig config, Role role)
	{
		string arg = string.Empty;
		string arg2 = string.Empty;
		switch (config)
		{
		case GuideConfig.StorageKey:
			arg2 = "guide_progress";
			break;
		case GuideConfig.EventFile:
			arg = "PlayGuide/";
			arg2 = "play_guide_event";
			break;
		case GuideConfig.FlowFile:
			arg = "PlayGuide/";
			arg2 = "play_guide_flow";
			break;
		}
		return $"{arg}{role.ToString().ToLower()}_{arg2}";
	}

	private string GetGuideConfig(GuideConfig config)
	{
		return GetGuideConfig(config, _currentGuideRole);
	}

	[CanBeNull]
	private GuideEvent FindEvent(string eventName)
	{
		return (!string.IsNullOrEmpty(eventName)) ? _eventDictionary.Get(eventName) : null;
	}

	private void PostSetEvent()
	{
		ApplyUIEvent(_currentEvent);
		ApplyMarkingNew(_currentEvent);
		ApplySpotlightTarget(_currentEvent);
		ApplySurvivalMemo(_currentEvent);
		RefreshHelperTarget();
		string[] array = _currentEvent.Messages;
		if (_currentEvent.MsgDuration > 0f && KUtility.GetSize(array) == 0)
		{
			array = new string[1] { string.Empty };
		}
		if (this.PostEventSet != null)
		{
			this.PostEventSet(array, _currentEvent);
		}
		Command.DispatchCustomCmd(_currentEvent.CustomCommand);
		ToDoCollection toDoCollection = _currentEvent.ToDoCollection;
		if (toDoCollection != null)
		{
			GameSystem<ToDoListSystem>.Instance().Add(toDoCollection);
		}
		if (KUtility.GetSize(array) == 0)
		{
			OnGuideMsgFinished();
		}
		if (!string.IsNullOrEmpty(_currentEvent.SpawnFlow))
		{
			BeginFlow(_currentEvent.SpawnFlow);
		}
	}

	private static void ApplyUIEvent(GuideEvent guideEvent)
	{
		int i = 0;
		for (int size = KUtility.GetSize(guideEvent.HighLightTargets); i < size; i++)
		{
			KSingleton<UIManager>.Instance().HighlightSprite(guideEvent.HighLightTargets[i], active: true);
		}
	}

	private void ApplyMarkingNew(GuideEvent guideEvent)
	{
		if (guideEvent.MarkingNew == null)
		{
			return;
		}
		for (int i = 0; i < guideEvent.MarkingNew.Length; i++)
		{
			if (string.IsNullOrEmpty(guideEvent.MarkingNew[i]))
			{
				continue;
			}
			string[] array = guideEvent.MarkingNew[i].Split('/');
			if (array.Length >= 2)
			{
				INewCheckerable newCheckerable = null;
				switch (array[0])
				{
				case "recipe":
					newCheckerable = GameSystem<RecipeSystem>.Instance().GetRecipe(array[1]);
					break;
				case "blueprint":
					newCheckerable = GameSystem<RecipeSystem>.Instance().GetBlueprint(array[1]);
					break;
				}
				if (newCheckerable != null)
				{
					newCheckerable.NewChecker.IsNew = true;
				}
			}
		}
	}

	public void RefreshHelperTarget()
	{
		if (_currentEvent.ToDoCollection == null || _currentEvent.ToDoCollection.IsReady)
		{
			ApplyHelperTarget(_currentEvent);
		}
	}

	private void ApplyHelperTarget(GuideEvent guideEvent)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (KUtility.GetSize(guideEvent.HelperTargets) == 0)
		{
			return;
		}
		for (int i = 0; i < guideEvent.HelperTargets.Length; i++)
		{
			HelperTarget helperTarget = guideEvent.HelperTargets[i];
			if (IsArrowHelperTarget(helperTarget.type))
			{
				Vector3 val = CalcArrowHelperTarget(helperTarget.type, helperTarget.id);
				if (val != Vector3.zero)
				{
					KSingleton<UIManager>.Instance().PlayGuideHelper.SetArrowTarget(val);
				}
				continue;
			}
			switch (helperTarget.type)
			{
			case "click":
			{
				ClickTargetLocator locator = ClickTargetFactory.Create(helperTarget.id, helperTarget.click_targets);
				KSingleton<UIManager>.Instance().PlayGuideHelper.EnableClickTarget(locator);
				break;
			}
			case "tooltip":
			{
				Transform val2 = KSingleton<UIManager>.Instance().FindTransform(helperTarget.id);
				if ((Object)(object)val2 != (Object)null)
				{
					WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
					widgetTooltipControl.Set(null, T._(helperTarget.text));
					widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Horizontal;
					UIWidget component = ((Component)val2).GetComponent<UIWidget>();
					widgetTooltipControl.Show(component, Vector2.zero, helperTarget.duration);
				}
				break;
			}
			}
		}
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
			return true;
		default:
			return false;
		}
	}

	private Vector3 CalcArrowHelperTarget(string type, string param)
	{
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		switch (type)
		{
		case "biome":
		{
			TerrainData.Biome[] biomes = TerrainDataHelper.ParseBiome(param);
			return KSingleton<TerrainA6>.Instance().GetNearestBiome(biomes, PlayerBehavior.LocalPlayer.CurrentPosition);
		}
		case "natural":
		{
			int[] entityTypes = TerrainDataHelper.ParseEntityTypes(param);
			return KSingleton<TerrainA6>.Instance().GetNearestNaturalObject(entityTypes, PlayerBehavior.LocalPlayer.CurrentPosition);
		}
		case "immovable":
		{
			int[] types = TerrainDataHelper.ParseEntityTypes(param);
			GameObject nearImmovable = PlayGuide.Util.GetNearImmovable(types, 9600f);
			return (!((Object)(object)nearImmovable != (Object)null)) ? Vector3.zero : nearImmovable.transform.position;
		}
		case "tile":
		{
			Vector2 tilePosition = StringToTile(param);
			return TerrainA6.TilePositionToClientPosition(tilePosition);
		}
		case "nearest_poi":
		{
			Shared.System.PointOfInterest type2 = param.ToEnum(Shared.System.PointOfInterest.Port);
			Connections.Frontend.Send(new RequestNearestPOI
			{
				Tile = PlayerBehavior.LocalPlayer.CurrentTile,
				Type = type2
			});
			return (!(_lastNearestPoi != Point2.zero)) ? Vector3.zero : TerrainA6.TilePositionToClientPosition(_lastNearestPoi);
		}
		default:
			return Vector3.zero;
		}
	}

	private static Vector2 StringToTile(string tilePos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(tilePos))
		{
			return Vector2.zero;
		}
		string[] array = tilePos.Split(',');
		return (Vector2)((array.Length == 2) ? new Vector2((float)array[0].ToInt(), (float)array[1].ToInt()) : Vector2.zero);
	}

	private void ApplySpotlightTarget(GuideEvent guideEvent)
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (guideEvent.SpotlightTarget == null)
		{
			return;
		}
		SpotlightTarget spotlightTarget = guideEvent.SpotlightTarget;
		Transform val = KSingleton<UIManager>.Instance().FindTransform(spotlightTarget.id);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		BlurMaskingGroup blurMaskingGroup = UIManager.FindScript<BlurMaskingGroup>();
		blurMaskingGroup.ClearObject();
		blurMaskingGroup.AddObject(((Component)val).gameObject);
		string text = T._(spotlightTarget.title);
		string text2 = T._(spotlightTarget.comment);
		if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(text2))
		{
			GuideTooltip tooltip = UIManager.Popup.Tooltip<GuideTooltip>();
			tooltip.Set(text, text2);
			tooltip.CommentWidth = spotlightTarget.comment_width;
			tooltip.Direction = spotlightTarget.direction;
			tooltip.HideIgnoreParent = ((Component)blurMaskingGroup).transform;
			blurMaskingGroup.AddObject(((Component)tooltip).gameObject);
			Vector2 offset = default(Vector2);
			((Vector2)(ref offset))._002Ector(spotlightTarget.x_offset, spotlightTarget.y_offset);
			tooltip.DragLock = true;
			tooltip.Show(((Component)val).gameObject, offset, 3600f);
			blurMaskingGroup.CloseLockTimer = 2f;
			blurMaskingGroup.Open(delegate
			{
				tooltip.Hide(instant: true);
			});
		}
	}

	private void ApplySurvivalMemo(GuideEvent guideEvent)
	{
		if (guideEvent.SurvivalMemo != 0)
		{
			EncyclopediaSystem.SetMemoAvailable(MemoType.Survival, guideEvent.SurvivalMemo);
		}
	}

	private void StartEvent(FlowStack flowStack, string eventName)
	{
		GuideEvent guideEvent = FindEvent(eventName);
		if (guideEvent == null)
		{
			SetCurrentEvent(BlankEvent);
			return;
		}
		guideEvent.FlowStack = flowStack;
		if (guideEvent.IsSystem || guideEvent.Autorun || _currentGuideRole == Role.Tutorial || KUtility.GetSize(guideEvent.Messages) == 0)
		{
			SetCurrentEvent(guideEvent);
		}
		else
		{
			NotifyEvent(guideEvent);
		}
	}

	public void NotifyEvent(GuideEvent newEvent)
	{
		ToDoCollection collection = new ToDoCollection
		{
			NPCType = newEvent.NPCType,
			Key = newEvent.Name + ".notify"
		};
		collection.Clicked = delegate
		{
			if (newEvent.FlowStack != null)
			{
				newEvent.FlowStack.Notify = null;
			}
			UIManager.FindScript<ToDoListGroup>().ShowIcons(visible: false, 0f);
			SetCurrentEvent(newEvent);
			GameSystem<ToDoListSystem>.Instance().Remove(collection);
		};
		GameSystem<ToDoListSystem>.Instance().Add(collection);
		if (newEvent.FlowStack != null)
		{
			newEvent.FlowStack.Notify = collection;
		}
	}

	private void SetCurrentEvent(GuideEvent newEvent)
	{
		if (newEvent == null)
		{
			return;
		}
		PlayGuideGroup playGuideGroup = UIManager.FindScript<PlayGuideGroup>();
		if (_currentEvent.FlowStack != newEvent.FlowStack && playGuideGroup.HasGuideMsg())
		{
			if (newEvent != BlankEvent)
			{
				_delayedEventQueue.Enqueue(newEvent);
			}
			return;
		}
		_currentEvent = newEvent;
		PostSetEvent();
		if (_currentEvent == BlankEvent)
		{
			RestoreCurrentEvent();
		}
	}

	private void RestoreCurrentEvent()
	{
		for (int i = 0; i < _flowStacks.Count; i++)
		{
			FlowStack flowStack = _flowStacks[i];
			if (!flowStack.Started || flowStack.Stack.Count == 0)
			{
				continue;
			}
			FlowStackItem flowStackItem = flowStack.Stack.Peek();
			if (flowStackItem == null)
			{
				continue;
			}
			FlowData current = flowStackItem.GetCurrent();
			if (current != null)
			{
				GuideEvent guideEvent = FindEvent(current.Name);
				if (guideEvent != null && guideEvent != BlankEvent)
				{
					guideEvent.FlowStack = flowStack;
					_currentEvent = guideEvent;
					RefreshHelperTarget();
					break;
				}
			}
		}
	}

	private static void CompleteEventToDo([NotNull] GuideEvent guideEvent)
	{
		if (guideEvent.ToDoCollection != null)
		{
			int i = 0;
			for (int size = KUtility.GetSize(guideEvent.ToDoCollection.ToDoList); i < size; i++)
			{
				ToDoBase toDoBase = guideEvent.ToDoCollection.ToDoList[i];
				toDoBase.IsCompleted = true;
			}
			GameSystem<ToDoListSystem>.Instance().Remove(guideEvent.ToDoCollection);
		}
	}

	private static void RemoveEventToDo([NotNull] GuideEvent guideEvent)
	{
		if (guideEvent.ToDoCollection != null)
		{
			GameSystem<ToDoListSystem>.Instance().Remove(guideEvent.ToDoCollection);
		}
	}

	private static void RemoveHelperTargets([NotNull] GuideEvent guideEvent)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (KUtility.GetSize(guideEvent.HelperTargets) != 0)
		{
			for (int i = 0; i < guideEvent.HelperTargets.Length; i++)
			{
				HelperTarget helperTarget = guideEvent.HelperTargets[i];
				if (IsArrowHelperTarget(helperTarget.type))
				{
					KSingleton<UIManager>.Instance().PlayGuideHelper.SetArrowTarget(Vector3.zero);
					continue;
				}
				switch (helperTarget.type)
				{
				case "click":
					KSingleton<UIManager>.Instance().PlayGuideHelper.DisableClickTarget();
					break;
				}
			}
		}
		if (guideEvent.HighLightTargets != null)
		{
			for (int j = 0; j < guideEvent.HighLightTargets.Length; j++)
			{
				KSingleton<UIManager>.Instance().HighlightSprite(guideEvent.HighLightTargets[j], active: false);
			}
		}
	}

	private static bool CheckEquipTag(string[] arguments)
	{
		if (arguments.Length < 2)
		{
			return false;
		}
		ItemData data = GameSystem<EquipSystem>.Instance().FindEquipItem(arguments[0]);
		TagEvaluator tagEvaluator = new TagEvaluator(arguments[1]);
		return tagEvaluator.Evaluate(data);
	}

	private bool CheckTime(string[] arguments)
	{
		if (arguments.Length < 2)
		{
			return false;
		}
		if (!float.TryParse(arguments[0], out var result) || !float.TryParse(arguments[1], out var result2))
		{
			return false;
		}
		return TimeGauge.CheckTime(result, result2);
	}

	private bool CheckBiome(string[] arguments)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (arguments.Length < 1)
		{
			return false;
		}
		TerrainData.Biome[] array = TerrainDataHelper.ParseBiome(arguments[0]);
		if (array == null)
		{
			return false;
		}
		Vector3 nearestBiome = KSingleton<TerrainA6>.Instance().GetNearestBiome(array, PlayerBehavior.LocalPlayer.CurrentPosition);
		return nearestBiome != Vector3.zero;
	}

	private bool CheckCompleted(string[] arguments)
	{
		if (arguments.Length < 1)
		{
			return false;
		}
		return _completedEvents.IndexOf(arguments[0]) >= 0;
	}

	private void ParseFunction(string condition, out string funcName, out string[] arguments)
	{
		int num = condition.IndexOf('(') + 1;
		int length = condition.IndexOf(')') - num;
		string text = condition.Substring(num, length);
		arguments = text.Split(',');
		for (int i = 0; i < arguments.Length; i++)
		{
			arguments[i] = arguments[i].Trim();
		}
		funcName = condition.Substring(0, num - 1).Trim();
	}

	private bool ExecuteFunction(string funcName, string[] arguments)
	{
		bool result = true;
		switch (funcName)
		{
		case "check_equip_tag":
			result = CheckEquipTag(arguments);
			break;
		case "check_time":
			result = CheckTime(arguments);
			break;
		case "check_biome":
			result = CheckBiome(arguments);
			break;
		case "check_completed":
			result = CheckCompleted(arguments);
			break;
		}
		return result;
	}

	private bool ParseAndExecute(string condition)
	{
		ParseFunction(condition, out var funcName, out var arguments);
		return ExecuteFunction(funcName, arguments);
	}

	public void CompleteAllEvents()
	{
		while (_currentEvent != BlankEvent)
		{
			CompleteCurrentEvent();
		}
		Command.Event_UnLockPlayerMove();
	}

	public void CompleteCurrentEvent()
	{
		CompleteEvent(_currentEvent);
	}

	public void CompleteEvent([NotNull] GuideEvent guideEvent)
	{
		if (guideEvent == BlankEvent)
		{
			return;
		}
		ToDoCollection toDoCollection = ((guideEvent.FlowStack == null) ? null : guideEvent.FlowStack.Notify);
		if (toDoCollection != null)
		{
			toDoCollection.Clicked();
			return;
		}
		if (_currentEvent == guideEvent && this.PostEventSet != null)
		{
			this.PostEventSet(null, null);
		}
		if (guideEvent.IsInstant)
		{
			if (this.InstantGuideCompleted != null)
			{
				this.InstantGuideCompleted();
			}
			RestoreCurrentEvent();
		}
		else
		{
			_completedEvents.Add(guideEvent.Name);
			CompleteEventToDo(guideEvent);
			RemoveHelperTargets(guideEvent);
			MoveToNextEvent(guideEvent.FlowStack);
		}
	}

	private void RemoveFlowCondition(string conditionName)
	{
		for (int num = _remainFlowConditions.Count - 1; num >= 0; num--)
		{
			FlowCondition flowCondition = _remainFlowConditions[num];
			if (flowCondition.Name == conditionName)
			{
				flowCondition.TryUnregister();
				_remainFlowConditions.RemoveAt(num);
			}
		}
	}

	private string DoMoveNextFlow(FlowStack flowStack, GuideRecoder recoder, bool forceCondition = false, bool condition = false)
	{
		string text = null;
		FlowData flowData = null;
		while (flowStack.Stack.Count > 0)
		{
			FlowStackItem flowStackItem = flowStack.Stack.Peek();
			flowStackItem.MoveNext();
			flowData = flowStackItem.GetCurrent();
			if (flowData == null)
			{
				flowStack.Stack.Pop();
				continue;
			}
			break;
		}
		if (flowData != null)
		{
			if (string.IsNullOrEmpty(flowData.Cond))
			{
				text = flowData.Name;
				if (recoder != null)
				{
					if (recoder.IsRecordingEnabled)
					{
						recoder.Record(text);
					}
					else if (text == "very_beginning")
					{
						recoder.IsRecordingEnabled = true;
					}
				}
			}
			else
			{
				bool flag = ((!forceCondition) ? ParseAndExecute(flowData.Cond) : condition);
				text = ((!flag) ? "false" : "true");
				recoder?.Record(text);
				if (string.IsNullOrEmpty(flowData.TrueFlow) && string.IsNullOrEmpty(flowData.FalseFlow))
				{
					FlowStackItem flowStackItem = new FlowStackItem((!flag) ? flowData.FalseList : flowData.TrueList);
					flowStack.Stack.Push(flowStackItem);
				}
				else
				{
					string text2 = ((!flag) ? flowData.FalseFlow : flowData.TrueFlow);
					if (!string.IsNullOrEmpty(text2))
					{
						FlowContainer flowContainer = FindFlowContainer(text2);
						if (flowContainer != null)
						{
							FlowStackItem flowStackItem = new FlowStackItem(flowContainer);
							flowStack.Stack.Push(flowStackItem);
						}
					}
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "blank";
			recoder?.Record(text);
		}
		return text;
	}

	private bool IsConditionEvent(string value)
	{
		return value == "true" || value == "false";
	}

	private void MoveToNextEvent(FlowStack flowStack)
	{
		string text;
		do
		{
			text = DoMoveNextFlow(flowStack, flowStack.Recoder);
		}
		while (IsConditionEvent(text));
		StartEvent(flowStack, text);
		SaveGuideProgress();
	}

	private void ProcessDelayedEventQueue()
	{
		if (_delayedEventQueue.Count > 0)
		{
			GuideEvent currentEvent = _delayedEventQueue.Dequeue();
			SetCurrentEvent(currentEvent);
		}
	}

	public void OnGuideMsgFinished()
	{
		if (_currentEvent == BlankEvent)
		{
			ProcessDelayedEventQueue();
			return;
		}
		ToDoCollection toDoCollection = _currentEvent.ToDoCollection;
		if (toDoCollection != null && !GuideEvent.CheckAllToDoCompleted(toDoCollection))
		{
			ProcessDelayedEventQueue();
		}
		else
		{
			CompleteCurrentEvent();
		}
	}

	public void ReloadFlow(string flowName)
	{
		FlowStack flowStack = FindFlowStack(flowName);
		if (flowStack == null)
		{
			return;
		}
		RemoveFlowRelated(flowStack);
		FlowStack flowStack2 = CreateFlowStack(flowName);
		if (flowStack2 == null)
		{
			return;
		}
		for (int i = 0; i < _flowStacks.Count; i++)
		{
			if (_flowStacks[i].Name == flowName)
			{
				_flowStacks[i] = flowStack2;
				break;
			}
		}
		if (_currentEvent.FlowStack == flowStack)
		{
			_currentEvent.FlowStack = null;
			BlankEvent.FlowStack = null;
			SetCurrentEvent(BlankEvent);
		}
	}

	private void RemoveFlowRelated(FlowStack flowStack)
	{
		if (flowStack.Stack.Count == 0)
		{
			return;
		}
		FlowStackItem flowStackItem = flowStack.Stack.Peek();
		if (flowStackItem == null)
		{
			return;
		}
		FlowData current = flowStackItem.GetCurrent();
		if (current != null)
		{
			GuideEvent guideEvent = FindEvent(current.Name);
			if (guideEvent != null)
			{
				RemoveEventToDo(guideEvent);
				RemoveHelperTargets(guideEvent);
			}
		}
	}

	public bool IsFlowBegin(string flowName)
	{
		return FindFlowStack(flowName)?.Started ?? false;
	}

	public void BeginFlow(string flowName, bool canMoveToNext = true)
	{
		FlowStack flowStack = FindFlowStack(flowName);
		if (flowStack != null && !flowStack.Started)
		{
			if (flowStack.Stack.Count == 0)
			{
				SetCurrentEvent(BlankEvent);
			}
			else
			{
				FlowStackItem flowStackItem = flowStack.Stack.Peek();
				if (flowStackItem == null)
				{
					SetCurrentEvent(BlankEvent);
				}
				else
				{
					FlowData current = flowStackItem.GetCurrent();
					if (current != null)
					{
						flowStack.Started = true;
						StartEvent(flowStack, current.Name);
					}
					else
					{
						if (!canMoveToNext)
						{
							return;
						}
						flowStack.Started = true;
						MoveToNextEvent(flowStack);
					}
				}
			}
		}
		RemoveFlowCondition(flowName);
	}

	public void EventOccured(string type, string param)
	{
		if (this.ExternalEventOccured != null)
		{
			this.ExternalEventOccured(type, param);
		}
	}

	private void TerrainA6_OnLoadingChunksFinished()
	{
		if (IsGuideBegin)
		{
			RefreshHelperTarget();
		}
		else
		{
			ProcessHideOtherPlayer();
			BeginNormalFlow();
			for (int num = _remainFlowConditions.Count - 1; num >= 0; num--)
			{
				BeginFlow(_remainFlowConditions[num].Name, canMoveToNext: false);
			}
			for (int num2 = _remainFlowConditions.Count - 1; num2 >= 0; num2--)
			{
				FlowCondition flowCondition = _remainFlowConditions[num2];
				flowCondition.TryRegister();
			}
			IsGuideBegin = true;
		}
		if (_prevGuideRole == Role.Risky && _currentGuideRole == Role.Safe && this.ReturnFromUnstable != null)
		{
			this.ReturnFromUnstable();
		}
	}

	private void ProcessHideOtherPlayer()
	{
		if (_currentGuideRole != Role.Tutorial)
		{
			return;
		}
		bool hide = true;
		for (int i = 0; i < _completedEvents.Count; i++)
		{
			GuideEvent guideEvent = FindEvent(_completedEvents[i]);
			if (guideEvent != null && guideEvent.CustomCommand != null && guideEvent.CustomCommand.Contains("Event_ShowOtherPlayer"))
			{
				hide = false;
				break;
			}
		}
		KSingleton<PlayerManager>.Instance().HideOtherPlayer(hide);
	}

	private void BeginNormalFlow()
	{
		BeginFlow("normal_flow");
	}
}
