using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Network;
using Durango.UI;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Quest;
using UnityEngine;

namespace Durango.Terrain;

public class GlobalLandmarks : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CLoadGlobalLandmarks_003Ed__8 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public GlobalLandmarks _003C_003E4__this;

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
		public _003CLoadGlobalLandmarks_003Ed__8(int _003C_003E1__state)
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
			GlobalLandmarks globalLandmarks = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				for (int i = 0; i < globalLandmarks._landmarks.Count; i++)
				{
					UnityEngine.Object.Destroy(globalLandmarks._landmarks[i]);
				}
				globalLandmarks._landmarks.Clear();
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (!TerrainBase.IsPlayerInitialized)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			List<LandmarkInfo> globalLandmarks2 = TerrainMeta.GlobalLandmarks;
			int j = 0;
			for (int count = globalLandmarks2.Count; j < count; j++)
			{
				LandmarkInfo info = globalLandmarks2[j];
				globalLandmarks.AddLandmark(info);
			}
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

	private static readonly string[] StoryNavigateKey = new string[2] { "landmark.story_k", "landmark.story_t" };

	private static readonly string[] StoryModelPath = new string[2] { "Models/NPC/F_NPC_K_Story.prefab", "Models/NPC/NPC_AgentMale_Story.prefab" };

	private static readonly LandmarkInfo[,] StoryInfo = new LandmarkInfo[2, 5]
	{
		{
			new LandmarkInfo
			{
				X = 68,
				Y = 73,
				Rotate = 90
			},
			new LandmarkInfo
			{
				X = 176,
				Y = 60,
				Rotate = 90
			},
			new LandmarkInfo
			{
				X = 153,
				Y = 36,
				Rotate = 90
			},
			new LandmarkInfo
			{
				X = 160,
				Y = 60,
				Rotate = 90
			},
			new LandmarkInfo
			{
				X = 79,
				Y = 38,
				Rotate = 90
			}
		},
		{
			new LandmarkInfo
			{
				X = 73,
				Y = 73,
				Rotate = 90
			},
			new LandmarkInfo
			{
				X = 171,
				Y = 60,
				Rotate = 90
			},
			new LandmarkInfo
			{
				X = 148,
				Y = 36,
				Rotate = 90
			},
			new LandmarkInfo
			{
				X = 155,
				Y = 60,
				Rotate = 90
			},
			new LandmarkInfo
			{
				X = 79,
				Y = 43,
				Rotate = 90
			}
		}
	};

	private static readonly string[] LandmarkNames = new string[2] { "ST_train_wreckage_01", "ST_highway_01" };

	private readonly List<GameObject> _landmarks = new List<GameObject>();

	public static bool IsKindOfGlobalLandmark(string path)
	{
		int i = 0;
		for (int num = LandmarkNames.Length; i < num; i++)
		{
			string value = LandmarkNames[i];
			if (path.Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		StartCoroutine(LoadGlobalLandmarks());
		Singleton<GameManager>.Instance().PostReconnect += delegate
		{
			ClearStoryModel();
			StartCoroutine(LoadGlobalLandmarks());
		};
		Connections.Frontend.On(delegate(AppearEpicNPC msg, PacketHeader header)
		{
			ClearStoryModel();
			for (int i = 0; i < KUtility.GetSize(msg.Npcs); i++)
			{
				EpicNPC msg2 = msg.Npcs[i];
				LoadStoryModel(msg2);
			}
		});
	}

	private void ClearStoryModel()
	{
		UnloadStoryModel(EpicNPCType.K);
		UnloadStoryModel(EpicNPCType.T);
	}

	private IEnumerator LoadGlobalLandmarks()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLoadGlobalLandmarks_003Ed__8(0)
		{
			_003C_003E4__this = this
		};
	}

	private void AddLandmark(LandmarkInfo info)
	{
		string landmarkPrefab = TerrainMeta.GetLandmarkPrefab(info.Id);
		AddLandMark(landmarkPrefab, info);
	}

	private void AddLandMark(string prefabName, LandmarkInfo info, Action<GameObject> onLoad = null)
	{
		Singleton<AssetBundleManager>.Instance().RequestAsset(prefabName, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null))
			{
				GameObject obj = (GameObject)UnityEngine.Object.Instantiate(asset);
				SetLandmark(obj, info);
				if (onLoad != null)
				{
					onLoad(obj);
				}
			}
		});
	}

	private void LoadStoryModel(EpicNPC msg)
	{
		if (msg.Npc == EpicNPCType.Invalid)
		{
			return;
		}
		string templateId = GameManager.Region.TemplateId;
		int num = templateId.Substring(templateId.Length - 1, 1).ToInt() - 1;
		if (num < 0 || num >= StoryInfo.GetLength(1))
		{
			num = 0;
		}
		int npc = (int)msg.Npc;
		UnloadStoryModel(msg.Npc);
		LandmarkInfo landmarkInfo = StoryInfo[npc, num];
		AddLandMark(StoryModelPath[npc], landmarkInfo, delegate(GameObject o)
		{
			if (!(o == null))
			{
				ClientInteractionQuest component = o.GetComponent<ClientInteractionQuest>();
				if ((bool)component)
				{
					component.Need = msg.Need;
					component.Interaction = msg.Interaction;
				}
			}
		});
		Vector3 vector = Util.TilePositionToClientPosition(new Vector2((int)landmarkInfo.X, (int)landmarkInfo.Y));
		Vector3 vector2 = new Vector3(landmarkInfo.OffsetX, landmarkInfo.OffsetY, landmarkInfo.OffsetZ);
		vector2.x += 100f;
		vector2.z += 100f;
		UIManager.FindScript<NavigateGroup>().Point.SetTarget(StoryNavigateKey[npc], new PointTargetController.Arguments
		{
			Position = vector + vector2,
			Icon = ((msg.Npc != 0) ? "todo_icon_npc_agent" : "todo_icon_npc_TheFirm"),
			HideInScreen = true
		});
	}

	private void UnloadStoryModel(EpicNPCType type)
	{
		string value = ((type != 0) ? "NPC_AgentMale_Story" : "F_NPC_K_Story");
		for (int num = _landmarks.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = _landmarks[num];
			if (gameObject.name.Contains(value))
			{
				UnityEngine.Object.Destroy(gameObject);
				_landmarks.RemoveAt(num);
			}
		}
		UIManager.FindScript<NavigateGroup>().Point.ClearTarget(StoryNavigateKey[(int)type]);
	}

	private void SetLandmark([NotNull] GameObject obj, LandmarkInfo info)
	{
		Vector2 tilePosition = new Vector2((int)info.X, (int)info.Y);
		Vector3 vector = new Vector3(info.OffsetX, info.OffsetY, info.OffsetZ);
		vector.x += 100f;
		vector.z += 100f;
		Vector3 position = Util.TilePositionToClientPosition(tilePosition);
		position += vector;
		obj.transform.parent = base.transform;
		obj.transform.localRotation = Quaternion.Euler(0f, info.Rotate * 2, 0f);
		obj.transform.position = position;
		_landmarks.Add(obj);
	}
}
