using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindGenerator : MonoBehaviour
{
    [SerializeField]
    private Rigidbody _rigidbody;

    [SerializeField]
    private Transform _gust;

    [SerializeField]
    private bool _translate;

    [SerializeField]
    private float _osselationY =10;
    [SerializeField]
    private float _osselationX =5;

    private LineRenderer _gustRenderer;

    public float minWind = 1;
    public float maxWind = 1;
    // Start is called before the first frame update
    void Start()
    {
        _gustRenderer = GetComponent<LineRenderer>();
    }

    private Vector3 _gustForce;
    private bool _gustActive;
    private float _gustTime;
    private float _gustStrength;
    //public IEnumerator GenerateGust()
    //{
      
    //    _gustActive = true;
    //    var gustLength = UnityEngine.Random.Range(5, 10);

    //    var gustDirY = UnityEngine.Random.Range(-_osselationX, _osselationX);
    //    var gustDirX = UnityEngine.Random.Range(-_osselationY, _osselationY);

    //  //  transform.localEulerAngles = new Vector3(0, gustDirY, 0);
    //    _gust.localEulerAngles = new Vector3(gustDirX, gustDirY, 0);

    //     _gustStrength = UnityEngine.Random.Range(minWind, maxWind);


    //    Debug.Log("start gust : " + gustLength);


    //  //  Debug.Log("gusting for seconds " + gustLength + " : gust power " + _gustForce);

    //    while (_gustActive)
    //    {
    //        _gustTime += Time.deltaTime;
          
    //        if (_gustTime > gustLength)
    //        {
    //            Debug.Log("end gusut " + _gustTime);
    //            _gustTime = 0;
    //            _gustActive = false;
    //        }

    //        yield return null;
    //    }
       
    //}

    private void FixedUpdate()
    {
        var tempPos = _rigidbody.transform.position;
        var tempPos1 = transform.position;
        tempPos1.y = tempPos.y;
        transform.position = tempPos1;

        var leadingPoint = _rigidbody.ClosestPointOnBounds(_rigidbody.transform.position + (transform.forward * -100));

       

       // var forceVector = Vector3.Lerp(_gust.transform.eulerAngles, _gustForce, Time.deltaTime);

        _rigidbody.AddForceAtPosition(_gustForce,leadingPoint);
       // _rigidbody.add
      //  _rigidbody.AddForce(_gustForce);

        if (!_translate)
        {
            _rigidbody.transform.position = tempPos;
        }

        _gustRenderer.SetPosition(0, leadingPoint);
        _gustRenderer.SetPosition(1, (leadingPoint + _gustForce));
        // var windX = UnityEngine.Random.Range(0, maxWind);

        //_rigidbody.AddTorque(new Vector3(0,0,windX* .001f));

        // var windZ = UnityEngine.Random.Range(0, maxWind);

        // _rigidbody.AddTorque(new Vector3(windZ * .001f,0,0));

        //if (_translate)
        //{
        //    _rigidbody.AddForce(new Vector3(-windX, 0, 0));
        //    _rigidbody.AddForce(new Vector3(0, 0, -windZ));
        //}
    }
    private float _gustLength;
    // Update is called once per frame
    void Update()
    {
        if (!_gustActive)
        {
            _gustActive = true;
            _gustLength = UnityEngine.Random.Range(5, 10);

            var gustDirY = UnityEngine.Random.Range(-_osselationY, _osselationY);
            var gustDirX = UnityEngine.Random.Range(-_osselationX, _osselationX);         

            _gust.localEulerAngles = new Vector3(gustDirX, gustDirY, 0);

            _gustStrength = UnityEngine.Random.Range(minWind, maxWind);

            _gustForce = (_gust.transform.forward * _gustStrength);     
            
            Debug.Log("Start gust : " + _gustForce);    
        }
        else
        {
                _gustTime += Time.deltaTime;

                if (_gustTime > _gustLength)
                {
                    Debug.Log("end gusut " + _gustTime);
                    _gustTime = 0;
                    _gustActive = false;
                }

  
        }
    }
}
