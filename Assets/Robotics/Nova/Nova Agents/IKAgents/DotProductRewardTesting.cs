using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotProductRewardTesting : MonoBehaviour
{
    public Vector3 Vector0;
    public Vector3 Vector1;
    public float DotProduct;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector0 = Vector3.forward;
        Vector1 = transform.localScale;
        DotProduct = (Vector0 - Vector1).magnitude;
    }
}
