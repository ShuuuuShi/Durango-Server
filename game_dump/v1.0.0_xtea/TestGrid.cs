using UnityEngine;

public class TestGrid : MonoBehaviour
{
	[SerializeField]
	private GameObject _tileGridPrefab;

	private GameObject _tileGrid;

	private float _hideAt;

	private bool _isInit;

	private void Awake()
	{
		HideGrid();
	}

	private void OnEnable()
	{
		if (KSingleton<TerrainA6>.HasInstance())
		{
			Init();
			if ((Object)(object)_tileGrid != (Object)null)
			{
				_tileGrid.SetActive(true);
			}
			_hideAt = ((!(_hideAt > Time.time)) ? 0f : _hideAt);
			KSingleton<TerrainA6>.Instance().LoadingChunksFinished += OnChunkLoadFinish;
		}
		else
		{
			HideGrid();
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)_tileGrid))
		{
			_tileGrid.SetActive(false);
		}
		if (KSingleton<TerrainA6>.HasInstance())
		{
			KSingleton<TerrainA6>.Instance().LoadingChunksFinished -= OnChunkLoadFinish;
		}
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tileGrid = ((Component)this).gameObject.AddChild(_tileGridPrefab);
		}
	}

	private void OnChunkLoadFinish()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_tileGrid == (Object)null)
		{
			HideGrid();
			return;
		}
		Vector3 clientPosition = ((!((Object)(object)PlayerBehavior.LocalPlayer == (Object)null)) ? PlayerBehavior.LocalPlayer.CurrentPosition : Vector3.zero);
		TerrainChunkA6 terrainChunk = KSingleton<TerrainA6>.Instance().GetTerrainChunk(TerrainA6.ClientPositionToChunkCoords(clientPosition));
		if ((Object)(object)terrainChunk == (Object)null)
		{
			HideGrid();
		}
		else
		{
			_tileGrid.transform.position = ((Component)terrainChunk).transform.position;
		}
	}

	private void Update()
	{
		if (0f < _hideAt && _hideAt < Time.time)
		{
			HideGrid();
		}
	}

	public void ShowGrid(float duration = 0f)
	{
		if (!((Behaviour)this).enabled)
		{
			_hideAt = ((!(duration > 0f)) ? 0f : (Time.time + duration));
			((Behaviour)this).enabled = true;
		}
	}

	public void HideGrid()
	{
		if (((Behaviour)this).enabled)
		{
			((Behaviour)this).enabled = false;
		}
	}
}
