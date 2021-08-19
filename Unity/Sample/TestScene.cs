namespace TakashiCompany.Unity.VoxReader.Sample
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class TestScene : MonoBehaviour
	{
		public static int index = 0;

		private VoxelHumanoid[] _voxelHumanoids;

		private void Start()
		{
			_voxelHumanoids = GameObject.FindObjectsOfType<VoxelHumanoid>(true);

			Change();
		}

		private void Update()
		{
			_voxelHumanoids[index].transform.Rotate(Vector3.up * 90 * Time.deltaTime);	
		}

		private void Change()
		{
			for (int i = 0; i < _voxelHumanoids.Length; i++)
			{
				_voxelHumanoids[i].gameObject.SetActive(index == i);
			}
		}

		private void OnGUI()
		{
			if (GUI.Button(new Rect(0, 0, 100, 100), "Reset"))
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene(0);
			}

			if (GUI.Button(new Rect(100, 0, 200, 100), _voxelHumanoids[index].name))
			{
				index++;

				if (_voxelHumanoids.Length <= index)
				{
					index = 0;
				}

				Change();
			}
		}
	}
}