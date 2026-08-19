using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.Render.PersonalMaps;

public class PersonalMaps : Singleton<PersonalMaps>
{
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
		IsWorking = true;
		IsCanceled = false;
		UnityEngine.Camera mainCamera = Singleton<MainCamera>.Instance().GetComponent<UnityEngine.Camera>();
		UnityEngine.Camera overlayCamera = Singleton<OverlayCamera>.Instance().GetComponent<UnityEngine.Camera>();
		Vector3 playerPosition = PlayerBehavior.LocalPlayer.transform.position;
		MemoryStream memoryStream = null;
		using (BitmapCaptor bitmapCaptor = new BitmapCaptor(mainCamera))
		{
			PersonalMapsSetting.ApplyCaptureSettings(mainCamera, captureMode: true);
			yield return null;
			GetCaptureArea(minTile, maxTile, out var humaneLeftBottom, out var humaneRightTop);
			GetScreenPosInterval(mainCamera, bitmapCaptor.Width, bitmapCaptor.Height, out var worldIntervalX, out var worldIntervalY);
			int xTotalStep;
			int yTotalStep;
			Vector3 worldPosStart = GetTotalCaptureSteps(humaneLeftBottom, humaneRightTop, worldIntervalX, worldIntervalY, out xTotalStep, out yTotalStep);
			int currentProgress = 0;
			int totalProgress = xTotalStep * yTotalStep;
			int imageWidth = bitmapCaptor.Width * xTotalStep;
			int imageHeight = bitmapCaptor.Height * yTotalStep;
			int bitmapWidth = imageWidth;
			int bitmapHeight = bitmapCaptor.Height;
			byte[] bitmapBuffer = new byte[bitmapWidth * bitmapHeight * 3];
			JpegCompressor jpegCompressor2 = JpegCompressor.Create(imageWidth, imageHeight, 90);
			for (int y = 0; y < yTotalStep; y++)
			{
				if (IsCanceled)
				{
					break;
				}
				if (jpegCompressor2 == null)
				{
					break;
				}
				Array.Clear(bitmapBuffer, 0, bitmapBuffer.Length);
				Vector3 posY = (yTotalStep - (y + 1)) * worldIntervalY;
				for (int x = 0; x < xTotalStep; x++)
				{
					if (IsCanceled)
					{
						break;
					}
					Vector3 currentPos = worldPosStart + x * worldIntervalX + posY;
					Point2 chunkCoords = Util.WorldPositionToChunkCoords(currentPos);
					if (0 <= chunkCoords.x && chunkCoords.x < TerrainMeta.ChunkCount && 0 <= chunkCoords.y && chunkCoords.y < TerrainMeta.ChunkCount)
					{
						yield return CoSetPositionAndWaiting(Util.WorldPositionToClientPosition(currentPos));
						yield return new WaitForEndOfFrame();
						bitmapCaptor.Capture(bitmapBuffer, bitmapCaptor.Width * x, 0, bitmapWidth, 3, overlayCamera);
					}
					if (onProgress != null)
					{
						int num;
						currentProgress = (num = currentProgress + 1);
						onProgress((float)num / (float)totalProgress);
					}
				}
				int i = (bitmapHeight - 1) * bitmapWidth;
				while (i >= 0 && !IsCanceled && jpegCompressor2 != null)
				{
					if (!jpegCompressor2.AddRow(bitmapBuffer, i * 3))
					{
						jpegCompressor2.Release();
						jpegCompressor2 = null;
					}
					yield return null;
					i -= bitmapWidth;
				}
			}
			PersonalMapsSetting.ApplyCaptureSettings(mainCamera, captureMode: false);
			yield return null;
			if (IsCanceled)
			{
				if (jpegCompressor2 != null)
				{
					jpegCompressor2.Release();
					jpegCompressor2 = null;
				}
				onProgress?.Invoke(null);
				yield return CoSetPositionAndWaiting(playerPosition, 2f);
			}
			else
			{
				if (jpegCompressor2 != null)
				{
					memoryStream = jpegCompressor2.Finish();
				}
				yield return CoSetPositionAndWaiting(playerPosition);
			}
			PlayerBehavior.LocalPlayer.SetVisible(visible: true);
		}
		onResult?.Invoke(memoryStream);
		IsWorking = false;
	}

	private IEnumerator CoSetPositionAndWaiting(Vector3 pos, float waitForSeconds = 0f)
	{
		PlayerBehavior.LocalPlayer.transform.position = pos;
		PlayerBehavior.LocalPlayer.SetVisible(visible: false);
		yield return null;
		if (waitForSeconds != 0f)
		{
			yield return new WaitForSeconds(waitForSeconds);
			yield break;
		}
		while (!TerrainChunksAndArtifactsLoadingCompleted() && !IsCanceled)
		{
			yield return null;
		}
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
		Vector2 tile = humaneLeftBottom;
		tile -= vector4 * 0.5f;
		tile += vector3 * 0.5f;
		return MapPositionParser.HumaneTileToPosition(tile);
	}

	private static bool TerrainChunksAndArtifactsLoadingCompleted()
	{
		return Singleton<TerrainBase>.Instance().IsEnoughChunksLoaded() && ArtifactLoadingIsCompleted();
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
