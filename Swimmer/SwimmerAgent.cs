using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class SwimmerAgent : Agent
{
    [Header("Set Gameobject")]
    public GameObject area;

    [Header("move property")]
    private float step = 0.1f;
    private float angle = 1f;

    Vector3 agentInitPos;
    Bounds boundArea;
    RayPerception3D my_rayPer;
    RayPerception3DCone my_rayPerCone;

    float rayDistance = 15f;
    readonly float[] rayAngles = { 60f, 90f, 120f};
    readonly float[] rayAnglesVer = {15f ,0 ,-15f};
    readonly string[] detectableObjects = { "wall", "agent", "food" };

    public override void InitializeAgent()
    {
        my_rayPer = GetComponent<RayPerception3D>();
        my_rayPerCone = GetComponent<RayPerception3DCone>();
        agentInitPos = transform.position;
    }

    public override void CollectObservations()
    {
        var ray = my_rayPerCone.PerceiveCone(rayDistance, rayAngles, rayAnglesVer, detectableObjects);
        AddVectorObs(ray);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Debug.Log(vectorAction[0] + " " + vectorAction[1]);
        //0 = Speed , 1 =  Direction
        float speed = 1f +(vectorAction[0] * 0.5f); //1 or 1.5f
        agentAction(speed, vectorAction[1]);
        

        var count = 0;
        //周りのエージェントと地かければ報酬を与える
        foreach (Transform agent in SwimmerAcademy.agents)
        {
            var dist = Vector3.Distance(agent.position, transform.position);
            if (1 < dist && dist < 3f)
            {
                AddReward(0.001f);
                transform.GetComponent<MeshRenderer>().material.color = Color.red;
                count++;
            }
        }
        //周りにいなければマイナス
        if (count == 0)
        {
            AddReward(-0.001f);
            transform.GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }

    public override float[] Heuristic()
    {
        //set spped
        float[] rtnList = new float[2];

        rtnList[0] = 0.5f;
        if (Input.GetKey(KeyCode.W))
        {
            rtnList[0] = 1f;
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            rtnList[0] = 0.1f;
        }

        rtnList[1] = 4;
        //rtnList[1] = Mathf.FloorToInt(Random.Range(0, 4));
        //set direction
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rtnList[1] = 0;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rtnList[1] = 1;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            rtnList[1] = 2;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            rtnList[1] = 3;
        }

        return rtnList;
    }

    private void agentAction(float stepParm, float direction)
    { 

        switch(direction)
        {
            case 0:
              transform.RotateAround(transform.position, transform.up, -angle);
                break;

            case 1:
               transform.RotateAround(transform.position, transform.up, angle);
                break ;

            case 2:
               transform.RotateAround(transform.position,transform.right, -angle);
                break;

            case 3:
               transform.RotateAround(transform.position, transform.right, angle);
                break;
            case 4:
                break;
        }

       transform.position += transform.forward * step * stepParm;
    }

    // "wall", "agent", "food" 

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
           // transform.position = agentInitPos;
            AddReward(-1f);
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("food"))
        {
            Destroy(collision.gameObject);
            Resources.UnloadUnusedAssets();
            AddReward(1f);
        }
    }
}
