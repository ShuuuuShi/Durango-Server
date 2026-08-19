using TerrainData;
using UnityEngine;

public class ChunkData
{
	public enum WaterDataType
	{
		Blank,
		Filled,
		Error
	}

	private const int WaterDataStride = 17;

	private static ChunkData _borderChunkData;

	private WaterDataType _waterDataType;

	private WaterData _waterData;

	private WaterDataType _riverDataType;

	private RiverData _riverData;

	public Vector2 Coords { get; set; }

	public byte[] TileBiomeData { get; set; }

	public NaturalInfo[] NaturalData { get; set; }

	public LandmarkInfo[] LandmarkData { get; set; }

	public static ChunkData GetBorderChunk()
	{
		if (_borderChunkData != null)
		{
			return _borderChunkData;
		}
		_borderChunkData = new ChunkData();
		byte[] array = new byte[324];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = 12;
		}
		_borderChunkData.TileBiomeData = array;
		byte[] array2 = new byte[289];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = 127;
		}
		_borderChunkData.SetWaterData(array2);
		return _borderChunkData;
	}

	public WaterData GetWaterData()
	{
		if (_waterData != null && _waterDataType == WaterDataType.Filled)
		{
			return _waterData;
		}
		return null;
	}

	public void SetWaterData(byte[] byteData)
	{
		switch (byteData.Length)
		{
		case 1:
			_waterData = null;
			if (byteData[0] == 0)
			{
				_waterDataType = WaterDataType.Blank;
				break;
			}
			Debug.LogError((object)("Ocean Data Error: error code " + byteData[0]));
			_waterDataType = WaterDataType.Error;
			break;
		case 0:
			_waterDataType = WaterDataType.Blank;
			break;
		default:
			if (byteData.Length != 289)
			{
				Debug.LogError((object)$"Ocean Data Error: Invalid Size ({byteData.Length})");
				_waterDataType = WaterDataType.Error;
			}
			else
			{
				_waterDataType = WaterDataType.Filled;
				_waterData = new WaterData(17, 17, byteData, 17);
			}
			break;
		}
	}

	public RiverData GetRiverData()
	{
		if (_riverData != null && _riverDataType == WaterDataType.Filled)
		{
			return _riverData;
		}
		return null;
	}

	public void SetRiverData(byte[] riverData)
	{
		_riverData = null;
		switch (riverData.Length)
		{
		case 1:
			if (riverData[0] == 0)
			{
				_riverDataType = WaterDataType.Blank;
				return;
			}
			Debug.LogError((object)("River Data Error: error code " + riverData[0]));
			_riverDataType = WaterDataType.Error;
			return;
		case 0:
			_riverDataType = WaterDataType.Blank;
			return;
		}
		int num = Mathf.RoundToInt(Mathf.Sqrt((float)(riverData.Length / 3)));
		int num2 = num * num * 3;
		if (num2 != riverData.Length)
		{
			Debug.LogError((object)$"Invalid River Data: Length {riverData.Length} ({num2} bytes expected)");
			_riverDataType = WaterDataType.Error;
		}
		else
		{
			_riverDataType = WaterDataType.Filled;
			_riverData = new RiverData(num, num, riverData);
		}
	}
}
