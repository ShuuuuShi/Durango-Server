using L10N;
using UnityEngine;

public class Inspector : MonoBehaviour
{
	[SerializeField]
	public Color NameColorNormal;

	[SerializeField]
	public Color NameColorMine;

	[SerializeField]
	private UILabel _ownerTag;

	[SerializeField]
	private UILabel _nameTag;

	private void Awake()
	{
		((Component)_ownerTag).gameObject.SetActive(false);
		((Component)_nameTag).gameObject.SetActive(false);
	}

	public void SetName(string name, string ownerName, bool showName, bool isLocalPlayers)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(ownerName))
		{
			((Component)_ownerTag).gameObject.SetActive(true);
			_ownerTag.text = T._("{0}의", ownerName);
		}
		if (showName)
		{
			((Component)_nameTag).gameObject.SetActive(true);
			_nameTag.text = name;
			_nameTag.color = ((!isLocalPlayers) ? NameColorNormal : NameColorMine);
		}
	}
}
