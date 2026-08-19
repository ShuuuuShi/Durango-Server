using Durango.Render.Particle;
using UnityEngine;

public abstract class TrapBase : MonoBehaviour
{
	[SerializeField]
	private ParticleType _particleOnConstruct;

	[SerializeField]
	private SoundEventType _soundOnConstruct;

	[SerializeField]
	private ParticleType _particleOnTrapped;

	[SerializeField]
	private SoundEventType _soundOnTrapped;

	[SerializeField]
	private ParticleType _particleOnBreak;

	[SerializeField]
	private SoundEventType _soundOnBreak;

	private void Awake()
	{
		ParticleManager.Cache(_particleOnConstruct);
		ParticleManager.Cache(_particleOnTrapped);
		ParticleManager.Cache(_particleOnBreak);
		SoundManager.PrepareEvent(_soundOnConstruct);
		SoundManager.PrepareEvent(_soundOnTrapped);
		SoundManager.PrepareEvent(_soundOnBreak);
	}

	public virtual void OnConstruct()
	{
		ParticleManager.Emit(_particleOnConstruct, base.gameObject.transform.position, Quaternion.identity, comeForwardToCamera: true);
		SoundManager.PlayEvent(_soundOnConstruct.Path, SoundPosition.Fix(base.gameObject.transform.position));
	}

	public virtual void OnTrapped()
	{
		ParticleManager.Emit(_particleOnTrapped, base.gameObject.transform.position, Quaternion.identity, comeForwardToCamera: true);
		SoundManager.PlayEvent(_soundOnTrapped.Path, SoundPosition.Fix(base.gameObject.transform.position));
	}

	public virtual void OnBreak()
	{
		ParticleManager.Emit(_particleOnBreak, base.gameObject.transform.position, Quaternion.identity, comeForwardToCamera: true);
		SoundManager.PlayEvent(_soundOnBreak.Path, SoundPosition.Fix(base.gameObject.transform.position));
	}
}
