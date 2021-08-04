namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelVertexGenerator : MonoBehaviour
	{
		[System.Serializable]
		public class Vertex
		{
			[SerializeField]
			private Vector3 _position;

			public Vector3 position => _position;

			[SerializeField]
			private int _index;

			public int index => _index;

			public Vertex(Vector3 position)
			{
				_position = position;
			}

			public void SetIndex(int index)
			{
				_index = index;
			}	
		}
		
		[System.Serializable]
		public class Face
		{
			[SerializeField]
			private Vertex[] _vertice;

			public Vertex[] vertice => _vertice;

			private static readonly int[] _triangleIndices = new int[] { 0, 1, 2, 0, 2, 3};

			public Face(params Vector3[] points)
			{
				Debug.Assert(points.Length == 4, "頂点の数が不正です。:" + points.Length);

				_vertice = new Vertex[points.Length];

				for (int i = 0; i < points.Length; i++)
				{
					var v = new Vertex(points[i]);
					
					_vertice[i] = v;
				}
			}

			public IEnumerable<int> GetTriangleIndices()
			{
				foreach (var i in _triangleIndices)
				{
					yield return vertice[i].index;
				}
			}
		}
		
		[System.Serializable]
		public class Voxel
		{
			[SerializeField]
			private Vector3Int _voxelPosition;

			public Vector3Int voxelPosition => _voxelPosition;

			[SerializeField]
			private Vector3 _positionFromCenter;

			public Vector3 positionFromCenter => _positionFromCenter;

			public int x => voxelPosition.x;
			public int y => voxelPosition.y;
			public int z => voxelPosition.z;

			[SerializeField]
			private Face[] _faces;

			public Face[] faces => _faces;

			[SerializeField]
			private HumanBodyBones _bone;

			public HumanBodyBones bone => _bone;
			
			public Voxel(Vector3Int position, Vector3 positionFromCenter, float unitPerSize, HumanBodyBones bone)
			{
				_voxelPosition = position;

				_positionFromCenter = positionFromCenter;

				var min = positionFromCenter - Vector3.one * unitPerSize / 2;
				var max = positionFromCenter + Vector3.one * unitPerSize / 2;

				// (0,0,0) => (0,0,1) => (0,1,0) => (0,1,1) => (1,0,0) => (1,0,1) => (1,1,0) => (1,1,1)
				var ldb = new Vector3(min.x, min.y, min.z);	// 左下後
				var ldf = new Vector3(min.x, min.y, max.z);	// 左下前
				var lub = new Vector3(min.x, max.y, min.z);	// 左上後
				var luf = new Vector3(min.x, max.y, max.z);	// 左上前
				var rdb = new Vector3(max.x, min.y, min.z);	// 右下後
				var rdf = new Vector3(max.x, min.y, max.z);	// 右下前
				var rub = new Vector3(max.x, max.y, min.z);	// 右上後
				var ruf = new Vector3(max.x, max.y, max.z);	// 右上前
				
				var left = new Face(luf, lub, ldb, ldf);
				var right = new Face(rub, ruf, rdf, rdb);
				var down = new Face(ldb, rdb, rdf, ldf);
				var up = new Face(luf, ruf, rub, lub);
				var back = new Face(lub, rub, rdb, ldb);
				var forward = new Face(ruf, luf, ldf, rdf);

				_faces = new Face[] { left, right, down, up, back, forward };

				_bone = bone;
			}

			public IEnumerable<Vertex> GetVertex()
			{
				foreach (var f in _faces)
				{
					foreach (var v in f.vertice)
					{
						yield return v;
					}
				}
			}

			public IEnumerable<Vector3> GetVertexPoints()
			{
				foreach (var f in _faces)
				{
					foreach (var v in f.vertice)
					{
						yield return v.position;
					}
				}
			}

			public IEnumerable<int> GetTriangleIndices()
			{
				foreach (var f in _faces)
				{
					foreach (var ti in f.GetTriangleIndices())
					{
						yield return ti;
					}
				}
			}
		}

#if UNITY_EDITOR
		[SerializeField]
		private Object _voxFile;
#endif

		[SerializeField]
		private float _voxelUnitScale = 1f;

		public float voxelUnitScale => _voxelUnitScale;

		[SerializeField]
		private Voxel[] _voxels;

		public Voxel[] voxels => _voxels;

		[SerializeField]
		private Vector3Int _voxelSize;

		public Vector3Int voxelSize => _voxelSize;

#if UNITY_EDITOR

		[ContextMenu("Load .vox")]
		public void LoadVoxFile()
		{
			var path = UnityEditor.AssetDatabase.GetAssetPath(_voxFile);

			var loader = new VoxLoader();

			var r = new CsharpVoxReader.VoxReader(path, loader);

			r.Read();

			GenerateVoxel(loader.voxelMap, loader.voxelBoneMap, _voxelUnitScale);
		}
#endif

		private void GenerateVoxel(byte[,,] data, HumanBodyBones?[,,] bones, float unit)
		{
			_voxelSize = new Vector3Int(data.GetLength(0), data.GetLength(1), data.GetLength(2));
			
			var unitHalf = unit / 2f;

			var offset = new Vector3((unit * _voxelSize.x) / 2f - unitHalf, - unitHalf, (unit * _voxelSize.z) / 2f - unitHalf) * -1;	// 0.5引いているのは、多分ボクセルの中心点の分

			var voxels = new List<Voxel>();

			var vertexIndex = 0;

			data.Foreach((v3, d) =>
			{
				if (d > 0)
				{
					var bone = bones[v3.x, v3.y, v3.z].HasValue ? bones[v3.x, v3.y, v3.z].Value : HumanBodyBones.LastBone;
					var voxel = new Voxel(v3, new Vector3(v3.x * unit, v3.y * unit, v3.z * unit) + offset, _voxelUnitScale, bone);
					
					voxels.Add(voxel);

					foreach (var v in voxel.GetVertex())
					{
						v.SetIndex(vertexIndex);
						vertexIndex++;
					}
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

		public Dictionary<HumanBodyBones, Bounds> BuildBoundsDict()
		{
			var dict = new Dictionary<HumanBodyBones, Bounds>();
			
			var boneTypes = new Dictionary<HumanBodyBones, List<VoxelVertexGenerator.Voxel>>();

			foreach (var v in _voxels)
			{
				if (!boneTypes.ContainsKey(v.bone))
				{
					boneTypes.Add(v.bone, new List<VoxelVertexGenerator.Voxel>() { v });
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