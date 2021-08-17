namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	[System.Serializable]
	public class VoxelMeshGenerator<T> where T : IVoxel, new()
	{

#if UNITY_EDITOR
		[SerializeField]
		private Object _voxFile;
#endif

		[SerializeField]
		private float _voxelUnitScale = 1f;

		public float voxelUnitScale => _voxelUnitScale;

		[SerializeField]
		private T[] _voxels;

		public T[] voxels => _voxels;

		[SerializeField]
		private Vector3Int _voxelSize;

		public Vector3Int voxelSize => _voxelSize;


		[ContextMenu("Load .vox")]
		private VoxLoader LoadVoxFile()
		{
			var path = "";
#if UNITY_EDITOR
			path = UnityEditor.AssetDatabase.GetAssetPath(_voxFile);
#endif
			var loader = new VoxLoader();

			var r = new CsharpVoxReader.VoxReader(path, loader);

			r.Read();

			return loader;
		}

		public void GenerateVoxel(bool isSingle)
		{
			var result = LoadVoxFile();
			GenerateVoxel(result.voxelMap, result.voxelBoneMap, _voxelUnitScale, isSingle);
		}

		private void GenerateVoxel(byte[,,] data, HumanBodyBones?[,,] bones, float unit, bool isSingle)
		{
			_voxelSize = new Vector3Int(data.GetLength(0), data.GetLength(1), data.GetLength(2));
			
			var unitHalf = unit / 2f;

			var offset = new Vector3((unit * _voxelSize.x) / 2f - unitHalf, - unitHalf, (unit * _voxelSize.z) / 2f - unitHalf) * -1;	// 0.5引いているのは、多分ボクセルの中心点の分

			var voxels = new List<T>();

			var totalVertexIndex = 0;
			var vertexIndexDict = new Dictionary<HumanBodyBones, int>();

			data.Foreach((v3, d) =>
			{
				if (d > 0)
				{
					var bone = bones[v3.x, v3.y, v3.z].HasValue ? bones[v3.x, v3.y, v3.z].Value : HumanBodyBones.LastBone;
					var voxel = new T();

					var vertexIndex = totalVertexIndex;

					if (!isSingle && !vertexIndexDict.TryGetValue(bone, out vertexIndex))
					{
						vertexIndexDict.Add(bone, 0);
						vertexIndex = 0;
					}

					voxel.Init(v3, new Vector3(v3.x * unit, v3.y * unit, v3.z * unit) + offset, _voxelUnitScale, bone, vertexIndex);
					
					voxels.Add(voxel);

					totalVertexIndex += 24;

					if (!isSingle)
					{
						vertexIndexDict[bone] += 24;
					}

					voxel.CreateCache();
				}
			});

			_voxels = voxels.ToArray();
		}

		public Mesh GenerateMesh()
		{
			var mesh = new Mesh();

			mesh.vertices = voxels.SelectMany(v => v.GetVertexPoints()).ToArray();

			var tris = new List<int>();

			foreach (var v in voxels)
			{
				tris.AddRange(v.GetTriangleIndices());
			}

			mesh.triangles = tris.ToArray();

			return mesh;
		}

		public Dictionary<HumanBodyBones, Mesh> GenerateMeshDict()
		{
			var vertexDict = new Dictionary<HumanBodyBones, List<Vector3>>();
			var triangleDict = new Dictionary<HumanBodyBones, List<int>>();

			var boundsDict = BuildBoundsDict();

			foreach (var v in voxels)
			{
				if (!vertexDict.TryGetValue(v.bone, out var vertice))
				{
					vertice = new List<Vector3>();
					vertexDict.Add(v.bone, vertice);
				}

				var points = v.GetVertexPoints().ToArray();

				var origin = boundsDict[v.bone].GetBoneConnectionPoint(v.bone);

				for (var i = 0; i < points.Length; i++)
				{
					var p = points[i];
					points[i] = p - origin;
				}

				vertice.AddRange(points);

				if (!triangleDict.TryGetValue(v.bone, out var indices))
				{
					indices = new List<int>();
					triangleDict.Add(v.bone, indices);
				}
				indices.AddRange(v.GetTriangleIndices());
			}

			var meshDict = new Dictionary<HumanBodyBones, Mesh>();

			foreach (var kvp in vertexDict)
			{
				var bone = kvp.Key;
				var vertice = kvp.Value;
				var triangleIndices = triangleDict[bone];

				var mesh = new Mesh();
				mesh.vertices = vertice.ToArray();
				mesh.triangles = triangleIndices.ToArray();

				meshDict.Add(bone, mesh);
			}

			return meshDict;
		}

		public Dictionary<HumanBodyBones, Bounds> BuildBoundsDict()
		{
			var dict = new Dictionary<HumanBodyBones, Bounds>();
			
			var boneTypes = new Dictionary<HumanBodyBones, List<IVoxel>>();

			foreach (var v in _voxels)
			{
				if (!boneTypes.ContainsKey(v.bone))
				{
					boneTypes.Add(v.bone, new List<IVoxel>() { v });
				}
				else
				{
					boneTypes[v.bone].Add(v);
				}
			}

			foreach (var kvp in boneTypes)
			{
				var key = kvp.Key;
				var list = kvp.Value;
				var bounds = list.GetBounds();
				dict.Add(key, bounds);
			}

			return dict;
		}

	}
}