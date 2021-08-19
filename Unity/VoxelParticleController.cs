namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelParticleController : MonoBehaviour
	{
		private List<VoxelHumanoid> _voxelHumanoids;

		[SerializeField]
		private ParticleSystem _damage;

		private void Start()
		{
			_voxelHumanoids = new List<VoxelHumanoid>();
			
			var humans = GameObject.FindObjectsOfType<VoxelHumanoid>(true);

			foreach (var vh in humans)
			{
				vh.onVoxelDestroyEvent += OnVoxelHumanVoxelDestroy;
			}
		}

		private void OnVoxelHumanVoxelDestroy(IVoxel voxel, Vector3 point, Vector3 center)
		{
			var ep = new ParticleSystem.EmitParams();
			ep.position = point;
			ep.velocity = (point - center).normalized * Random.Range(1, 2);
			_damage.Emit(ep, 1);
		}
	}
}