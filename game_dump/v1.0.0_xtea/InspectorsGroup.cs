using UnityEngine;

public class InspectorsGroup : UIBase
{
	[SerializeField]
	private InspectorPanel _inspectorPanel;

	private void OnEnable()
	{
		KSingleton<AnimalManager>.Instance().AnimalAppeared += OnAppearAnimal;
	}

	private void OnAppearAnimal(AnimalBehavior animal)
	{
		_inspectorPanel.Add(animal);
	}
}
