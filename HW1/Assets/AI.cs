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

    public float maxSpeed;
    public float maxAcceleration;
    public float maxRotationSpeed = 10.0f;
    private GameObject circle;
    public struct steering{
        float angular;
        float linear;
    }
    public float targetDistance;
    public float distance;
    public float time;
    public float timeToTarget;
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
        EVADING,
        PURSUING,
        PATHFOLLOWING
    }
    public State state;

    // Use this for initialization
    void Start () {
        
        alive = true;
        radius = this.gameObject.GetComponent<SphereCollider>().radius / 2f;
        if (state == State.WANDERING) setWanderTarget();
        cam = Camera.main;
        pathPoints = GameObject.FindGameObjectsWithTag("wayPoints");
        pathPoints = pathPoints.OrderBy(path => path.name).ToArray();
        pathIndex = 0;// pathPoints.Length -1;
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
                case State.EVADING:
                    Flee();
                    break;
                case State.PATHFOLLOWING:
                    Pathfollow();
                    break;
                case State.PURSUING:
                    Pursue();
                    break;
            }
            yield return null;
        }
    }
	// Update is called once per frame
	void Update () {
        displayState();
        FSM();
	}
    public void Pathfollow()
    {
        if (pathIndex > pathPoints.Length - 1) pathIndex = 0;
        target = pathPoints[pathIndex].transform;
        distance = Vector3.Distance(target.position, transform.position);
        seekTarget(target.position);
        align();

        if (distance < 1f)
        {
            pathIndex++;
        }
    }
    void Flee()
    {
        linearAcceleration = transform.position - target.position;
        linearVelocity += linearAcceleration * Time.deltaTime;
        clipAccelVeloc();
        angularAcceleration = 0;
        align();
        if (checkBounds(transform.position + linearVelocity * Time.deltaTime))
        {
            transform.position += linearVelocity * Time.deltaTime;
        }
        else
        {
            transform.position = new Vector3(transform.position.x / -2, 0);
        }
        distance = Vector3.Distance(transform.position, dest);
    }
    void seekTarget(Vector3 target)
    {
        //Move towards target at full acceleration
        linearAcceleration = target - transform.position;
        linearAcceleration.Normalize();
        linearAcceleration *= maxAcceleration;
        //clip velocity
        linearVelocity += linearAcceleration;
        linearVelocity.Normalize();
        linearVelocity *= maxSpeed;
        //move ai
        transform.position += linearVelocity * Time.deltaTime;
    }
    //if the AI is a hunter or a wolf in WANDERING state
    //create a new wander target
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
            circle.transform.position = transform.position + (transform.right * targetDistance);

        }
    }
    void Pursue()
    {
        //Find the circle that gets drawn upon arrival.
        drawCircle circ = GameObject.Find("arriveRadius").GetComponent<drawCircle>();
        //Check Scene view for this line that is drawn when an AI is in pursuit, it draws a line from
        //current AI position to target AI position
        Debug.DrawLine(target.position + (target.right * 1), transform.position, Color.green, 2f);
        //predict target future movement
        Vector3 predictedTarget = target.position + (target.right * 1);
        //calculate steering towards future position
        seekTarget(predictedTarget);
        
        distance = Vector3.Distance(transform.position, target.position);
        align();

        //This checks if current is within the radius of the arrival circle
        if (distance < circ.xrad / 2)
        {
            Arrive(distance);
        }
        else
        {
            LineRenderer rad = GameObject.Find("arriveRadius").GetComponent<LineRenderer>();
            rad.enabled = false;

        }
    }
    void Arrive(float dist)
    {
        //Find circle object to draw when arriving
        LineRenderer rad = GameObject.Find("arriveRadius").GetComponent<LineRenderer>();
        rad.enabled = true;
        rad.transform.position = target.position;
        
        //calculate target speed
        float targetSpeed = maxSpeed * (dist / 1.25f);
        Vector3 dir = (target.position - transform.position).normalized;
        Vector3 targetVelocity = dir * targetSpeed;
        targetVelocity = dir * targetSpeed;
        //Acceleration tries to get to the target velocity
        linearVelocity = targetVelocity - linearVelocity;

        if (linearVelocity.magnitude > targetSpeed)
        {
            linearVelocity = linearVelocity.normalized;
            linearVelocity *= targetSpeed;
        }
        linearVelocity = linearVelocity / timeToTarget;
        linearAcceleration = linearVelocity;

        transform.position += linearVelocity * Time.deltaTime;

    }
    void Evade() { }
    void Wander()
    {
     //   dest = target.position;
        linearAcceleration = (dest - transform.position);

        linearVelocity = linearAcceleration;
        //clip accel/velocity to max accel/speed
        clipAccelVeloc();

        distance = Vector3.Distance(transform.position, dest);

        transform.position += linearVelocity * Time.deltaTime;

        if (linearVelocity != Vector3.zero) align();
        
        if (distance < 0.5f)
        {
            Vector3 circlePos = calculateTargetPosition(radius);
            circle.transform.position = transform.transform.position + (transform.right * targetDistance);
            Vector2 temp = (Vector2)circle.transform.position + (Vector2)circlePos;

            bool check = checkBounds(temp);
            if (check)
            {
                circle.transform.position = transform.transform.position + (transform.right * targetDistance);
                dest = (Vector2)circle.transform.position + (Vector2)circlePos;
            }
        }
        if (this.gameObject.name.ToLower() == "wolf")
        {
            GameObject hunter = GameObject.Find("Hunter");
            float distToHunter = Vector3.Distance(hunter.transform.position, transform.position);
            if (distToHunter < 2f)
            {
                AI HunterAI = hunter.GetComponent<AI>();
                HunterAI.state = State.PURSUING;
                HunterAI.target = transform;
                state = State.EVADING;
                target = HunterAI.transform;
                Debug.Log(distToHunter);
            }
            Debug.DrawLine(hunter.transform.position, transform.position);
        }
    }
    Vector3 calculateTargetPosition(float radius)
    {
        Vector3 randomPointOnCircle = Random.insideUnitCircle;
        randomPointOnCircle.Normalize();
        randomPointOnCircle *= radius;
        return randomPointOnCircle;
    }
    void clipAccelVeloc()
    {
        linearVelocity = linearVelocity.normalized;
        linearVelocity *= maxSpeed;

        linearAcceleration = linearAcceleration.normalized;
        linearAcceleration *= maxAcceleration;

    }
    void displayState()
    {
        string name = this.gameObject.name.ToLower();
        TextMesh mesh;
        switch (name)
        {
            case ("wolf"):
                mesh = GameObject.Find("WolfState").GetComponent<TextMesh>();
                mesh.text = state.ToString();
                break;
            case ("red"):
                mesh = GameObject.Find("RedState").GetComponent<TextMesh>();
                mesh.text = state.ToString();
                break;
            case ("hunter"):
                mesh = GameObject.Find("HunterState").GetComponent<TextMesh>();
                mesh.text = state.ToString();
                break;
        }

        //  Debug.Log(name);
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
   //     angularVelocity += 1;
        float angle = Mathf.Atan2(linearVelocity.y, linearVelocity.x) * Mathf.Rad2Deg;
        //rotate towards target
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = q;
        Quaternion.RotateTowards(transform.rotation, q, 60f * Time.deltaTime);
    }
}
   
