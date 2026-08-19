using System.Collections.Generic;
using UnityEngine;

public class KSpritePool : MonoBehaviour
{
	private const int MaxSubPoolCount = 8;

	public int CurSpriteCount;

	public int CurSubPoolIndex;

	private bool _selectable;

	private int _poolSize;

	private readonly List<KSpriteSubPool> _kSpriteSubPoolList = new List<KSpriteSubPool>(8);

	public bool Initialized { get; private set; }

	public void Init(bool selectable, int poolSize)
	{
		Initialized = true;
		_selectable = selectable;
		_poolSize = poolSize;
		_kSpriteSubPoolList.Add(CreateSubPool());
		ResetSprites();
	}

	public void ResetSprites()
	{
		for (int i = 0; i < _kSpriteSubPoolList.Count; i++)
		{
			KSpriteSubPool kSpriteSubPool = _kSpriteSubPoolList[i];
			kSpriteSubPool.ResetSprites();
		}
		CurSpriteCount = 0;
		CurSubPoolIndex = 0;
	}

	public KSprite GetNextKSprite()
	{
		KSpriteSubPool kSpriteSubPool = _kSpriteSubPoolList[CurSubPoolIndex];
		if (kSpriteSubPool.CurSpriteCount == kSpriteSubPool.GetPoolSize())
		{
			CurSubPoolIndex++;
			if (CurSubPoolIndex == _kSpriteSubPoolList.Count)
			{
				kSpriteSubPool = CreateSubPool();
				_kSpriteSubPoolList.Add(kSpriteSubPool);
			}
			else
			{
				kSpriteSubPool = _kSpriteSubPoolList[CurSubPoolIndex];
			}
		}
		CurSpriteCount++;
		return kSpriteSubPool.GetNextSprite();
	}

	private KSpriteSubPool CreateSubPool()
	{
		return new KSpriteSubPool(_poolSize, _selectable, ((Component)this).gameObject);
	}

	public void ReallocateSubPools()
	{
		for (int num = _kSpriteSubPoolList.Count - 1; num >= 0; num--)
		{
			KSpriteSubPool kSpriteSubPool = _kSpriteSubPoolList[num];
			if (kSpriteSubPool.CurSpriteCount > 1)
			{
				CurSubPoolIndex = num;
				break;
			}
		}
	}

	public void CheckLoaded()
	{
		if (Initialized)
		{
			for (int i = 0; i < _kSpriteSubPoolList.Count; i++)
			{
				KSpriteSubPool kSpriteSubPool = _kSpriteSubPoolList[i];
				kSpriteSubPool.CheckLoaded();
			}
		}
	}
}
