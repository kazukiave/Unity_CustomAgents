using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointInsideBoud : MonoBehaviour
{
    public GameObject target;
    Bounds boud;
    // Start is called before the first frame update
    void Start()
    {
        boud =  target.GetComponent<BoxCollider>().bounds;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right;
        }
        if (boud.Contains(transform.position))
        {
            target.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            target.GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }
}
