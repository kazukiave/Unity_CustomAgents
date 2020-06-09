using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class RandomWalkAgent : Agent
{
    private Vector3 TargetInitPos;
    private Vector3 AgentIntPos;

    public GameObject Target;
    private Bounds boundsTarget;

    public GameObject cube;
    public int step = 1;
    private List<Vector3> storePost = new List<Vector3>();
    private List<GameObject> cubes = new List<GameObject>();
    private Rigidbody TargetRb;

    private void FixedUpdate()
    {
        if (TargetRb != null)
        {
     TargetRb.AddForce(Vector3.down * 100, ForceMode.Acceleration);
        }
    }

    public override void InitializeAgent()
    {
        storePost.Add(Vector3.zero);
        var box = Target.AddComponent<BoxCollider>();
        boundsTarget = Target.GetComponent<BoxCollider>().bounds;
        Destroy(box);
       // Target.GetComponent<BoxCollider>().enabled = false;
        TargetRb = Target.GetComponent<Rigidbody>();
        TargetRb.isKinematic = true;
        TargetRb.useGravity = false;

        TargetInitPos = Target.transform.position;
        AgentIntPos = transform.position;
    }

    int[] mask = new int[2] {0,4};
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        SetActionMask(0, mask);
        float dist = Vector3.Distance(TargetInitPos, transform.position)/20f;

        if (dist > 2f)
        {
            AddReward( -0.01f);
        }
      
        else
        {
           AddReward(0.001f);
        }

        if (dist > 2.0f)
        {
            AddReward( -1f);
            Done();
        }

         if (GetStepCount() > 300)
           {
               TargetRb.useGravity = true;
               TargetRb.isKinematic = false;
            AddReward(Mathf.Abs(Target.transform.position.y /( TargetInitPos.y))*0.1f);
           }
           else
           {
               TargetRb.useGravity = false;
               TargetRb.isKinematic = true;
           }
     

        if (Target.gameObject.transform.position.y < 8f)
        {
            AddReward(-0.2f);
            Done();
        }

        MoveAgent(vectorAction[0]);
        /*   Debug.Log(GetCumulativeReward());
          Debug.Log("dist" + dist);*/
     //  Debug.Log("vectorAction[0]" + vectorAction[0]);
        // Debug.Log(Mathf.Abs(Target.transform.position.y / (TargetInitPos.y)) * 0.1f);
    }

    public override float[] Heuristic()
    {
        float[] action = new float[1];
        if (Input.GetKey(KeyCode.R))
        {
            action[0] = Random.Range(0, 7);
        }
        else 
        {
          // action[0] = 0;
        }
        SetActionMask(0, 0);
        return action;
    }
    
    public override void CollectObservations()
    {
        float dist = Vector3.Distance(Target.transform.position, transform.position)/20f;
        AddVectorObs(dist);


        var hitOr = raySixAxis();
        /*
        var str = "";
        foreach (int num in hitOr)
        {
            str += num.ToString();
        }
        Debug.Log(str);
        */
        AddVectorObs(hitOr[0]);
        AddVectorObs(hitOr[1]);
        AddVectorObs(hitOr[2]);
        AddVectorObs(hitOr[3]);
        AddVectorObs(hitOr[4]);
        AddVectorObs(hitOr[5]);

        SetActionMask(0, mask);
    }

    public override void AgentOnDone()
    {
        inite();
    }

    public override void AgentReset()
    {
        inite();
    }

    private void inite()
    {
        transform.position = AgentIntPos;
        TargetRb.isKinematic = true;
        TargetRb.useGravity = false;
        Target.transform.position = TargetInitPos;
        TargetRb.isKinematic = false;
        foreach (GameObject _cube in cubes)
        {
            Destroy(_cube);
        }
        storePost.Clear();
        storePost.Add(AgentIntPos);
       
    }

    public void MoveAgent(float index)
    {
        Vector3 newVec = new Vector3();
        switch (index)
        {
            case 0:
               
                break;

            case 1:
                newVec = new Vector3(step, 0, 0);
                break;

            case 2:
                newVec = new Vector3(step * -1f, 0, 0);
                break;

            case 3:
                newVec = new Vector3(0, step, 0);
                break;

            case 4:
                newVec = new Vector3(0, step * -1f, 0);
                break;

            case 5:
                newVec = new Vector3(0, 0, step);
                break;

            case 6:
                newVec = new Vector3(0, 0, step * -1f);
                break;
        }
        newVec += transform.position;

        //もしAgentの位置が変わるなら
        if (storePost.Contains(newVec) == false && boundsTarget.Contains(newVec) == false)
        {
            cubes.Add(Instantiate(cube, transform.position, Quaternion.identity));
            transform.position = newVec;
            storePost.Add(newVec);
            AddReward(0.0001f);
        }
        else
        {
           
        }
    }

    public int[] raySixAxis()
    {
        Vector3[] direction = new Vector3[6] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        int[] hitOr = new int[6];
        for (int i = 0; i < direction.Length; i++)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, direction[i]);

            if (Physics.Raycast(ray, out hit, (float)step*2))
            {
                if (hit.transform.tag == "Obstacles")
                {
                    hitOr[i] = 1;
                }
                else if (hit.transform.tag == "Target")
                {
                    hitOr[i] = 2;
                }
            }
            else
            {
                hitOr[i] = 0;
            }

        }


        return hitOr;
    }

}
