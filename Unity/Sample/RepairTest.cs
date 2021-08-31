namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class RepairTest : MonoBehaviour
	{
		[SerializeField]
		private VoxelHumanoid _target;

		[SerializeField]
		private VoxelParticle _vp;

		[SerializeField]
		private Vector3 _from;

		void OnGUI()
		{
			if (GUI.Button(new Rect(0, Screen.height - 100, 100, 100), "Hoge"))
			{
				var list = new List<IVoxel>();

				foreach (var v in _target.voxels)
				{
					if (Random.Range(0, 3) == 0)
					{
						list.Add(v);
					}
				}

				_vp.Repair(_target, list, _from, 1, voxel =>
				{
					
				});
			}
		}
	}
}