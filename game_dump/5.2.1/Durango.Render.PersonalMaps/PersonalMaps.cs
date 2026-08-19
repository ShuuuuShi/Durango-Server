using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.Render.PersonalMaps;

public class PersonalMaps : Singleton<PersonalMaps>
{
	[CompilerGenerated]
	private sealed class _003CCoCapture_003Ed__11 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PersonalMaps _003C_003E4__this;

		public Point2 minTile;

		public Point2 maxTile;

		public Action<float?> onProgress;

		public Action<MemoryStream> onResult;

		private UnityEngine.Camera _003CmainCamera_003E5__2;

		private UnityEngine.Camera _003CoverlayCamera_003E5__3;

		private Vector3 _003CplayerPosition_003E5__4;

		private MemoryStream _003CmemoryStream_003E5__5;

		private BitmapCaptor _003CbitmapCaptor_003E5__6;

		private Vector3 _003CworldIntervalX_003E5__7;

		private Vector3 _003CworldIntervalY_003E5__8;

		private int _003CxTotalStep_003E5__9;

		private int _003CyTotalStep_003E5__10;

		private Vector3 _003CworldPosStart_003E5__11;

		private int _003CcurrentProgress_003E5__12;

		private int _003CtotalProgress_003E5__13;

		private int _003CbitmapWidth_003E5__14;

		private int _003CbitmapHeight_003E5__15;

		private byte[] _003CbitmapBuffer_003E5__16;

		private JpegCompressor _003CjpegCompressor2_003E5__17;

		private int _003Cy_003E5__18;

		private Vector3 _003CposY_003E5__19;

		private int _003Ci_003E5__20;

