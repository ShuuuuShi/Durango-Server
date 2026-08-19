using System;
using BestHTTP;
using Durango.Logic;
using Durango.Logic.Clusters;
using Durango.Logic.PlayGuide;
using Durango.Network;
using Durango.System;
using Durango.Utils;
using Durango.Utils.Extensions;
using Newtonsoft.Json.Linq;

public class CustomerServiceSystem : GameSystem<CustomerServiceSystem>
{
	private const string LastCsReadTime = "last_read_cs_time";

	private bool _hasUnreadAnswer;

	private double? _lastAnswerTime;

	public bool HasUnreadAnswer
	{
		get
		{
			return _hasUnreadAnswer;
		}
		private set
		{
			if (_hasUnreadAnswer != value)
			{
				_hasUnreadAnswer = value;
				OnHasUnreadAnswerUpdated();
			}
		}
	}

	public event Action HasUnreadAnswerUpdated;

	private void Start()
	{
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (GameManager.ClusterMode == Mode.Online)
			{
				string url = GameManager.GatewayUrl + "/cs/answer";
				Http.Request(url, OnLastAnswerTime, disableCache: true, addSession: true);
			}
		};
	}

	private void OnLastAnswerTime(byte[] bytes, HTTPResponse response)
	{
		if (bytes != null)
		{
			_lastAnswerTime = Json.Read<JObject>(bytes)?.Get<double?>("updated_at");
			float @float = Preferences.GetFloat("last_read_cs_time", 0f, Preferences.Level.User);
			double? lastAnswerTime = _lastAnswerTime;
			HasUnreadAnswer = lastAnswerTime.HasValue && lastAnswerTime.GetValueOrDefault() > (double)@float;
		}
	}

	public void ShowCustomerServiece()
	{
		Platform.Instance.ShowCustomerServiece();
		OnReadCustomerService();
	}

	private void OnReadCustomerService()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		Preferences.SetFloat("last_read_cs_time", (float)predictedServerTime, Preferences.Level.User);
		double? lastAnswerTime = _lastAnswerTime;
		HasUnreadAnswer = lastAnswerTime.HasValue && lastAnswerTime.GetValueOrDefault() > predictedServerTime;
	}

	private void OnHasUnreadAnswerUpdated()
	{
		ToDoCollection toDoCollection = GameSystem<ToDoListSystem>.Instance().FindCollection("read_cs");
		if (toDoCollection == null)
		{
			toDoCollection = new CustomerServiceToDoCollection();
			GameSystem<ToDoListSystem>.Instance().Add(toDoCollection, UIManager.IsLoadingCurtain);
		}
		else if (!HasUnreadAnswer)
		{
			GameSystem<ToDoListSystem>.Instance().Remove(toDoCollection);
		}
		if (this.HasUnreadAnswerUpdated != null)
		{
			this.HasUnreadAnswerUpdated();
		}
	}

	[ExposedInEditor(null)]
	private void Toggle()
	{
		HasUnreadAnswer = !HasUnreadAnswer;
	}
}
