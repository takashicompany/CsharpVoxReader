namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelParticle : MonoBehaviour
	{
		[SerializeField]
		private ParticleSystem _particleSystem;

		public delegate void CompleteDelegate(IVoxel voxel);

		[System.Serializable]
		private class Param
		{
			public VoxelHumanoid voxelOwner { get; /*private*/ set; }
			public IVoxel voxel { get; /*private*/ set; }

			public Vector3 from { get; /*private*/ set; }
			public Vector3 current { get; /*private*/ set; }
			public float speedOrDuration { get; /*private*/ set; }
			public CompleteDelegate onComplete { get; set; }

			public bool Update(float deltaTime, ParticleSystem.Particle[] particles, int index)
			{
				var isAlive = Update(deltaTime, out var position, out var velocity);

				particles[index].remainingLifetime = isAlive ? 1 : -1f;
				particles[index].position = position;
				particles[index].velocity = velocity;
				return isAlive;
			}

			public bool Update(float deltaTime, out ParticleSystem.EmitParams emitParams)
			{
				var isAlive = Update(deltaTime, out var position, out var velocity);

				emitParams = new ParticleSystem.EmitParams();

				emitParams.startLifetime = isAlive ? 1 : 0f;
				emitParams.position = position;
				emitParams.velocity = velocity;
				
				return isAlive;
			}

			private bool Update(float deltaTime, out Vector3 position, out Vector3 velocity)
			{
				var to = voxelOwner.GetVoxelWorldPosition(voxel.x, voxel.y, voxel.z);
				velocity = Vector3.zero;

				if (Vector3.Distance(current, to) <= speedOrDuration * deltaTime)
				{
					position = Vector3.zero;
					
					return false;
				}

				var dir = (to - current).normalized;
				current += dir * speedOrDuration * deltaTime;

				position = current;

				return true;
			}
		}

		private List<Param> _params = new List<Param>();
		private ParticleSystem.Particle[] _particles;
		
		private void Awake()
		{
			_particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
		}

		public void Repair(VoxelHumanoid voxelOwner, IEnumerable<IVoxel> voxels, Vector3 startPoint, float speedOrDuration, CompleteDelegate onComplete)
		{
			foreach (var v in voxels)
			{
				var p = new Param()
				{
					voxelOwner = voxelOwner,
					voxel = v,
					from = startPoint,
					current = startPoint,
					speedOrDuration = speedOrDuration,
					onComplete = onComplete
				};

				_params.Add(p);
			}
		}

		private void LateUpdate()
		{
			var aliveParticleCount = _particleSystem.GetParticles(_particles);
			// Debug.Log("alive:" + aliveParticleCount);
			var paramCount = _params.Count;
			
			var deltaTime = Time.deltaTime;

			var unused = 0;

			var isSetParticles = false;

			for (var i = paramCount - 1; i >= 0; i--)
			{
				var p = _params[i];
				var particleIndex = paramCount - i - 1 - unused;
				// Debug.Log(aliveParticleCount + ", " + particleIndex + ", " + paramCount);
				if (particleIndex < aliveParticleCount)
				{
					if (!p.Update(deltaTime, _particles, particleIndex))
					{
						_params.RemoveAt(i);
						//unused++;
					}
				}
				else
				{
					TrySetParticle();
					_particleSystem.Emit(1);
				}
			}

			TrySetParticle();

			void TrySetParticle()
			{
				if (isSetParticles)
				{
					return;
				}

				isSetParticles = true;
				_particleSystem.SetParticles(_particles,aliveParticleCount);
			}
		}



		/*
		private void Update()
		{
			var particleCount = _particleSystem.GetParticles(_particles);
			var paramCount = _params.Count;		// 5だとする

			var setParticles = false;

			var deltaTime = Time.deltaTime;

			// Debug.Log(paramCount + ", " + particleCount);

			for (var i = paramCount - 1; i >= 0; i--)	// i は4になる
			{
				var p = _params[i];

				var particleIndex = paramCount - i - 1;

				if (!p.Update(deltaTime, out var particle))
				{
					_params.RemoveAt(i);
				}

				_particles[particleIndex] = particle;

				// if (particleIndex < particleCount)
				// {
				// 	if (!p.Update(deltaTime, out var particle))
				// 	{
				// 		_params.RemoveAt(i);
				// 	}

				// 	_particles[particleIndex] = particle;
				// }
				// else
				// {
				// 	TrySetParticle();

				// 	if (p.UpdateWithEmit(deltaTime, out var emitParams))
				// 	{
				// 		_particleSystem.Emit(emitParams, 1);
				// 	}
				// 	else
				// 	{
				// 		_params.RemoveAt(i);
				// 	}
				// }
			}

			TrySetParticle();

			void TrySetParticle()
			{
				if (setParticles)
				{
					return;
				}

				setParticles = true;
				_particleSystem.SetParticles(_particles);
			}
		}
		*/
	}
}