using UnityEngine;

namespace Toolkit.Utilities
{
    public class FaceCameraUI : MonoBehaviour
    {
        [SerializeField] private Transform _targetTransform;

        // Update is called once per frame
        void Update()
        {
           // transform.LookAt(Vector3.up, _targetTransform.position);//
            transform.rotation = Quaternion.LookRotation(Vector3.up, _targetTransform.position - transform.position);
        }
    } 
}
