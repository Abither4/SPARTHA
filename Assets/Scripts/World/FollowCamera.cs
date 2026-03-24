using UnityEngine;

namespace Spartha.World
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 12f, -8f);
        public float smoothSpeed = 5f;
        public float lookAngle = 50f;

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 desiredPos = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(lookAngle, 0, 0);
        }
    }
}
