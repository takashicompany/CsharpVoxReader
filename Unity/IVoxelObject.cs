namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public interface IVoxelObject
	{
		Vector3Int voxelSize { get; }
		IVoxel[] voxels { get; }
		public delegate void VoxelDestroyDelegate(IVoxel voxel, Vector3 worldPoint, Vector3 center);
		event VoxelDestroyDelegate onVoxelDestroyEvent;
	
		bool IsActiveVoxel(Vector3Int voxelPosition);
		Vector3 GetVoxelWorldPosition(Vector3Int voxelPosition);
		void Damage(Vector3 center, float radius);
		void ChangeVoxelActiveAll(bool active);
		void ChangeVoxelActive(Vector3Int voxelPosition, bool active);
		void RequestUpdateMesh();
	}

	public static class IVoxelObjectExtension
	{
		public static List<IVoxel> GenerateVoxelList(this IVoxelObject voxelObject, bool active)
		{
			var list = new List<IVoxel>();

			foreach (var v in voxelObject.voxels)
			{
				if (voxelObject.IsActiveVoxel(v.voxelPosition) == active)
				{
					list.Append(v);
				}
			}

			return list;
		}
	}
}