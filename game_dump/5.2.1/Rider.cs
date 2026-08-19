using System;
using UnityEngine;

public class Rider : MonoBehaviour, IEventTrasferer<Rider>
{
	private CharacterBehavior _owner;

	private bool _isRiding;

	public CharacterBehavior Owner
	{
		get
		{
			if (_owner == null)
			{
				_owner = GetComponent<CharacterBehavior>();
			}
			return _owner;
		}
	}

	public bool IsRiding
	{
		get
		{
			return _isRiding;
		}
		set
		{
			if (_isRiding != value)
			{
				_isRiding = value;
				if (this.RidingStateChanged != null)
				{
					this.RidingStateChanged();
				}
			}
		}
	}

	public event Action RidingStateChanged;

	public void TransferEvent(Rider rider)
	{
		RidingStateChanged += rider.RidingStateChanged;
		rider.RidingStateChanged = null;
	}
}
