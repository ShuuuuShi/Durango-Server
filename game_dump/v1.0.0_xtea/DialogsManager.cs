using UnityEngine;

public class DialogsManager : KSingleton<DialogsManager>
{
	public GameObject _colliderWithKid;

	public GameObject _triggerDialogAfterEvent;

	public GameObject _triggerDialogActingSitDown;

	public GameObject _npcKid;

	public TextAsset _dialoglocalizedTextFile;

	public void AfterKidFindDropItem()
	{
	}
}
