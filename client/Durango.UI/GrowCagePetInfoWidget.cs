using System;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Animal;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class GrowCagePetInfoWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UIWidget _contentsWidget;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UIModelViewer _viewer;

	[SerializeField]
	private NestedPrefabLinker _taskProgressWidget;

	[SerializeField]
	private UILabel _taskNameLabel;

	[SerializeField]
	private SelectableWidget _productionTaskButton;

	[SerializeField]
	private SelectableWidget _trainingTaskButton;

	[SerializeField]
	private SelectableWidget _takeoutButton;

	[SerializeField]
	private SelectableWidget _normalFeedButton;

	[SerializeField]
	private SelectableWidget _taskStopButton;

	[SerializeField]
	private SelectableWidget _taskFeedButton;

	[SerializeField]
	private SelectableWidget _skipProcessButton;

	[SerializeField]
	private SelectableButton _taskFinishedButton;

	[SerializeField]
	private UIWidget _emptyWidget;

	[SerializeField]
	private UILabel _emptyLabel;

	private Messages.Pet _target;

	private TaskStatus? _task;

	private UIWidget[] _normalButtons;

	private UIWidget[] _progressingButtons;

	public event Action<Messages.Pet, PetTaskType> TaskStarted;

	public event Action<Messages.Pet> TaskStopped;

	public event Action<Messages.Pet> TaskFinished;

	public event Action<Messages.Pet> PetTookOut;

	public event Action<Messages.Pet> OnFeed;

	void IUIInitializable.Init()
	{
		SelectableButton taskFinishedButton = _taskFinishedButton;
		taskFinishedButton.Clicked = (Action)Delegate.Combine(taskFinishedButton.Clicked, (Action)delegate
		{
			if (this.TaskFinished != null)
			{
				this.TaskFinished(_target);
			}
		});
		SelectableWidget productionTaskButton = _productionTaskButton;
		productionTaskButton.Clicked = (Action)Delegate.Combine(productionTaskButton.Clicked, (Action)delegate
		{
			if (this.TaskStarted != null)
			{
				this.TaskStarted(_target, PetTaskType.Production);
			}
		});
		SelectableWidget trainingTaskButton = _trainingTaskButton;
		trainingTaskButton.Clicked = (Action)Delegate.Combine(trainingTaskButton.Clicked, (Action)delegate
		{
			if (this.TaskStarted != null)
			{
				this.TaskStarted(_target, PetTaskType.Training);
			}
		});
		SelectableWidget takeoutButton = _takeoutButton;
		takeoutButton.Clicked = (Action)Delegate.Combine(takeoutButton.Clicked, (Action)delegate
		{
			if (this.PetTookOut != null)
			{
				this.PetTookOut(_target);
			}
		});
		SelectableWidget normalFeedButton = _normalFeedButton;
		normalFeedButton.Clicked = (Action)Delegate.Combine(normalFeedButton.Clicked, (Action)delegate
		{
			if (this.OnFeed != null)
			{
				this.OnFeed(_target);
			}
		});
		SelectableWidget taskStopButton = _taskStopButton;
		taskStopButton.Clicked = (Action)Delegate.Combine(taskStopButton.Clicked, (Action)delegate
		{
			if (this.TaskStopped != null)
			{
				this.TaskStopped(_target);
			}
		});
		SelectableWidget taskFeedButton = _taskFeedButton;
		taskFeedButton.Clicked = (Action)Delegate.Combine(taskFeedButton.Clicked, (Action)delegate
		{
			if (this.OnFeed != null)
			{
				this.OnFeed(_target);
			}
		});
		SelectableWidget skipProcessButton = _skipProcessButton;
		skipProcessButton.Clicked = (Action)Delegate.Combine(skipProcessButton.Clicked, (Action)delegate
		{
		});
		_normalButtons = new UIWidget[4] { _productionTaskButton.Widget, _trainingTaskButton.Widget, _normalFeedButton.Widget, _takeoutButton.Widget };
		_progressingButtons = new UIWidget[2] { _taskStopButton.Widget, _taskFeedButton.Widget };
		_taskFinishedButton.Text = T._("확인");
		_taskFinishedButton.SetEffect(PresetButton.Effect.Emphasis);
		_skipProcessButton.gameObject.SetActive(value: false);
	}

	public void Set(Messages.Pet target, TaskStatus? task)
	{
		_target = target;
		_task = task;
		_emptyWidget.gameObject.SetActive(value: false);
		Yaml.Pet pet = SingletonDict<int, Yaml.Pet>.Get(target.EntityType);
		Animal animal = ((pet != null) ? SingletonDict<int, Animal>.Get(pet.VehicleEntityType) : null);
		if (animal == null)
		{
			_contentsWidget.gameObject.SetActive(value: false);
			return;
		}
		_nameLabel.text = target.GetPetName(includeRank: true);
		_infoLabel.text = $"{animal.Name}\n{PetUtil.GetPetInfoString(target)}";
		string prefabPath = animal.PrefabPath;
		bool flag = target.Stat.Life == null || target.Stat.Life.Ratio() <= 0f;
		bool isOld = target.Stat.IsOld;
		object obj;
		if (flag)
		{
			obj = _viewer.DefaultDeadAnimalPlay(isOld);
		}
		else
		{
			TaskStatus? task2 = _task;
			obj = (task2.HasValue ? _viewer.DefaultAnimalPlay("move_motion_sets", isOld) : _viewer.DefaultAnimalPlay("idle", "stand", isOld));
		}
		Action<GameObject> loaded = (Action<GameObject>)obj;
		_viewer.SetPlainModel(prefabPath, new UIModelViewer.Arguments
		{
			CameraAngle = 35f,
			Rotation = 140f,
			Loaded = loaded
		});
		RefreshButtons();
		RefreshProgressWidget();
		_contentsWidget.gameObject.SetActive(value: true);
	}

	public void SetEmpty()
	{
		SetEmpty(T._("새로운 동물을 넣어주세요.\n[size=22][FFFFFF90]길들인 동물을 보관하거나 동물의 상태를 확인할 수 있습니다.[/size]"));
	}

	private void SetEmpty(string text)
	{
		_contentsWidget.gameObject.SetActive(value: false);
		_emptyWidget.gameObject.SetActive(value: true);
		_emptyLabel.text = text;
	}

	private void RefreshButtons()
	{
		TaskStatus? task = _task;
		if (!task.HasValue)
		{
			SetNormalButtons();
			return;
		}
		TaskStatus value = _task.Value;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (predictedServerTime < value.Until)
		{
			SetTaskProgressingButtons();
		}
		else
		{
			SetTaskFinishedButtons();
		}
	}

	private void RefreshProgressWidget()
	{
		TaskStatus? task = _task;
		if (!task.HasValue)
		{
			_taskProgressWidget.gameObject.SetActive(value: false);
			return;
		}
		_taskProgressWidget.gameObject.SetActive(value: true);
		TaskStatus value = _task.Value;
		_taskProgressWidget.Object.GetComponent<PetTaskProgressWidget>().Set(_target, _task.Value);
		PetTask petTask = ((!string.IsNullOrEmpty(value.TaskId)) ? SingletonDict<string, PetTask>.Get(value.TaskId) : null);
		_taskNameLabel.text = ((petTask != null) ? petTask.Name.ToString() : value.TaskId);
	}

	private void SetNormalButtons()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_normalButtons); i < size; i++)
		{
			_normalButtons[i].gameObject.SetActive(value: true);
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(_progressingButtons); j < size2; j++)
		{
			_progressingButtons[j].gameObject.SetActive(value: false);
		}
		_taskFinishedButton.gameObject.SetActive(value: false);
		Vector3 localPosition = _normalButtons[0].transform.localPosition;
		localPosition.x = 0f;
		UIUtility.WidgetsReposition(_normalButtons, Vector3.right, localPosition, 0f, 0.5f);
	}

	private void SetTaskProgressingButtons()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_normalButtons); i < size; i++)
		{
			_normalButtons[i].gameObject.SetActive(value: false);
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(_progressingButtons); j < size2; j++)
		{
			_progressingButtons[j].gameObject.SetActive(value: true);
		}
		_taskFinishedButton.gameObject.SetActive(value: false);
		Vector3 localPosition = _progressingButtons[0].transform.localPosition;
		localPosition.x = 0f;
		UIUtility.WidgetsReposition(_progressingButtons, Vector3.right, localPosition, 0f, 0.5f);
	}

	private void SetTaskFinishedButtons()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_normalButtons); i < size; i++)
		{
			_normalButtons[i].gameObject.SetActive(value: false);
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(_progressingButtons); j < size2; j++)
		{
			_progressingButtons[j].gameObject.SetActive(value: false);
		}
		_taskFinishedButton.gameObject.SetActive(value: true);
		Vector3 localPosition = _taskFinishedButton.transform.localPosition;
		localPosition.x = 0f;
		_taskFinishedButton.transform.localPosition = localPosition;
	}
}
