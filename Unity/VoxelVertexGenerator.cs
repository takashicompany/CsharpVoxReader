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

			public Vertex(Vector3 position, int index)
			{
				_position = position;
				_index = index;
			}
		}
		
		[System.Serializable]
		public class Face
		{
			[SerializeField]
			private Vertex[] _vertice;

			public Vertex[] vertice => _vertice;

			private static readonly int[] _triangleIndexOrder = new int[] { 0, 1, 2, 0, 2, 3};

			private int[] _triangleIndices;

			public Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int startIndex)
			{
				_vertice = new Vertex[]{
					new Vertex(a, startIndex + 0),
					new Vertex(b, startIndex + 1),
					new Vertex(c, startIndex + 2),
					new Vertex(d, startIndex + 3)
				};
			}

			// public void FixTriangleIndices()
			// {
			// 	_triangleIndices = new int[]
			// 	{
			// 		_vertice[_triangleIndexOrder[0]].index,
			// 		_vertice[_triangleIndexOrder[1]].index,
			// 		_vertice[_triangleIndexOrder[2]].index,
			// 		_vertice[_triangleIndexOrder[3]].index,
			// 		_vertice[_triangleIndexOrder[4]].index,
			// 		_vertice[_triangleIndexOrder[5]].index
			// 	};
			// }

			public IEnumerable<int> GetTriangleIndices()
			{
				// foreach (var i in _triangleIndexOrder)
				// {
				// 	yield return vertice[i].index;
				// }

				_triangleIndices = new int[]
				{
					_vertice[_triangleIndexOrder[0]].index,
					_vertice[_triangleIndexOrder[1]].index,
					_vertice[_triangleIndexOrder[2]].index,
					_vertice[_triangleIndexOrder[3]].index,
					_vertice[_triangleIndexOrder[4]].index,
					_vertice[_triangleIndexOrder[5]].index
				};

				return _triangleIndices;
			}
		}
		
		[System.Serializable]
		private class Voxel : IVoxel
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

			private Vertex[] _vertice;
			private Vector3[] _vertexPoints;
			private int[] _triangleIndices;
			
			public Voxel(Vector3Int position, Vector3 positionFromCenter, float unitPerSize, HumanBodyBones bone, int startIndex)
			{
				Init(position, positionFromCenter, unitPerSize, bone, startIndex);
			}

			public void Init(Vector3Int position, Vector3 positionFromCenter, float unitPerSize, HumanBodyBones bone, int startIndex)
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
				
				var left = new Face(luf, lub, ldb, ldf, startIndex + 0);
				var right = new Face(rub, ruf, rdf, rdb, startIndex + 4);
				var down = new Face(ldb, rdb, rdf, ldf, startIndex + 8);
				var up = new Face(luf, ruf, rub, lub, startIndex + 12);
				var back = new Face(lub, rub, rdb, ldb, startIndex + 16);
				var forward = new Face(ruf, luf, ldf, rdf, startIndex + 20);

				_faces = new Face[]{ left, right, down, up, back, forward };

				_bone = bone;
			}

			public void CreateCache()
			{
				if (_vertice != null && _vertexPoints != null && _triangleIndices != null)
				{
					return;
				}

				// 事前に値を持っておく
				_vertice = new Vertex[_faces.Length * 4];
				_vertexPoints = new Vector3[_faces.Length * 4];
				_triangleIndices = new int[_faces.Length * 6];

				var vertexIndex = 0;
				var triangleIndex = 0;

				foreach (var f in _faces)
				{
					foreach (var v in f.vertice)
					{
						_vertice[vertexIndex] = v;
						_vertexPoints[vertexIndex] = v.position;
						vertexIndex++;
					}

					foreach (var t in f.GetTriangleIndices())
					{
						_triangleIndices[triangleIndex] = t;
						triangleIndex++;
					}
				}

			}

			public IEnumerable<Vertex> GetVertex()
			{
				// if (_vertice == null)
				// {
				// 	CreateCache();
				// }

				return _vertice;

				// foreach (var f in _faces)
				// {
				// 	foreach (var v in f.vertice)
				// 	{
				// 		yield return v;
				// 	}
				// }
			}

			public IEnumerable<Vector3> GetVertexPoints()
			{
				// if (_vertexPoints == null)
				// {
				// 	CreateCache();
				// }

				return _vertexPoints;

				// foreach (var f in _faces)
				// {
				// 	foreach (var v in f.vertice)
				// 	{
				// 		yield return v.position;
				// 	}
				// }
			}

			public IEnumerable<int> GetTriangleIndices()
			{
				// if (_triangleIndices == null)
				// {
				// 	CreateCache();
				// }

				return _triangleIndices;
				
				// foreach (var f in _faces)
				// {
				// 	foreach (var ti in f.GetTriangleIndices())
				// 	{
				// 		yield return ti;
				// 	}
				// }
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

		public IVoxel[] voxels => _voxels;

		[SerializeField]
		private Vector3Int _voxelSize;

		public Vector3Int voxelSize => _voxelSize;



		[ContextMenu("Load .vox")]
		public void LoadVoxFile()
		{
			var path = "";
#if UNITY_EDITOR
			path = UnityEditor.AssetDatabase.GetAssetPath(_voxFile);
#endif
			var loader = new VoxLoader();

			var r = new CsharpVoxReader.VoxReader(path, loader);

			r.Read();

			GenerateVoxel(loader.voxelMap, loader.voxelBoneMap, _voxelUnitScale);
		}


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
					var voxel = new Voxel(v3, new Vector3(v3.x * unit, v3.y * unit, v3.z * unit) + offset, _voxelUnitScale, bone, vertexIndex);
					
					voxels.Add(voxel);

					vertexIndex += 24;

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