namespace TakashiCompany.Unity.VoxReader.Sample
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class Shooter : MonoBehaviour
	{
		[SerializeField]
		private Bullet _prefab;

		private void Awake()
		{
			Application.targetFrameRate = 60;
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				var bullet = Instantiate(_prefab, transform);

				bullet.Shoot(ray);
				
			}
		}
	}
}