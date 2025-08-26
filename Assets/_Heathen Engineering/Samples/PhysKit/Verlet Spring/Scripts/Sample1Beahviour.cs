#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.Demos
{
    /// <summary>
    /// This is for demonstration purposes only
    /// </summary>
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Sample1Beahviour : MonoBehaviour
    {
        public Transform ball;
        public Transform start;
        public Transform target;
        public GameObject prefab;
        public float vel;

        private void Update()
        {
            var plane = new Plane(Vector3.up, Vector3.zero);
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float distance))
            {
                ball.transform.position = ray.GetPoint(distance);
            }
        }
    }
}

#endif
