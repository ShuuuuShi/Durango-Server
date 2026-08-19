using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

public abstract class ParticleAffector : MonoBehaviour
{
	protected struct GetForceParameters
	{
		public float distanceToAffectorCenterSqr;

		public Vector3 scaledDirectionToAffectorCenter;

		public Vector3 particlePosition;
	}

	[Header("Common Controls")]
	public float radius = float.PositiveInfinity;

	public float force = 5f;

	public Vector3 offset = Vector3.zero;

	private float _radius;

	private float radiusSqr;

	private float forceDeltaTime;

	private Vector3 transformPosition;

	private float[] particleSystemExternalForcesMultipliers;

	public AnimationCurve scaleForceByDistance = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	private ParticleSystem _particleSystem;

	public ParticleSystem[] _particleSystems;

	private int particleSystemsCount;

	private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	private ParticleSystem.Particle[][] particleSystemParticles;

	private ParticleSystem.MainModule[] particleSystemMainModules;

	private Renderer[] particleSystemRenderers;

	protected ParticleSystem currentParticleSystem;

	protected GetForceParameters parameters;

	public bool alwaysUpdate;

	public float scaledRadius => radius * base.transform.lossyScale.x;

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
		_particleSystem = GetComponent<ParticleSystem>();
	}

	protected virtual void perParticleSystemSetup()
	{
	}

	protected virtual Vector3 getForce()
	{
		return Vector3.zero;
	}

	protected virtual void Update()
	{
	}

	protected virtual void LateUpdate()
	{
		_radius = scaledRadius;
		radiusSqr = _radius * _radius;
		forceDeltaTime = force * Time.deltaTime;
		transformPosition = base.transform.position + offset;
		if ((bool)_particleSystem)
		{
			if (particleSystems.Count == 1)
			{
				particleSystems[0] = _particleSystem;
			}
			else
			{
				particleSystems.Clear();
				particleSystems.Add(_particleSystem);
			}
		}
		else if (_particleSystems.Length != 0)
		{
			if (particleSystems.Count != _particleSystems.Length)
			{
				particleSystems.Clear();
				particleSystems.AddRange(_particleSystems);
			}
			else
			{
				for (int i = 0; i < _particleSystems.Length; i++)
				{
					particleSystems[i] = _particleSystems[i];
				}
			}
		}
		else
		{
			particleSystems.Clear();
			particleSystems.AddRange(UnityEngine.Object.FindObjectsOfType<ParticleSystem>());
		}
		parameters = default(GetForceParameters);
		particleSystemsCount = particleSystems.Count;
		if (particleSystemParticles == null || particleSystemParticles.Length < particleSystemsCount)
		{
			particleSystemParticles = new ParticleSystem.Particle[particleSystemsCount][];
			particleSystemMainModules = new ParticleSystem.MainModule[particleSystemsCount];
			particleSystemRenderers = new Renderer[particleSystemsCount];
			particleSystemExternalForcesMultipliers = new float[particleSystemsCount];
			for (int j = 0; j < particleSystemsCount; j++)
			{
				ref ParticleSystem.MainModule reference = ref particleSystemMainModules[j];
				reference = particleSystems[j].main;
				particleSystemRenderers[j] = particleSystems[j].GetComponent<Renderer>();
				particleSystemExternalForcesMultipliers[j] = particleSystems[j].externalForces.multiplier;
			}
		}
		for (int k = 0; k < particleSystemsCount; k++)
		{
			if (!particleSystemRenderers[k].isVisible && !alwaysUpdate)
			{
				continue;
			}
			int maxParticles = particleSystemMainModules[k].maxParticles;
			if (particleSystemParticles[k] == null || particleSystemParticles[k].Length < maxParticles)
			{
				particleSystemParticles[k] = new ParticleSystem.Particle[maxParticles];
			}
			currentParticleSystem = particleSystems[k];
			perParticleSystemSetup();
			int particles = currentParticleSystem.GetParticles(particleSystemParticles[k]);
			ParticleSystemSimulationSpace simulationSpace = particleSystemMainModules[k].simulationSpace;
			ParticleSystemScalingMode scalingMode = particleSystemMainModules[k].scalingMode;
			Transform transform = currentParticleSystem.transform;
			Transform customSimulationSpace = particleSystemMainModules[k].customSimulationSpace;
			if (simulationSpace == ParticleSystemSimulationSpace.World)
			{
				for (int l = 0; l < particles; l++)
				{
					parameters.particlePosition = particleSystemParticles[k][l].position;
					parameters.scaledDirectionToAffectorCenter.x = transformPosition.x - parameters.particlePosition.x;
					parameters.scaledDirectionToAffectorCenter.y = transformPosition.y - parameters.particlePosition.y;
					parameters.scaledDirectionToAffectorCenter.z = transformPosition.z - parameters.particlePosition.z;
					parameters.distanceToAffectorCenterSqr = parameters.scaledDirectionToAffectorCenter.sqrMagnitude;
					if (parameters.distanceToAffectorCenterSqr < radiusSqr)
					{
						float time = parameters.distanceToAffectorCenterSqr / radiusSqr;
						float num = scaleForceByDistance.Evaluate(time);
						Vector3 vector = getForce();
						float num2 = forceDeltaTime * num * particleSystemExternalForcesMultipliers[k];
						vector.x *= num2;
						vector.y *= num2;
						vector.z *= num2;
						Vector3 velocity = particleSystemParticles[k][l].velocity;
						velocity.x += vector.x;
						velocity.y += vector.y;
						velocity.z += vector.z;
						particleSystemParticles[k][l].velocity = velocity;
					}
				}
			}
			else
			{
				Vector3 zero = Vector3.zero;
				Quaternion identity = Quaternion.identity;
				Vector3 one = Vector3.one;
				Transform transform2 = transform;
				switch (simulationSpace)
				{
				case ParticleSystemSimulationSpace.Local:
					zero = transform2.position;
					identity = transform2.rotation;
					one = transform2.localScale;
					break;
				case ParticleSystemSimulationSpace.Custom:
					transform2 = customSimulationSpace;
					zero = transform2.position;
					identity = transform2.rotation;
					one = transform2.localScale;
					break;
				default:
					throw new NotSupportedException($"Unsupported scaling mode '{simulationSpace}'.");
				}
				for (int m = 0; m < particles; m++)
				{
					parameters.particlePosition = particleSystemParticles[k][m].position;
					if (simulationSpace == ParticleSystemSimulationSpace.Local || simulationSpace == ParticleSystemSimulationSpace.Custom)
					{
						switch (scalingMode)
						{
						case ParticleSystemScalingMode.Hierarchy:
							parameters.particlePosition = transform2.TransformPoint(particleSystemParticles[k][m].position);
							break;
						case ParticleSystemScalingMode.Local:
							parameters.particlePosition = Vector3.Scale(parameters.particlePosition, one);
							parameters.particlePosition = identity * parameters.particlePosition;
							parameters.particlePosition += zero;
							break;
						case ParticleSystemScalingMode.Shape:
							parameters.particlePosition = identity * parameters.particlePosition;
							parameters.particlePosition += zero;
							break;
						default:
							throw new NotSupportedException($"Unsupported scaling mode '{scalingMode}'.");
						}
					}
					parameters.scaledDirectionToAffectorCenter.x = transformPosition.x - parameters.particlePosition.x;
					parameters.scaledDirectionToAffectorCenter.y = transformPosition.y - parameters.particlePosition.y;
					parameters.scaledDirectionToAffectorCenter.z = transformPosition.z - parameters.particlePosition.z;
					parameters.distanceToAffectorCenterSqr = parameters.scaledDirectionToAffectorCenter.sqrMagnitude;
					if (!(parameters.distanceToAffectorCenterSqr < radiusSqr))
					{
						continue;
					}
					float time2 = parameters.distanceToAffectorCenterSqr / radiusSqr;
					float num3 = scaleForceByDistance.Evaluate(time2);
					Vector3 vector2 = getForce();
					float num4 = forceDeltaTime * num3 * particleSystemExternalForcesMultipliers[k];
					vector2.x *= num4;
					vector2.y *= num4;
					vector2.z *= num4;
					if (simulationSpace == ParticleSystemSimulationSpace.Local || simulationSpace == ParticleSystemSimulationSpace.Custom)
					{
						switch (scalingMode)
						{
						case ParticleSystemScalingMode.Hierarchy:
							vector2 = transform2.InverseTransformVector(vector2);
							break;
						case ParticleSystemScalingMode.Local:
							vector2 = Quaternion.Inverse(identity) * vector2;
							vector2 = Vector3.Scale(vector2, new Vector3(1f / one.x, 1f / one.y, 1f / one.z));
							break;
						case ParticleSystemScalingMode.Shape:
							vector2 = Quaternion.Inverse(identity) * vector2;
							break;
						default:
							throw new NotSupportedException($"Unsupported scaling mode '{scalingMode}'.");
						}
					}
					Vector3 velocity2 = particleSystemParticles[k][m].velocity;
					velocity2.x += vector2.x;
					velocity2.y += vector2.y;
					velocity2.z += vector2.z;
					particleSystemParticles[k][m].velocity = velocity2;
				}
			}
			currentParticleSystem.SetParticles(particleSystemParticles[k], particles);
		}
	}

	private void OnApplicationQuit()
	{
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position + offset, scaledRadius);
	}
}
