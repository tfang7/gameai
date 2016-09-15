using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class AI : MonoBehaviour {
    private bool alive;
    public Vector3 linearVelocity, linearAcceleration,
                   angularVelocity;
    public Quaternion angularRotation;
    public Vector3 maxLinearVelocity;
    public Vector3 maxLinearAcceleration;
    public float maxRotationSpeed = 10.0f;

    private GameObject circle;
    public float targetDistance;
    public float distance;

    private Transform currentTransform;
    public Vector3 dest;

    public Transform target;
    public GameObject huntersTarget;
    public Camera cam;
    public GameObject label;
    public string TYPE;
    int dir;

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
        currentTransform = this.gameObject.transform;
        if  (state == State.WANDERING)
        {
            huntersTarget = GameObject.FindWithTag("hunterTarget");
            circle = huntersTarget;
            Vector3 circlePos = calculateTargetPosition(0.5f);
            dest = target.transform.position + circlePos;
            circle.transform.position = currentTransform.transform.position + (currentTransform.right * targetDistance);
        }
        cam = Camera.main;
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
            }
            yield return null;
        }
    }
	// Update is called once per frame
	void Update () {
        FSM();
	}
    void Pathfollow()
    {

    }
    void Flee()
    {
        dest = target.transform.position;
        linearAcceleration = currentTransform.position - dest;
        linearAcceleration = clipValue(linearAcceleration, maxLinearAcceleration);

        linearVelocity += linearAcceleration;
        linearVelocity = clipValue(linearVelocity, maxLinearVelocity);
        //angular acceleration = 0;
        align();
        currentTransform.position += linearVelocity * Time.deltaTime;

    }
    void Seek()
    {
        dest = target.transform.position;
        linearAcceleration = dest - currentTransform.position;
        linearAcceleration = clipValue(linearAcceleration, maxLinearAcceleration);

        linearVelocity += linearAcceleration;
        linearVelocity = clipValue(linearVelocity, maxLinearVelocity);
        //angular acceleration = 0;
        align();
        currentTransform.position += linearVelocity * Time.deltaTime;

        /*    linearAcceleration.x = Mathf.Clamp(linearAcceleration.x, 0, maxLinearAcceleration.x);
            linearAcceleration.y = Mathf.Clamp(linearAcceleration.y, maxLinearAcceleration.y * -1, maxLinearAcceleration.y);

            linearVelocity += linearAcceleration;
            linearVelocity.x = Mathf.Clamp(linearVelocity.x, maxLinearVelocity.x * -1, maxLinearVelocity.x);
            linearVelocity.y = Mathf.Clamp(linearVelocity.y, maxLinearVelocity.y * -1, maxLinearVelocity.y);*/

        //clip to max acceleration
        //clip to max speed
        //angular acceleration = 0;
        currentTransform.position += linearVelocity * Time.deltaTime;

    }
    void Wander()
    {
        
        float radius = this.gameObject.GetComponent<SphereCollider>().radius;
        linearAcceleration = (dest - currentTransform.position);
        linearVelocity = linearAcceleration;
        distance = linearVelocity.magnitude;
        currentTransform.position += linearVelocity * Time.deltaTime;

        if (linearVelocity != Vector3.zero) align();
        
        if (distance < 0.5f)
        {
            Vector3 circlePos = calculateTargetPosition(0.5f);
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
        toClip.x = Mathf.Clamp(toClip.x, clipRange.x * -1, clipRange.x);
        toClip.y = Mathf.Clamp(toClip.y, clipRange.y * -1, clipRange.y);
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
        float angle = Mathf.Atan2(linearVelocity.y, linearVelocity.x) * Mathf.Rad2Deg;
        //rotate towards target
        Debug.Log(Vector3.right);
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        currentTransform.rotation = Quaternion.RotateTowards(currentTransform.rotation, q, maxRotationSpeed * Time.deltaTime);
    }
}
   
