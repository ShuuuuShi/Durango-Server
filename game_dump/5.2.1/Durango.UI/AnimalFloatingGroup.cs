using Durango.Render.Camera;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Shared.Animal;
using Shared.Battle;
using UnityEngine;

namespace Durango.UI;

public class AnimalFloatingGroup : UIBase
{
	[SerializeField]
	[EnumList(typeof(AnimalStatus), true, 0, -1)]
	private SpriteData[] _animalStatusIconList;

	[SerializeField]
	[EnumList(typeof(PetAI.HungryState), false, 0, -1)]
	private SpriteData[] _hungryStatusIconList;

	[SerializeField]
	private AnimalFloatingControl _inspectorPrefab;

	[SerializeField]
	private int _iconOffsetX;

	[SerializeField]
	private int _iconOffsetY;

	private readonly ListObjectPool<AnimalFloatingControl> _inspectorList = new ListObjectPool<AnimalFloatingControl>();

	private void Awake()
	{
		_inspectorList.BaseObject = _inspectorPrefab;
		_inspectorList.Init(null, base.transform);
	}

	private void Start()
	{
		Singleton<AnimalManager>.Instance().AnimalAppeared += OnAppearAnimal;
		Singleton<PetManager>.Instance().PetAppeared += OnAppearPet;
		Singleton<GameManager>.Instance().PreReconnect += GameManager_PreReconnect;
	}

	private void OnAppearAnimal(AnimalBehavior animal)
	{
		Add(animal);
	}

	private void OnAppearPet(AnimalBehavior animal)
	{
		PetAI component = animal.GetComponent<PetAI>();
		if (component.IsLocalPlayersPet() && !component.InCage)
		{
			Add(animal);
		}
	}

	private void GameManager_PreReconnect()
	{
		_inspectorList.Clear();
	}

	private void LateUpdate()
	{
		for (int num = _inspectorList.Count - 1; num >= 0; num--)
		{
			AnimalFloatingControl animalFloatingControl = _inspectorList[num];
			AnimalBehavior animal = animalFloatingControl.Animal;
			if (animal == null)
			{
				int num2 = _inspectorList.Count - 1;
				_inspectorList.Swap(num, num2);
				_inspectorList.Set(num2);
			}
			else
			{
				PetAI pet = animalFloatingControl.Pet;
				if (pet != null)
				{
					int hungry = (int)pet.Hungry;
					animalFloatingControl.SetStatusIcon(_hungryStatusIconList.Get(hungry));
				}
				else
				{
					int status = (int)animal.Status;
					animalFloatingControl.SetStatusIcon(_animalStatusIconList.Get(status));
				}
				Transform bodyPartTransform = animal.GetBodyPartTransform(BodyPart.Head);
				float zoom = Singleton<MainCamera>.Instance().Zoom;
				Vector3 localPosition = MainCamera.WorldToNGUIPos(bodyPartTransform.position);
				localPosition.x += (float)_iconOffsetX * zoom;
				localPosition.y += (float)_iconOffsetY * zoom;
				animalFloatingControl.transform.localPosition = localPosition;
			}
		}
	}

	private void Add([NotNull] AnimalBehavior animalBehavior)
	{
		_inspectorList.Add().Initialize(animalBehavior);
	}
}
