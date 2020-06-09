using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class ChairAgent : Agent
{
    [Header("agent property")]
    public GameObject sphere;
    float step = 0.2f;
    float angle = 3f;
    Vector3 agentPrePos;
    Quaternion agentInitQua;
    private bool collisionOr = false;
    private float targetDist = 0;

    [Header("area property")]
    public GameObject fallObjct;
    private Rigidbody fallObjectRb;
    private Vector3 fallObjectInitPos;
    public int fallCount;
    public bool isDeform ;
    public GameObject area;
    private List<GameObject> spheres;
    private ChairAcademy chairAcademy;

    [Header("general")]
    public bool oneEpisode;
    


    Bounds areaBounds;
    DeformMesh deformMesh;
    RayPerception3DCone my_rayPerCone;

    float rayDistance = 120f;
    readonly float[] rayAngles = { 60f, 90f, 120f };
    readonly float[] rayAnglesVer = { 15f, 0, -15f };
    readonly string[] detectableObjects = { "Target", "agent"};

    private void FixedUpdate()
    {
        if (fallObjectRb != null)
        {
            fallObjectRb.AddForce(Vector3.down * 50f);
        }
    }

    public override void InitializeAgent()
    {
       var areaBox = area.GetComponent<BoxCollider>();
        areaBounds = areaBox.bounds;
        Destroy(areaBox);

        my_rayPerCone = GetComponent<RayPerception3DCone>();
        deformMesh = fallObjct.GetComponentInChildren<DeformMesh>();
        fallObjectRb = fallObjct.GetComponent<Rigidbody>();
        fallObjectInitPos = fallObjct.transform.position;
        spheres = new List<GameObject>();
        agentInitQua = transform.rotation;
        Monitor.SetActive(true);
        chairAcademy = FindObjectOfType<ChairAcademy>();
    }

    public override void CollectObservations()
    {
        var ray = my_rayPerCone.PerceiveCone(rayDistance, rayAngles, rayAnglesVer, detectableObjects);
        AddVectorObs(ray);

        AddVectorObs(targetDist);

        AddVectorObs(collisionOr);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
     
        AgentWalk( 3f, vectorAction[0]);

        //ある程度時間が立ったらオブジェクトを落下させる。
        if (GetStepCount() > fallCount)
        {
            fallObjectRb.isKinematic = false;
            fallObjectRb.useGravity = true;

            AddReward((fallObjct.transform.position.y / fallObjectInitPos.y) * 0.1f);
        }
        else
        {
            fallObjectRb.isKinematic = true;
            fallObjectRb.useGravity = false;
        }

        if (fallObjct.transform.position.y < (fallObjectInitPos.y * 0.666f))
        {
            AddReward(-1.0f);
            Done();
        }

        if (gameObject.transform.position.y < -5f)
        {
            AddReward(gameObject.transform.position.y * 0.001f);
        }
        

        Monitor.Log("reward", GetCumulativeReward().ToString());
      
    }

    public override float[] Heuristic()
    {
        //set spped
        float[] rtnList = new float[1];
  

        rtnList[0] = 4;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rtnList[0] = 0;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rtnList[0] = 1;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            rtnList[0] = 2;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            rtnList[0] = 3;
        }

        return rtnList;
    }

    private void AgentWalk(float stepSpeed, float direction)
    {

        switch (direction)
        {
            case 0:
                transform.RotateAround(transform.position, transform.up, -angle);
                break;

            case 1:
                transform.RotateAround(transform.position, transform.up, angle);
                break;

            case 2:
                transform.RotateAround(transform.position, transform.right, -angle);
                break;

            case 3:
                transform.RotateAround(transform.position, transform.right, angle);
                break;
            case 4:
                break;
        }
        
      
        var nextPos = transform.position + transform.forward * step * stepSpeed;
        var nextPosClosed = fallObjct.GetComponent<MeshCollider>().ClosestPoint(nextPos);
        var dist = Vector3.Distance(nextPos, nextPosClosed);
        targetDist = dist;

        //Targetに近すぎなければAgentを移動させる
        if (dist > 0.1f)
        {
            collisionOr = false;
            agentPrePos = transform.position;

            chairAcademy.spheres.Add(Instantiate(sphere, transform.position, Quaternion.identity));
            chairAcademy.spheresPos.Add(transform.position);
            //   spheres.Add(Instantiate(sphere, transform.position, Quaternion.identity));
            transform.position += transform.forward * step * stepSpeed;
            AddReward(0.0001f);
        }
        else//ターゲットに突っ込んでるとき
        {
            collisionOr = true;
        }
    }


    public override void AgentReset()
    {
        if (oneEpisode && IsDone())
        {
            UnityEditor.EditorApplication.isPaused = true;
            chairAcademy.SavePosition();
            return;
        }

        if (isDeform == true)
        {
            deformMesh.Deform();
        }

        float offsetVal0 = Random.Range(-20f, 20f);
        float offsetVal1 = Random.Range(-20f, 20f);
        float offsetVal2 = Random.Range(-20f, 20f);
        fallObjct.transform.position = fallObjectInitPos + new Vector3(offsetVal0, offsetVal1, offsetVal2);

        DestroyAllSphere();
        transform.position = MakeRandomPos();
        transform.rotation = agentInitQua;

        var closedPt = fallObjct.GetComponent<MeshCollider>().ClosestPoint(transform.position);
        targetDist = Vector3.Distance(transform.position, closedPt);
    }

    private Vector3 MakeRandomPos()
    {
        var x = Random.Range(areaBounds.min.x, areaBounds.max.x);
        var z = Random.Range(areaBounds.min.z, areaBounds.max.z);
        var pos = new Vector3(x, 0, z);
        return pos;
    }

    private void DestroyAllSphere()
    {
        if (chairAcademy.spheres.Count > 0)
        {
            foreach (GameObject sphere in chairAcademy.spheres)
            {
                Destroy(sphere);
            }
        }
    }


}
