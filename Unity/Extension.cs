namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public static partial class Extension
	{
		public static void Foreach(this Vector3Int self, System.Action<Vector3Int> callback)
		{
			for (var x = 0; x < self.x; x++)
			{
				for (var y = 0; y < self.y; y++)
				{
					for (var z = 0; z < self.z; z++)
					{
						callback?.Invoke(new Vector3Int(x, y, z));
					}
				}
			}
		}

		public static void Foreach<T>(this T[,,] array, System.Action<Vector3Int, T> callback)
		{
			array.Foreach((x, y, z, item) =>
			{
				callback(new Vector3Int(x, y, z), item);
			});
		}

		public static void Foreach<T>(this T[,,] array, System.Action<int, int, int, T> callback)
		{
			for (int x = 0; x < array.GetLength(0); x++)
			{
				for (int y = 0; y < array.GetLength(1); y++)
				{
					for (int z = 0; z < array.GetLength(2); z++)
					{
						callback?.Invoke(x, y, z, array[x, y, z]);
					}
				}
			}
		}

		public static bool TryGetHumanBodyBones(this string name, out HumanBodyBones bone)
		{
			return System.Enum.TryParse(name, out bone);
		}

		public static Bounds GetBounds(this IEnumerable<VoxToMesh.Vertex> vertices)
		{
			return vertices.Select(v => v.position).GetBounds();
		}

		public static Bounds GetBounds(this IEnumerable<VoxelVertexGenerator.Voxel> vertice)
		{
			return vertice.GetPoints().GetBounds();
		}

		public static IEnumerable<Vector3> GetPoints(this IEnumerable<VoxelVertexGenerator.Voxel> vertice)
		{
			foreach (var v in vertice)
			{
				foreach (var p in v.GetVertexPoints())
				{
					yield return p;
				}
			}
		}

		// https://github.com/takashicompany/utils/blob/master/Utils.csより
		public static Bounds GetBounds(this IEnumerable<Vector3> points)
		{
			var minX = float.MaxValue;
			var minY = float.MaxValue;
			var minZ = float.MaxValue;

			var maxX = float.MinValue;
			var maxY = float.MinValue;
			var maxZ = float.MinValue;

			foreach (var p in points)
			{
				if (minX > p.x) minX = p.x;
				if (minY > p.y) minY = p.y;
				if (minZ > p.z) minZ = p.z;

				if (maxX < p.x) maxX = p.x;
				if (maxY < p.y) maxY = p.y;
				if (maxZ < p.z) maxZ = p.z;
			}

			var size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
			var center = new Vector3(minX + size.x / 2, minY + size.y / 2, minZ + size.z / 2);

			return new Bounds(center, size);
		}

		public static Vector3 GetBoneConnectionPoint(this Bounds bounds, HumanBodyBones bone)
		{
			var dir = Vector3Int.zero;

			switch (bone)
			{
				case HumanBodyBones.Hips:
				case HumanBodyBones.Spine:
				case HumanBodyBones.Chest:
				case HumanBodyBones.Neck:
				case HumanBodyBones.Head:
					dir = Vector3Int.down;
					break;
					

				case HumanBodyBones.LeftUpperLeg:
				case HumanBodyBones.LeftLowerLeg:
				case HumanBodyBones.LeftFoot:
				case HumanBodyBones.RightUpperLeg:
				case HumanBodyBones.RightLowerLeg:
				case HumanBodyBones.RightFoot:
					dir = Vector3Int.up;
					break;

				case HumanBodyBones.LeftUpperArm:
				case HumanBodyBones.LeftLowerArm:
				case HumanBodyBones.LeftHand:
					dir = Vector3Int.right;
					break;

				case HumanBodyBones.RightUpperArm:
				case HumanBodyBones.RightLowerArm:
				case HumanBodyBones.RightHand:
					dir = Vector3Int.left;
					break;

				default:
					throw new System.NotImplementedException(bone + " is not implemented.");
			}

			if (dir == Vector3Int.down)
			{
				return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
			}
			else if (dir == Vector3Int.up)
			{
				return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
			}
			else if (dir == Vector3Int.left)
			{
				return new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
			}
			else if (dir == Vector3Int.right)
			{
				return new Vector3(bounds.max.x, bounds.center.y, bounds.center.z);
			}

			throw new System.NotImplementedException(dir + " is not valid.");
		}

		public static HumanBodyBones GetParentBone(this HumanBodyBones bone)
		{
			switch (bone)
			{
				case HumanBodyBones.Hips:
					return HumanBodyBones.LastBone;

				case HumanBodyBones.Spine:
					return HumanBodyBones.Hips;

				case HumanBodyBones.Chest:
					return HumanBodyBones.Spine;

				case HumanBodyBones.Neck:
					return HumanBodyBones.Chest;

				case HumanBodyBones.Head:
					return HumanBodyBones.Neck;

				case HumanBodyBones.LeftUpperLeg:
					return HumanBodyBones.Hips;

				case HumanBodyBones.LeftLowerLeg:
					return HumanBodyBones.LeftUpperLeg;

				case HumanBodyBones.LeftFoot:
					return HumanBodyBones.LeftLowerLeg;

				case HumanBodyBones.RightUpperLeg:
					return HumanBodyBones.Hips;
					
				case HumanBodyBones.RightLowerLeg:
					return HumanBodyBones.RightUpperLeg;

				case HumanBodyBones.RightFoot:
					return HumanBodyBones.RightLowerLeg;

				case HumanBodyBones.LeftUpperArm:
					return HumanBodyBones.Chest;

				case HumanBodyBones.LeftLowerArm:
					return HumanBodyBones.LeftUpperArm;

				case HumanBodyBones.LeftHand:
					return HumanBodyBones.LeftLowerArm;

				case HumanBodyBones.RightUpperArm:
					return HumanBodyBones.Chest;
				case HumanBodyBones.RightLowerArm:
					return HumanBodyBones.RightUpperArm;
				case HumanBodyBones.RightHand:
					return HumanBodyBones.RightLowerArm;
			}

			throw new System.NotFiniteNumberException(bone + " は未実装です");
		}
	}
}