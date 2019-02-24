using UnityEngine;

namespace VARP.KochFractals
{
    public class RotateCamera : MonoBehaviour
    {
        public AudioPeer audioPeer;
        public Vector3 rotateAxis1 = Vector3.up;
        public Vector3 rotateAxis2 = Vector3.right;
        public float rotateSpeed = 10;

        private void Update()
        {
            transform.RotateAround(Vector3.Lerp(rotateAxis1, rotateAxis2, audioPeer.amplitude), rotateSpeed * Time.deltaTime);
        }
    }
}