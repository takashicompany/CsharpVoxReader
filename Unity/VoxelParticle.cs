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

			public bool Update(float deltaTime, out ParticleSystem.Particle particle)
			{
				var to = voxelOwner.GetVoxelWorldPosition(voxel.x, voxel.y, voxel.z);

				particle = new ParticleSystem.Particle();
				particle.velocity = Vector3.zero;

				if (Vector3.Distance(current, to) <= speedOrDuration * deltaTime)
				{
					particle.remainingLifetime = -1;
					return false;
				}

				particle.remainingLifetime = 1f;

				var dir = (to - current).normalized;

				current += dir * speedOrDuration * deltaTime;

				particle.position = current;
				return true;
			}
		}

		private List<Param> _params = new List<Param>();
		private ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[1000];

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

			_particleSystem.Emit(1000);
		}

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
	}
}