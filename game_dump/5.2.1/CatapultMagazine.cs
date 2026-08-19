using UnityEngine;

public class CatapultMagazine : MonoBehaviour
{
	public enum Quantity
	{
		None,
		Many,
		Full
	}

	[SerializeField]
	private GameObject _modelMany;

	[SerializeField]
	private GameObject _modelFull;

	public void UpdateMagazine(Quantity quantity)
	{
		_modelMany.SetActive(quantity == Quantity.Many);
		_modelFull.SetActive(quantity == Quantity.Full);
	}
}
