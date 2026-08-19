using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Control;

public class UIParticleSprite : UISprite
{
	private struct Particle
	{
		public Vector2 Pos;

		public Vector2 Size;

		public Vector2 Velocity;

		public float StartAt;

		public float HideAt;
	}

	[HideInInspector]
	[SerializeField]
	private Vector2 _pivot = Vector2.one * 0.5f;

	[HideInInspector]
	[SerializeField]
	private Vector3 _angle = Vector2.zero;

	[HideInInspector]
	[SerializeField]
	private float _radius = 100f;

	[HideInInspector]
	[SerializeField]
	private float _spreadRatio = 2f;

	[HideInInspector]
	[SerializeField]
	private float _frequency = 5f;

	[HideInInspector]
	[SerializeField]
	private float _minMakeRatio = 0.5f;

	[HideInInspector]
	[SerializeField]
	private float _maxMakeRatio = 1f;

	[HideInInspector]
	[SerializeField]
	private float _power = 100f;

	[HideInInspector]
	[SerializeField]
	private float _minPower = 0.8f;

	[HideInInspector]
	[SerializeField]
	private float _maxPower = 1f;

	[HideInInspector]
	[SerializeField]
	private float _minDuration = 0.8f;

	[HideInInspector]
	[SerializeField]
	private float _maxDuration = 1.2f;

	[HideInInspector]
	[SerializeField]
	private float _minSize = 10f;

	[HideInInspector]
	[SerializeField]
	private float _maxSize = 16f;

	[HideInInspector]
	[SerializeField]
	private float _fadeIn = 0.3f;

	[HideInInspector]
	[SerializeField]
	private float _fadeOut = 0.3f;

	private readonly List<Particle> _particles = new List<Particle>();

	private float _nextMakeAt;

	private float _time;

	private float _delta;

	private bool _isEditorPlay;

	protected override void Awake()
	{
		fillGeometry = false;
		base.Awake();
	}

	protected override void OnEnable()
	{
		_particles.Clear();
		_nextMakeAt = 0f;
		base.OnEnable();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		bool isPlaying = Application.isPlaying;
		if (isPlaying || _isEditorPlay)
		{
			if (isPlaying)
			{
				_delta = Time.deltaTime;
				_time = Time.time;
			}
			else
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				_delta = realtimeSinceStartup - _time;
				_time = realtimeSinceStartup;
			}
			OnFill();
		}
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
	}

	private void OnFill()
	{
		UISpriteData atlasSprite = GetAtlasSprite();
		if (atlasSprite != null)
		{
			UpdateParticles();
			if (_nextMakeAt < _time)
			{
				MakeParticle(atlasSprite);
			}
			Texture texture = mainTexture;
			Rect rect = new Rect(atlasSprite.x, atlasSprite.y, atlasSprite.width, atlasSprite.height);
			rect = NGUIMath.ConvertToTexCoords(rect, texture.width, texture.height);
			geometry.Clear();
			int i = 0;
			for (int count = _particles.Count; i < count; i++)
			{
				Draw(_particles[i], rect);
			}
			if (Application.isPlaying)
			{
				mChanged = true;
			}
			else
			{
				NGUITools.SetDirty(this);
			}
		}
	}

	private void Draw(Particle particle, Rect uv)
	{
		if (!(particle.HideAt < _time))
		{
			Vector3 vector = particle.Pos;
			Vector2 size = particle.Size;
			geometry.verts.Add(vector);
			geometry.verts.Add(vector + Vector3.right * size.x);
			geometry.verts.Add(vector + Vector3.right * size.x + Vector3.up * size.y);
			geometry.verts.Add(vector + Vector3.up * size.y);
			geometry.uvs.Add(new Vector2(uv.xMin, uv.yMin));
			geometry.uvs.Add(new Vector2(uv.xMax, uv.yMin));
			geometry.uvs.Add(new Vector2(uv.xMax, uv.yMax));
			geometry.uvs.Add(new Vector2(uv.xMin, uv.yMax));
			Color item = color;
			item.a = finalAlpha;
			if (_time - particle.StartAt < _fadeIn)
			{
				item.a *= (_time - particle.StartAt) / _fadeIn;
			}
			else if (particle.HideAt - _time < _fadeOut)
			{
				item.a *= (particle.HideAt - _time) / _fadeOut;
			}
			geometry.cols.Add(item);
			geometry.cols.Add(item);
			geometry.cols.Add(item);
			geometry.cols.Add(item);
		}
	}

	private void UpdateParticles()
	{
		int i = 0;
		for (int count = _particles.Count; i < count; i++)
		{
			Particle value = _particles[i];
			value.Pos += value.Velocity * _delta;
			_particles[i] = value;
		}
	}

	private void MakeParticle(UISpriteData sd)
	{
		Particle particle = default(Particle);
		Vector3[] array = localCorners;
		float num = UnityEngine.Random.Range(_minSize, _maxSize);
		Vector3 vector = array[0] + Vector3.Scale(localSize, _pivot);
		Vector3 up = Vector3.up;
		Vector3 forward = Vector3.forward;
		Vector3 right = Vector3.right;
		Quaternion quaternion = Quaternion.Euler(_angle);
		up = quaternion * up;
		forward = quaternion * forward;
		right = quaternion * right;
		float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		float num2 = UnityEngine.Random.Range(_minMakeRatio, _maxMakeRatio) * _radius;
		Vector3 vector2 = (forward * Mathf.Cos(f) + right * Mathf.Sin(f)) * num2;
		Vector3 vector3 = vector + vector2;
		Vector3 vector4 = vector + up * _power + vector2 * _spreadRatio;
		particle.Pos = vector3;
		float num3 = (float)sd.width / (float)sd.height;
		particle.Size.x = ((!(num3 >= 1f)) ? (num * num3) : num);
		particle.Size.y = ((!(num3 <= 1f)) ? (num / num3) : num);
		particle.Pos -= particle.Size * 0.5f;
		particle.Velocity = (vector4 - vector3) * UnityEngine.Random.Range(_minPower, _maxPower);
		particle.StartAt = _time;
		particle.HideAt = _time + UnityEngine.Random.Range(_minDuration, _maxDuration);
		int num4 = -1;
		for (int i = 0; i < _particles.Count; i++)
		{
			if (_particles[i].HideAt < _time)
			{
				num4 = i;
				break;
			}
		}
		if (num4 == -1)
		{
			_particles.Add(particle);
		}
		else
		{
			_particles[num4] = particle;
		}
		_nextMakeAt = _time + 1f / _frequency;
	}

	public void EditorPlay(bool enable)
	{
		if (enable)
		{
			_isEditorPlay = true;
			_time = Time.realtimeSinceStartup;
			return;
		}
		_isEditorPlay = false;
		_particles.Clear();
		geometry.Clear();
		MarkAsChanged();
	}
}
