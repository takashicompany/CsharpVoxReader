namespace TakashiCompany.Unity.VoxReader.Sample
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class Bullet : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody _rigidbody;

		[SerializeField]
		private float _speed = 10f;

		public void Shoot(Ray ray)
		{
			transform.position = ray.origin;
			transform.forward = ray.direction.normalized;
			_rigidbody.velocity = ray.direction.normalized * _speed;
		}

		private void OnCollisionEnter(Collision collision)
		{
			var human = collision.collider.GetComponentInParent<VoxelSkinnedMeshHumanoid>();

			if (human != null)
			{
				Destroy(this.gameObject);
			}
		}
	}
}