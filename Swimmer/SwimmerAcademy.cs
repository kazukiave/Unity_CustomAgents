using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class SwimmerAcademy : Academy
{
    public GameObject food;
    public GameObject area;

    private Bounds areaBound;

    public static List<Transform> agents = new List<Transform>();
    public static List<GameObject> foods = new List<GameObject>();
    /*
    private void FixedUpdate()
    {
        if (foods.Count > 0)
        {
            foreach (GameObject food in foods)
            {
                food.GetComponent<Rigidbody>().AddForce(Vector3.down * 0.1f, ForceMode.Acceleration);
            }
        }
    }*/

    public override void InitializeAcademy()
    {
       // agents.Capacity = 10;
        var child = gameObject.GetComponentsInChildren<Transform>();
        foreach(Transform obj in child)
        {
            if (obj.CompareTag("agent"))
            {
                agents.Add(obj);
            }
        }
        agents.Capacity = agents.Count;
       areaBound =  area.GetComponent<BoxCollider>().bounds;
    }

    public override void AcademyStep()
    {
        if (food == null) return;

        var posX = Random.Range(areaBound.min.x, areaBound.max.x);
        var posY = areaBound.max.y;
        var posZ = Random.Range(areaBound.min.z, areaBound.max.z);
        var pos = new Vector3(posX, posY, posZ);

        if (Random.Range(0,50)  == 0)// 1/100
        {
            var obj = Instantiate(food, pos , Quaternion.identity, transform);
            obj.SetActive(true);
            foods.Add(obj);
        }
     
        if (foods.Count != 0)
        {
           for(int i = 0; i < foods.Count; i++)
            {
                if (foods[i] == null)
                {
                    foods.RemoveAt(i);
                }
                else if (areaBound.Contains(foods[i].transform.position) != true)
                {
                    Destroy(foods[i]);
                    foods.RemoveAt(i);
                    Resources.UnloadUnusedAssets();
                }
            }
        }
    }

    public override void AcademyReset()
    {
       
    }


  
}
