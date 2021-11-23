using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNan : MonoBehaviour
{
    Matrix4x4 textMatrix;
    public Transform target;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        textMatrix = new Matrix4x4();
    }

    // Update is called once per frame
    void Update()
    {
        textMatrix.SetTRS(transform.position, transform.rotation, Vector3.one);

        offset = textMatrix.inverse.MultiplyPoint(target.position);
    }
}