		private int _003Cx_003E5__21;

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
		public _003CCoCapture_003Ed__11(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || (uint)(num - 1) <= 6u)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003CmainCamera_003E5__2 = null;
			_003CoverlayCamera_003E5__3 = null;
			_003CmemoryStream_003E5__5 = null;
			_003CbitmapCaptor_003E5__6 = null;
			_003CbitmapBuffer_003E5__16 = null;
			_003CjpegCompressor2_003E5__17 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				PersonalMaps personalMaps = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					personalMaps.IsWorking = true;
					personalMaps.IsCanceled = false;
					_003CmainCamera_003E5__2 = Singleton<MainCamera>.Instance().GetComponent<UnityEngine.Camera>();
					_003CoverlayCamera_003E5__3 = Singleton<OverlayCamera>.Instance().GetComponent<UnityEngine.Camera>();
					_003CplayerPosition_003E5__4 = PlayerBehavior.LocalPlayer.transform.position;
					_003CmemoryStream_003E5__5 = null;
					_003CbitmapCaptor_003E5__6 = new BitmapCaptor(_003CmainCamera_003E5__2);
					_003C_003E1__state = -3;
					PersonalMapsSetting.ApplyCaptureSettings(_003CmainCamera_003E5__2, captureMode: true);
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				case 1:
				{
					_003C_003E1__state = -3;
					GetCaptureArea(minTile, maxTile, out var humaneLeftBottom, out var humaneRightTop);
					GetScreenPosInterval(_003CmainCamera_003E5__2, _003CbitmapCaptor_003E5__6.Width, _003CbitmapCaptor_003E5__6.Height, out _003CworldIntervalX_003E5__7, out _003CworldIntervalY_003E5__8);
					_003CworldPosStart_003E5__11 = GetTotalCaptureSteps(humaneLeftBottom, humaneRightTop, _003CworldIntervalX_003E5__7, _003CworldIntervalY_003E5__8, out _003CxTotalStep_003E5__9, out _003CyTotalStep_003E5__10);
					_003CcurrentProgress_003E5__12 = 0;
					_003CtotalProgress_003E5__13 = _003CxTotalStep_003E5__9 * _003CyTotalStep_003E5__10;
					int width = _003CbitmapCaptor_003E5__6.Width * _003CxTotalStep_003E5__9;
					int height = _003CbitmapCaptor_003E5__6.Height * _003CyTotalStep_003E5__10;
					_003CbitmapWidth_003E5__14 = width;
					_003CbitmapHeight_003E5__15 = _003CbitmapCaptor_003E5__6.Height;
					_003CbitmapBuffer_003E5__16 = new byte[_003CbitmapWidth_003E5__14 * _003CbitmapHeight_003E5__15 * 3];
					_003CjpegCompressor2_003E5__17 = JpegCompressor.Create(width, height, 90);
					_003Cy_003E5__18 = 0;
					goto IL_0411;
				}
				case 2:
					_003C_003E1__state = -3;
					_003C_003E2__current = new WaitForEndOfFrame();
					_003C_003E1__state = 3;
					return true;
				case 3:
					_003C_003E1__state = -3;
					_003CbitmapCaptor_003E5__6.Capture(_003CbitmapBuffer_003E5__16, _003CbitmapCaptor_003E5__6.Width * _003Cx_003E5__21, 0, _003CbitmapWidth_003E5__14, 3, _003CoverlayCamera_003E5__3);
					goto IL_031b;
				case 4:
					_003C_003E1__state = -3;
					_003Ci_003E5__20 -= _003CbitmapWidth_003E5__14;
					goto IL_03e6;
				case 5:
					_003C_003E1__state = -3;
					if (personalMaps.IsCanceled)
					{
						if (_003CjpegCompressor2_003E5__17 != null)
						{
							_003CjpegCompressor2_003E5__17.Release();
							_003CjpegCompressor2_003E5__17 = null;
						}
						onProgress?.Invoke(null);
						_003C_003E2__current = personalMaps.CoSetPositionAndWaiting(_003CplayerPosition_003E5__4, 2f);
						_003C_003E1__state = 6;
						return true;
					}
					if (_003CjpegCompressor2_003E5__17 != null)
					{
						_003CmemoryStream_003E5__5 = _003CjpegCompressor2_003E5__17.Finish();
					}
					_003C_003E2__current = personalMaps.CoSetPositionAndWaiting(_003CplayerPosition_003E5__4);
					_003C_003E1__state = 7;
					return true;
				case 6:
					_003C_003E1__state = -3;
					break;
				case 7:
					{
						_003C_003E1__state = -3;
						break;
					}
					IL_0411:
					if (_003Cy_003E5__18 < _003CyTotalStep_003E5__10 && !personalMaps.IsCanceled && _003CjpegCompressor2_003E5__17 != null)
					{
						Array.Clear(_003CbitmapBuffer_003E5__16, 0, _003CbitmapBuffer_003E5__16.Length);
						_003CposY_003E5__19 = (_003CyTotalStep_003E5__10 - (_003Cy_003E5__18 + 1)) * _003CworldIntervalY_003E5__8;
						_003Cx_003E5__21 = 0;
						goto IL_0361;
					}
					PersonalMapsSetting.ApplyCaptureSettings(_003CmainCamera_003E5__2, captureMode: false);
					_003C_003E2__current = null;
					_003C_003E1__state = 5;
					return true;
					IL_03e6:
					if (_003Ci_003E5__20 >= 0 && !personalMaps.IsCanceled && _003CjpegCompressor2_003E5__17 != null)
					{
						if (!_003CjpegCompressor2_003E5__17.AddRow(_003CbitmapBuffer_003E5__16, _003Ci_003E5__20 * 3))
						{
							_003CjpegCompressor2_003E5__17.Release();
							_003CjpegCompressor2_003E5__17 = null;
						}
						_003C_003E2__current = null;
						_003C_003E1__state = 4;
						return true;
					}
					_003Cy_003E5__18++;
					goto IL_0411;
					IL_031b:
					if (onProgress != null)
					{
						int num2 = ++_003CcurrentProgress_003E5__12;
						onProgress((float)num2 / (float)_003CtotalProgress_003E5__13);
					}
					_003Cx_003E5__21++;
					goto IL_0361;
					IL_0361:
					if (_003Cx_003E5__21 < _003CxTotalStep_003E5__9 && !personalMaps.IsCanceled)
					{
						Vector3 worldPosition = _003CworldPosStart_003E5__11 + _003Cx_003E5__21 * _003CworldIntervalX_003E5__7 + _003CposY_003E5__19;
						Point2 point = Util.WorldPositionToChunkCoords(worldPosition);
						if (0 <= point.x && point.x < TerrainMeta.ChunkCount && 0 <= point.y && point.y < TerrainMeta.ChunkCount)
						{
							_003C_003E2__current = personalMaps.CoSetPositionAndWaiting(Util.WorldPositionToClientPosition(worldPosition));
							_003C_003E1__state = 2;
							return true;
						}
						goto IL_031b;
					}
					_003Ci_003E5__20 = (_003CbitmapHeight_003E5__15 - 1) * _003CbitmapWidth_003E5__14;
					goto IL_03e6;
				}
				PlayerBehavior.LocalPlayer.SetVisible(visible: true);
				_003CbitmapBuffer_003E5__16 = null;
				_003CjpegCompressor2_003E5__17 = null;
				_003C_003Em__Finally1();
				_003CbitmapCaptor_003E5__6 = null;
				onResult?.Invoke(_003CmemoryStream_003E5__5);
				personalMaps.IsWorking = false;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003CbitmapCaptor_003E5__6 != null)
			{
				((IDisposable)_003CbitmapCaptor_003E5__6).Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCoSetPositionAndWaiting_003Ed__12 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Vector3 pos;

		public float waitForSeconds;

		public PersonalMaps _003C_003E4__this;

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
		public _003CCoSetPositionAndWaiting_003Ed__12(int _003C_003E1__state)
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
			PersonalMaps personalMaps = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				PlayerBehavior.LocalPlayer.transform.position = pos;
				PlayerBehavior.LocalPlayer.SetVisible(visible: false);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (waitForSeconds != 0f)
				{
					_003C_003E2__current = new WaitForSeconds(waitForSeconds);
					_003C_003E1__state = 2;
					return true;
				}
				break;
			case 2:
				_003C_003E1__state = -1;
				return false;
			case 3:
				_003C_003E1__state = -1;
				break;
			}
			if (!TerrainChunksAndArtifactsLoadingCompleted() && !personalMaps.IsCanceled)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 3;
				return true;
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

	private const int _jpegQuality = 90;

	public bool IsWorking { get; private set; }

	public bool IsCanceled { get; private set; }

	public void Capture(Point2 minTile, Point2 maxTile, Action<float?> onProgress, Action<MemoryStream> onResult)
	{
		if (!IsWorking)
		{
			StartCoroutine(CoCapture(minTile, maxTile, onProgress, onResult));
		}
	}

	public void Cancel()
	{
		if (IsWorking)
		{
			IsCanceled = true;
		}
	}

	private IEnumerator CoCapture(Point2 minTile, Point2 maxTile, Action<float?> onProgress, Action<MemoryStream> onResult)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoCapture_003Ed__11(0)
		{
			_003C_003E4__this = this,
			minTile = minTile,
			maxTile = maxTile,
			onProgress = onProgress,
			onResult = onResult
		};
	}

	private IEnumerator CoSetPositionAndWaiting(Vector3 pos, float waitForSeconds = 0f)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoSetPositionAndWaiting_003Ed__12(0)
		{
			_003C_003E4__this = this,
			pos = pos,
			waitForSeconds = waitForSeconds
		};
	}

	private static void GetCaptureArea(Point2 minTile, Point2 maxTile, out Vector2 humaneLeftBottom, out Vector2 humaneRightTop)
	{
		Vector3 pos = Util.TilePositionToWorldPosition(new Point2(minTile.x, minTile.y));
		Vector3 pos2 = Util.TilePositionToWorldPosition(new Point2(maxTile.x, minTile.y));
		Vector3 pos3 = Util.TilePositionToWorldPosition(new Point2(minTile.x, maxTile.y));
		Vector3 pos4 = Util.TilePositionToWorldPosition(new Point2(maxTile.x, maxTile.y));
		Vector2 vector = MapPositionParser.PositionToHumaneTile(pos);
		Vector2 vector2 = MapPositionParser.PositionToHumaneTile(pos2);
		Vector2 vector3 = MapPositionParser.PositionToHumaneTile(pos3);
		Vector2 vector4 = MapPositionParser.PositionToHumaneTile(pos4);
		humaneLeftBottom = new Vector2(vector3.x, vector.y);
		humaneRightTop = new Vector2(vector2.x, vector4.y);
	}

	private static void GetScreenPosInterval(UnityEngine.Camera camera, int captureWidth, int captureHeight, out Vector3 worldIntervalX, out Vector3 worldIntervalY)
	{
		Vector3 vector = ScreenPosToTerrainWorldPos(camera, new Vector3((float)captureWidth * 0.5f, (float)captureHeight * 0.5f));
		Vector3 vector2 = ScreenPosToTerrainWorldPos(camera, new Vector3((float)captureWidth * 1.5f, (float)captureHeight * 0.5f));
		Vector3 vector3 = ScreenPosToTerrainWorldPos(camera, new Vector3((float)captureWidth * 0.5f, (float)captureHeight * 1.5f));
		worldIntervalX = vector2 - vector;
		worldIntervalY = vector3 - vector;
	}

	private static Vector3 ScreenPosToTerrainWorldPos(UnityEngine.Camera camera, Vector3 unityPos)
	{
		Ray ray = camera.ScreenPointToRay(unityPos);
		unityPos = ray.origin - (ray.origin.y - 0.1f) / ray.direction.y * ray.direction;
		return unityPos;
	}

	private static Vector3 GetTotalCaptureSteps(Vector2 humaneLeftBottom, Vector2 humaneRightTop, Vector3 worldIntervalX, Vector3 worldIntervalY, out int xTotalStep, out int yTotalStep)
	{
		Vector3 vector = MapPositionParser.HumaneTileToPosition(humaneLeftBottom);
		Vector2 vector2 = new Vector2(MapPositionParser.PositionToHumaneTile(vector + worldIntervalX).x, MapPositionParser.PositionToHumaneTile(vector + worldIntervalY).y);
		Vector2 vector3 = new Vector2(vector2.x - humaneLeftBottom.x, vector2.y - humaneLeftBottom.y);
		float num = humaneRightTop.x - humaneLeftBottom.x;
		float num2 = humaneRightTop.y - humaneLeftBottom.y;
		xTotalStep = Mathf.Max(Mathf.CeilToInt((num + vector3.x) / vector3.x), 1);
		yTotalStep = Mathf.Max(Mathf.CeilToInt((num2 + vector3.y) / vector3.y), 1);
		Vector2 vector4 = new Vector2(vector3.x * (float)xTotalStep - num, vector3.y * (float)yTotalStep - num2);
		return MapPositionParser.HumaneTileToPosition(humaneLeftBottom - vector4 * 0.5f + vector3 * 0.5f);
	}

	private static bool TerrainChunksAndArtifactsLoadingCompleted()
	{
		if (Singleton<TerrainBase>.Instance().IsEnoughChunksLoaded())
		{
			return ArtifactLoadingIsCompleted();
		}
		return false;
	}

	private static bool ArtifactLoadingIsCompleted()
	{
		PersonalIslandInfo? personalIslandInfo = GameSystem<EstateSystem>.Instance().PersonalIslandInfo;
		if (personalIslandInfo.HasValue)
		{
			Dictionary<Point2, string[]> artifactsByChunks = personalIslandInfo.Value.ArtifactsByChunks;
			Point2 centerChunkCoords = Singleton<TerrainBase>.Instance().CenterChunkCoords;
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					Point2 key = centerChunkCoords + new Point2(i, j);
					if (!artifactsByChunks.TryGetValue(key, out var value))
					{
						continue;
					}
					string[] array = value;
					foreach (string entityId in array)
					{
						if (Singleton<ArtifactManager>.Instance().Find(entityId) == null)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}
}
