using UnityEngine;

namespace MirzaBeig.ParticleSystems;

public class ParticleSystems : MonoBehaviour
{
	public delegate void onParticleSystemsDeadEventHandler();

	public ParticleSystem[] particleSystems { get; set; }

	public event onParticleSystemsDeadEventHandler onParticleSystemsDeadEvent;

	protected virtual void Awake()
	{
		particleSystems = GetComponentsInChildren<ParticleSystem>();
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
	}

	protected virtual void LateUpdate()
	{
		if (this.onParticleSystemsDeadEvent != null && !isAlive())
		{
			this.onParticleSystemsDeadEvent();
		}
	}

	public void reset()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].time = 0f;
		}
	}

	public void play()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Play(withChildren: false);
		}
	}

	public void pause()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Pause(withChildren: false);
		}
	}

	public void stop()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Stop(withChildren: false);
		}
	}

	public void clear()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Clear(withChildren: false);
		}
	}

	public void setLoop(bool loop)
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem.MainModule main = particleSystems[i].main;
			main.loop = loop;
		}
	}

	public void setPlaybackSpeed(float speed)
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem.MainModule main = particleSystems[i].main;
			main.simulationSpeed = speed;
		}
	}

	public void simulate(float time, bool reset = false)
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Simulate(time, withChildren: false, reset);
		}
	}

	public bool isAlive()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			if ((bool)particleSystems[i] && particleSystems[i].IsAlive())
			{
				return true;
			}
		}
		return false;
	}

	public bool isPlaying(bool checkAll = false)
	{
		if (particleSystems.Length == 0)
		{
			return false;
		}
		if (!checkAll)
		{
			return particleSystems[0].isPlaying;
		}
		for (int i = 0; i < 0; i++)
		{
			if (!particleSystems[i].isPlaying)
			{
				return false;
			}
		}
		return true;
	}

	public int getParticleCount()
	{
		int num = 0;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			if ((bool)particleSystems[i])
			{
				num += particleSystems[i].particleCount;
			}
		}
		return num;
	}
}
