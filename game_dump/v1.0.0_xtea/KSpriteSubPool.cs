using UnityEngine;

public class KSpriteSubPool
{
	private readonly int _poolSize;

	private readonly bool _selectable;

	private readonly GameObject _parent;

	private readonly KSprite[] _kSpriteArray;

	public int CurSpriteCount { get; private set; }

	public KSpriteSubPool(int poolSize, bool selectable, GameObject parent)
	{
		_poolSize = poolSize;
		_selectable = selectable;
		_parent = parent;
		CurSpriteCount = 0;
		_kSpriteArray = new KSprite[poolSize];
		for (int i = 0; i < _poolSize; i++)
		{
			_kSpriteArray[i] = CreateSprite();
		}
	}

	private KSprite CreateSprite()
	{
		return KSingleton<SpriteManager>.Instance().CreateSprite(_parent, _selectable);
	}

	public int GetPoolSize()
	{
		return _poolSize;
	}

	public KSprite GetNextSprite()
	{
		return _kSpriteArray[CurSpriteCount++];
	}

	public void ResetSprites()
	{
		CurSpriteCount = 0;
		for (int i = 0; i < _poolSize; i++)
		{
			_kSpriteArray[i].GameObject.SetActive(false);
		}
	}

	public void CheckLoaded()
	{
		for (int i = 0; i < CurSpriteCount; i++)
		{
			if (!((Object)(object)_kSpriteArray[i].GameObject == (Object)null))
			{
				_kSpriteArray[i].CheckLoaded();
			}
		}
	}
}
