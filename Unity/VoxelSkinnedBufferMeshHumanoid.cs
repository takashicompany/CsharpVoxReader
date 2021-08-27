namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelSkinnedBufferMeshHumanoid : VoxelHumanoid<SimpleVoxel>
	{
		[SerializeField]
		protected SkinnedMeshRenderer _renderer;

		private HashSet<Vector3Int> _updateList = new HashSet<Vector3Int>();

		protected override void Awake()
		{
			base.Awake();
			var mesh = _renderer.sharedMesh;

			var tris = new List<int>();
			// Debug.Log("vg:" + (_vertexGenerator != null));
			// Debug.Log("vv:" + (_vertexGenerator.voxels != null));
			// Debug.Log("va:" + (_voxelActive != null));
			
			foreach (var v in _vertexGenerator.voxels)
			{
				tris.AddRange(v.GetTriangleIndices());
			}

			mesh.SetIndexBufferParams(tris.Count, UnityEngine.Rendering.IndexFormat.UInt32);
			mesh.SetIndexBufferData(tris.ToArray(), 0, 0, tris.Count);
		}

		private void UpdateMesh()
		{
			var mesh = _renderer.sharedMesh;

			foreach (var v3int in _updateList)
			{
				var v = GetVoxel(v3int.x, v3int.y, v3int.z);
				// mesh.SetIndexBufferData(
			}

			_updateList.Clear();
		}

		protected override void OnChangeVoxelActive(int x, int y, int z, bool active)
		{
			base.OnChangeVoxelActive(x, y, z, active);

			_updateList.Add(new Vector3Int(x, y, z));
		}

		[ContextMenu("generate all")]
		private void　Generate()
		{
			_vertexGenerator.GenerateVoxel(true);

			GenerateBones();

			var boneIndexDict = new Dictionary<HumanBodyBones, int>();

			Debug.Assert(VoxToMesh.boneNames.Length == _bones.Length, "ボーンの数が不正です。" + VoxToMesh.boneNames.Length + "/" + _bones.Length);

			for (int i = 0; i < VoxToMesh.boneNames.Length; i++)
			{
				var bn = VoxToMesh.boneNames[i];
				boneIndexDict[bn] = i;
			}

			var mesh = _vertexGenerator.GenerateMesh();

			var boneWeights = new List<BoneWeight>();

			foreach (var v in _vertexGenerator.voxels)
			{
				foreach (var p in v.GetVertexPoints())
				{
					var index = boneIndexDict[v.bone];
					var bw = new BoneWeight();
					bw.boneIndex0 = index;
					bw.weight0 = 1f;
					boneWeights.Add(bw);
				}
			}

			mesh.boneWeights = boneWeights.ToArray();

			Matrix4x4[] bindPoses = new Matrix4x4[_bones.Length];

			for (int i = 0; i < bindPoses.Length; i++)
			{
				var bone = _bones[i];
				bindPoses[i] = bone.worldToLocalMatrix * transform.localToWorldMatrix;
			}

			mesh.bindposes = bindPoses;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();

			_renderer.bones = _bones;
			_renderer.sharedMesh = mesh;

			SetUpAvatar();
		}

		public override void Damage(Vector3 position, float radius)
		{
			base.Damage(position, radius);
			UpdateMesh();
		}

		private void OnDrawGizmos()
		{
			// if (_voxelPositionMap != null)
			// {
			// 	Gizmos.color = Color.red;

			// 	foreach (var v in _vertexGenerator.voxels)
			// 	{
			// 		var p = _voxelPositionMap[v.x, v.y, v.z];

			// 		if (_boneDict.TryGetValue(v.bone, out var bone))
			// 		{
			// 			var wp = bone.TransformPoint(p);

			// 			Gizmos.DrawWireSphere(wp, _vertexGenerator.voxelUnitScale / 2);
			// 		}
			// 	}
			// }
		}
	}
}