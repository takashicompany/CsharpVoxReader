namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public interface IVoxel
	{
		Vector3Int voxelPosition { get; }
		Vector3 positionFromCenter { get; }
		HumanBodyBones bone { get; }

		int x { get; }
		int y { get; }
		int z { get; }

		void CreateCache();

		IEnumerable<Vector3> GetVertexPoints();

		IEnumerable<int> GetTriangleIndices();
	}

	[System.Serializable]
	public class SimpleVoxel : IVoxel
	{
		public const int vertexCount = 4 * 6;
		public const int triangleCount = 3 * 2 * 6;

		[SerializeField]
		private Vector3Int _voxelPosition;

		public Vector3Int voxelPosition => _voxelPosition;

		[SerializeField]
		private Vector3 _positionFromCenter;

		public Vector3 positionFromCenter => _positionFromCenter;

		[SerializeField]
		private Vector3[] _vertice;

		[SerializeField]
		private int[] _vertexIndices;

		[SerializeField]
		private int[] _triangleIndices;

		[SerializeField]
		private HumanBodyBones _bone;

		public HumanBodyBones bone => _bone;

		public int x => voxelPosition.x;
		public int y => voxelPosition.y;
		public int z => voxelPosition.z;

		public SimpleVoxel(Vector3Int position, Vector3 positionFromCenter, float unitPerSize, HumanBodyBones bone, int startIndex)
		{
			_voxelPosition = position;

			_positionFromCenter = positionFromCenter;

			_vertice = new Vector3[4 * 6];
			_vertexIndices = new int[4 * 6];
			_triangleIndices = new int[3 * 2 * 6];

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
			
			var index = 0;

			Face(luf, lub, ldb, ldf, startIndex, ref index);	// left
			Face(rub, ruf, rdf, rdb, startIndex, ref index);	// right
			Face(ldb, rdb, rdf, ldf, startIndex, ref index);	// bottom
			Face(luf, ruf, rub, lub, startIndex, ref index);	// top
			Face(lub, rub, rdb, ldb, startIndex, ref index);	// back
			Face(ruf, luf, ldf, rdf, startIndex, ref index);	// forward

			_bone = bone;
		}

		private void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int startIndex, ref int index)
		{
			var triIndex = 6 * (index / 4);

			_vertexIndices[index] = startIndex + index;
			_vertice[index++] = a;
			
			_vertexIndices[index] = startIndex + index;
			_vertice[index++] = b;

			_vertexIndices[index] = startIndex + index;
			_vertice[index++] = c;

			_vertexIndices[index] = startIndex + index;
			_vertice[index++] = d;

			
			_triangleIndices[triIndex++] = startIndex + 0;
			_triangleIndices[triIndex++] = startIndex + 1;
			_triangleIndices[triIndex++] = startIndex + 2;
			_triangleIndices[triIndex++] = startIndex + 0;
			_triangleIndices[triIndex++] = startIndex + 2;
			_triangleIndices[triIndex++] = startIndex + 3;
		}

		public void CreateCache()
		{
			//throw new System.NotImplementedException();
		}

		public IEnumerable<int> GetTriangleIndices()
		{
			return _triangleIndices;
		}

		public IEnumerable<Vector3> GetVertexPoints()
		{
			return _vertice;
		}
	}

	public static partial class Extension
	{

	}
}