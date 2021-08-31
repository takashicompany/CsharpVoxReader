namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelRepairer : MonoBehaviour
	{
		private IVoxelObject _targetInternal;

		private IVoxelObject _target => _targetInternal ?? (_targetInternal = GetComponent<IVoxelObject>());

		[SerializeField]
		private VoxelParticle _voxelParticle;
		
		private enum VoxelState
		{
			None,
			Active,		// 使わないかも
			Repairing
		}

		private VoxelState[,,] _voxelStateMap;

		private void Start()
		{
			var size = _target.voxelSize;
			_voxelStateMap = new VoxelState[size.x, size.y, size.z];
		}

		public void RepairRandom(int count, Bounds point, float speedOrDuration)
		{
			var repairList = _target.GenerateVoxelList(false);

			var picked = new List<IVoxel>();

			for (int i = 0; i < count && repairList.Count > 0; i++)
			{
				var index = Random.Range(0, repairList.Count);
				picked.Add(repairList[index]);
				repairList.RemoveAt(index);
			}

			foreach (var v in picked)
			{
				
			}
		}

		public void Repair(IVoxel voxel, Vector3 position, float speedOrDuration)
		{
			if (IsRepairing(voxel.voxelPosition))
			{
				return;
			}

			_voxelParticle.Repair(_target, voxel, position, speedOrDuration, v =>
			{
				ChangeVoxelState(v.voxelPosition, VoxelState.None);
				_target.ChangeVoxelActive(v.voxelPosition, true);
				_target.RequestUpdateMesh();
			});
		}

		private void ChangeVoxelState(Vector3Int voxelPosition, VoxelState state)
		{
			_voxelStateMap[voxelPosition.x,  voxelPosition.y, voxelPosition.z] = state;
		}

		private bool IsRepairing(Vector3Int voxelPosition)
		{
			return _voxelStateMap[voxelPosition.x, voxelPosition.y, voxelPosition.z] == VoxelState.Repairing;
		}
	}
}