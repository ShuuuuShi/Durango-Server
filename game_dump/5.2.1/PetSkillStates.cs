using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils.Extensions;
using Messages;
using Shared.Pet;
using Yaml;

public class PetSkillStates
{
	public class State
	{
		public string Id;

		public SkillRank Rank;

		public double? ReservedAt;

		public double? UsedAt;

		public double? ReactivatedAt;
	}

	private readonly Dictionary<string, State> _states = new Dictionary<string, State>();

	private readonly HashSet<string> _invalidKeys = new HashSet<string>();

	public Messages.Pet? Pet { get; private set; }

	public event Action PetChanged;

	public event Action<State> StateChanged;

	public void Set(Messages.Pet? pet)
	{
		Messages.Pet? pet2 = Pet;
		Pet = pet;
		if (!pet2.HasValue || !pet.HasValue || pet2.Value.EntityId != pet.Value.EntityId)
		{
			_states.Clear();
		}
		if (!Pet.HasValue)
		{
			return;
		}
		_invalidKeys.Clear();
		_invalidKeys.AddRange(_states.Keys);
		Messages.PetActiveSkill[] availableActiveSkill = Pet.Value.Statistics.AvailableActiveSkill;
		for (int i = 0; i < availableActiveSkill.Length; i++)
		{
			Messages.PetActiveSkill petActiveSkill = availableActiveSkill[i];
			if (_states.TryGetValue(petActiveSkill.SkillId, out var value))
			{
				value.Rank = petActiveSkill.Rank;
			}
			else
			{
				_states.Add(petActiveSkill.SkillId, new State
				{
					Id = petActiveSkill.SkillId,
					Rank = petActiveSkill.Rank
				});
			}
			_invalidKeys.Remove(petActiveSkill.SkillId);
		}
		foreach (string invalidKey in _invalidKeys)
		{
			_states.Remove(invalidKey);
		}
		if (this.PetChanged != null)
		{
			this.PetChanged();
		}
	}

	public void Reserved(string id)
	{
		State state = _states.Get(id);
		if (state != null)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			state.ReservedAt = predictedServerTime;
			state.UsedAt = null;
			state.ReactivatedAt = null;
			if (this.StateChanged != null)
			{
				this.StateChanged(state);
			}
		}
	}

	public void Used(string id)
	{
		State state = _states.Get(id);
		if (state != null)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			state.ReservedAt = null;
			state.UsedAt = predictedServerTime;
			state.ReactivatedAt = null;
			Yaml.PetActiveSkill petActiveSkill = PetActiveSkills.Get(state.Id, state.Rank);
			if (petActiveSkill != null)
			{
				state.ReactivatedAt = predictedServerTime + petActiveSkill.Cooltime;
			}
			if (this.StateChanged != null)
			{
				this.StateChanged(state);
			}
		}
	}

	public void Canceled(string id)
	{
		State state = _states.Get(id);
		if (state != null)
		{
			state.ReservedAt = null;
			state.UsedAt = null;
			state.ReactivatedAt = null;
			if (this.StateChanged != null)
			{
				this.StateChanged(state);
			}
		}
	}

	public State GetState(string id)
	{
		return _states.Get(id);
	}
}
