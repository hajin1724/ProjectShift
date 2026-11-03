using UnityEngine;

namespace ProjectShift
{
    public sealed class SoldierMovement : MonoBehaviour
    {
        public void Follow(Vector3 target, Quaternion settledFacing, float maxStep)
        {
            Vector3 direction = target - transform.position;
            Quaternion facing = direction.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(direction) : settledFacing;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, 540f * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, target, maxStep);
        }
    }
}
