using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


public class AI : MonoBehaviour {
    private bool alive;
    public Vector3 linearVelocity, linearAcceleration;
    public float angularAcceleration, angularVelocity;
    public Vector3 maxLinearVelocity, maxLinearAcceleration;

    public float maxRotationSpeed = 10.0f;
    private GameObject circle;

    public float targetDistance;
    public float distance;

    private Transform currentTransform;
    public Vector3 dest;

    public Transform target;
    public GameObject wanderTarget;
    public Camera cam;
    public GameObject label;
    public string TYPE;
    private float radius;
    int dir;

    private int pathIndex;
    private GameObject[] pathPoints;

    public enum State
    {
        WANDERING,
        SEEKING,
        FLEEING,
        HUNTING,
        PURSUING,
        PATHFOLLOWING
    }
    public State state;

    // Use this for initialization
    void Start () {
        
        dir = 1;
        alive = true;
        radius = this.gameObject.GetComponent<SphereCollider>().radius / 2f;
        pathIndex = 0;
        currentTransform = this.gameObject.transform;
        if (state == State.WANDERING) setWanderTarget();
        cam = Camera.main;
        pathPoints = GameObject.FindGameObjectsWithTag("wayPoints");
        pathIndex = pathPoints.Length -1;

        // pathList = pathList.OrderBy<path => tile.Name).ToList();
        StartCoroutine("FSM");
	}
	//Finite State machine to hold logic for switching AI states.
    IEnumerator FSM()
    {
        while (alive)
        {
            switch (state)
            {
                case State.WANDERING:
                    Wander();
                    break;
                case State.SEEKING:
                    Seek();
                    break;
                case State.FLEEING:
                    Flee();
                    break;
                case State.PATHFOLLOWING:
                    Pathfollow();
                    break;
            }
            yield return null;
        }
    }
	// Update is called once per frame
	void Update () {
        FSM();
	}
    public void Pathfollow()
    {
        if (pathIndex < 0) pathIndex = pathPoints.Length - 1;
        //  Debug.Log(pathIndex);
        target = pathPoints[pathIndex].transform;
        distance = Vector3.Distance(target.position, currentTransform.position);
        //    seekTarget();
        //    Debug.Log(target.position - currentTransform.position);
        seekTarget();
        if (distance < 0.2f)
        {
            pathIndex--;
         //   seekTarget();
        }
    }
    void Flee()
    {
        linearAcceleration = currentTransform.position - target.position;
        linearAcceleration = clipValue(linearAcceleration, maxLinearAcceleration);

        linearVelocity += linearAcceleration;
        linearVelocity = clipValue(linearVelocity, maxLinearVelocity) ;
        angularAcceleration = 0;
        //angular acceleration = 0;
        align();
        if (checkBounds(currentTransform.position + linearVelocity * Time.deltaTime))
        {
            currentTransform.position += linearVelocity * Time.deltaTime;
        }
        else
        {
            currentTransform.position = new Vector3(0, 0);
        }
      //  currentTransform.position += linearVelocity * Time.deltaTime;

        distance = Vector3.Distance(currentTransform.position, dest);
    }
    void Seek()
    {
        seekTarget();

        distance = Vector3.Distance(currentTransform.position, dest);

        if (distance < 0.5f)
        {
         //   Debug.Log("arrived");
        }

    }
    void seekTarget()
    {
        linearAcceleration = target.position - currentTransform.position;
        linearAcceleration = clipValue(linearAcceleration, maxLinearAcceleration);

        linearVelocity += linearAcceleration;
        linearVelocity = clipValue(linearVelocity, maxLinearVelocity);

        angularAcceleration = 0;
        //align();
        currentTransform.position += linearVelocity * Time.deltaTime;

    }
    void setWanderTarget()
    {
        if (state == State.WANDERING)
        {

            if (this.gameObject.name.ToLower() == "hunter")
            {
                wanderTarget = GameObject.FindWithTag("hunterTarget");
            }
            if (this.gameObject.name.ToLower() == "wolf")
            {
                wanderTarget = GameObject.FindWithTag("wolfTarget");
            }
            circle = wanderTarget;
            target = wanderTarget.transform;
            Vector3 circlePos = calculateTargetPosition(radius);
            dest = target.transform.position + circlePos;
            circle.transform.position = currentTransform.transform.position + (currentTransform.right * targetDistance);

        }
    }
    void Wander()
    {
        
        linearAcceleration = (dest - currentTransform.position);
     //   linearAcceleration = clipValue(linearAcceleration, maxLinearAcceleration);

        linearVelocity = linearAcceleration;
       // linearVelocity = clipValue(linearVelocity, maxLinearVelocity);

        distance = Vector3.Distance(currentTransform.position, dest);

        currentTransform.position += linearVelocity * Time.deltaTime;

        if (linearVelocity != Vector3.zero) align();
        
        if (distance < 0.5f)
        {
            Vector3 circlePos = calculateTargetPosition(radius);
            circle.transform.position = currentTransform.transform.position + (currentTransform.right * targetDistance);
            Vector2 temp = (Vector2)circle.transform.position + (Vector2)circlePos;

            bool check = checkBounds(temp);
            if (check)
            {
                circle.transform.position = currentTransform.transform.position + (currentTransform.right * targetDistance) * dir;
                dest = (Vector2)circle.transform.position + (Vector2)circlePos;
                //  Debug.DrawRay(transform.position, (dest.normalized), Color.green, 10f, false);
            }
            else
            {
                dir *= -1;
            }
        }
    }
    void stopWandering()
    {
        //switch state

        //turn off target
        circle.SetActive(false);
    }
    Vector3 calculateTargetPosition(float radius)
    {
        Vector3 randomPointOnCircle = Random.insideUnitCircle;
        randomPointOnCircle.Normalize();
        randomPointOnCircle *= radius;
        return randomPointOnCircle;
    }
    Vector3 clipValue(Vector3 toClip, Vector3 clipRange)
    {
        Vector3 res;
        if (toClip.x > clipRange.x) toClip.x = clipRange.x;
        if (toClip.x < clipRange.x * -1) toClip.x = clipRange.x * -1;
        if (toClip.y > clipRange.y) toClip.y = clipRange.y;
        if (toClip.y < clipRange.y * -1) toClip.y = clipRange.y * -1;

        // toClip.x = Mathf.Clamp(toClip.x, clipRange.x * -1, clipRange.x);
       // toClip.y = Mathf.Clamp(toClip.y, clipRange.y * -1, clipRange.y);
        res = toClip;
        return res;
    }
    bool checkBounds(Vector3 pos)
    {

        Vector3 p1 = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 p2 = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
        Vector3 p3 = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        Vector3 p4 = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));

        if (pos.x < p1.x || pos.x > p2.x 
         || pos.y < p1.y || pos.y > p3.y)
        {
            return false;
        }

        float width = (p2 - p1).magnitude;
        float height = (p3 - p2).magnitude;

        return true;
    }
    void align()
    {
        angularVelocity += angularAcceleration;
        float angle = Mathf.Atan2(linearVelocity.y, linearVelocity.x) * Mathf.Rad2Deg;
        //rotate towards target
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        currentTransform.rotation = q;// Quaternion.RotateTowards(currentTransform.rotation, q, angularVelocity * Time.deltaTime);
    }
}
   
