using UnityEngine;

public class Rider : MonoBehaviour
{
	private CharacterBehavior _owner;

	public CharacterBehavior Owner
	{
		get
		{
			if ((Object)(object)_owner == (Object)null)
			{
				_owner = ((Component)this).GetComponent<CharacterBehavior>();
			}
			return _owner;
		}
	}

	public bool IsRiding { get; set; }
}
