using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Windows;
using System.Diagnostics;





public class VehicleController : MonoBehaviour
{
    [SerializeField] private InputReader input;

    [SerializeField] private GameObject gameManager;
    [SerializeField] private GameObject testCube;

    [SerializeField] private float speed;
    [SerializeField] private float shootSpeed, releaseSpeed;

    [SerializeField] private bool isAI = false;
    private bool aiControlChanged;
    [SerializeField] private float aiSpeed = 7f;

    [SerializeField] private VehicleSlideAxis slideAxis;
    private enum VehicleSlideAxis { axisX, axisZ };

    private float moveDirection;
    private bool isMagnet;
    private float cooldownTimer = 3f, inUseTimer = 5f, aiMagnetTimer,
                    cooldownTimeRemaining, inUseTimeRemaining, aiMagnetTimeRemaining;

    private Rigidbody rb;
    private GameObject body;
    private GameObject magnet;
    Material magnetMaterial;
    MagnetState magnetState;
    private enum MagnetState { InUse, Cooldown, Idle };

    float colorIntensity = 3f;

    private List<GameObject> attachedBalls;
    private List<Tuple<GameObject, Rigidbody>> ballsOnTheGround;
    private List<Tuple<GameObject, Rigidbody>> ballsIncoming;
    private Vector3 targetPosition;
    Vector2 leftBorder, rightBorder;
    List<Tuple<GameObject, Rigidbody>> tuplesToBeRemoved;


    // Start is called before the first frame update
    private void Start()
    {
        InitializeVehicle();
    }

    private void Update()
    {
        if (aiControlChanged != isAI)
        {
            HandleEvents();
            aiControlChanged = isAI;
        }
    }

    void FixedUpdate()
    {
        if (isAI)
        {
            AIController();
        }
        else
        {
            Slide();
        }

        Magnet();
    }

    private void AIController()
    {

        bool isXAxis = slideAxis == VehicleSlideAxis.axisX;
        FindTargetBall(isXAxis);
        //GameObject go = Instantiate(testCube, targetPosition, Quaternion.identity);
        //Destroy(go, 0.2f);

        if (body.transform.position != targetPosition) rb.velocity = (targetPosition - body.transform.position).normalized * aiSpeed;


        if (!isMagnet && magnetState == MagnetState.Idle)
        {
            isMagnet = true;
            aiMagnetTimeRemaining = UnityEngine.Random.Range(0f, 5f);
        }
        else if (isMagnet)
        {
            aiMagnetTimeRemaining -= Time.deltaTime;
            if (aiMagnetTimeRemaining < 0f)
            {
                cooldownTimeRemaining = cooldownTimer;
                isMagnet = false;
            }
        }


    }

    private void FindTargetBall(bool xAxis)
    {
        //ballsOnTheGround ??= gameManager.GetComponent<BallSpawner>().spawnedBallList;
        //tuplesToBeRemoved = new List<Tuple<GameObject, Rigidbody>>();

        float minGoalTime = Mathf.Infinity;

        foreach (var ball in ballsOnTheGround)
        {
            Vector2 ballPosition;
            Vector2 ballDirection;

            if (xAxis)
            {
                ballPosition = new Vector2(ball.Item1.transform.position.x, ball.Item1.transform.position.z);
                ballDirection = new Vector2(ball.Item2.velocity.x, ball.Item2.velocity.z).normalized;
            }
            else
            {
                ballPosition = new Vector2(ball.Item1.transform.position.z, ball.Item1.transform.position.x);
                ballDirection = new Vector2(ball.Item2.velocity.z, ball.Item2.velocity.x).normalized;
            }

            float distance = GetRayToLineSegmentIntersection(ballPosition, ballDirection, leftBorder, rightBorder);

            if (distance != -1f)
            {
                Vector2 intersectionPoint = GetRayIntersectionPoint(ballPosition, ballDirection, distance);
                Vector3 trajectedPosition = xAxis
                    ? new Vector3(intersectionPoint.x, body.transform.position.y, intersectionPoint.y)
                    : new Vector3(intersectionPoint.y, body.transform.position.y, intersectionPoint.x);

                float goalTime = distance / ball.Item2.velocity.magnitude;
                if (goalTime < minGoalTime)
                {
                    minGoalTime = goalTime;
                    targetPosition = trajectedPosition;
                }
            }


        }
        if (minGoalTime == Mathf.Infinity)
        {
            if (xAxis)
            {
                targetPosition.x = (leftBorder + rightBorder).x / 2;
            }
            else
            {
                targetPosition.z = (leftBorder + rightBorder).x / 2;
            }

        }
    }




    public static float Cross(Vector2 value1, Vector2 value2)
    {
        return value1.x * value2.y
               - value1.y * value2.x;
    }

    Vector2 GetRayIntersectionPoint(Vector2 origin, Vector2 vector, float distance)
    {
        Vector2 pt = origin + (vector * distance);
        return pt;
    }

    public float GetRayToLineSegmentIntersection(Vector2 rayOrigin, Vector2 rayDirection, Vector2 point1, Vector2 point2)
    {
        var v1 = rayOrigin - point1;
        var v2 = point2 - point1;
        var v3 = new Vector2(-rayDirection.y, rayDirection.x);


        float dot = Vector2.Dot(v2, v3);
        if (Mathf.Abs(dot) < 0.000001)
            return -1.0f;

        float t1 = Cross(v2, v1) / dot;
        float t2 = Vector2.Dot(v1, v3) / dot;

        if (t1 >= 0.0 && (t2 >= 0.0 && t2 <= 1.0))
            return t1;

        return -1.0f;
    }

    private void Slide()
    {
        if (moveDirection != 0f)
        {
            rb.AddForce(transform.right * moveDirection * Time.fixedDeltaTime * speed);
        }
    }

    private void Magnet()
    {
        if (magnetState == MagnetState.Cooldown)
        {
            HandleCooldown();
        }
        else
        {
            if (isMagnet)
            {
                UseMagnetUntilCooldown();
            }
            else if (magnetState == MagnetState.InUse)
            {
                ShootBallsIfAny();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        AttachBallsIfMagnetInUse(collision);
    }

    private void AttachBallsIfMagnetInUse(Collision collision)
    {
        if (magnetState == MagnetState.InUse)
        {
            ContactPoint contactPoint = collision.GetContact(0);
            GameObject self = contactPoint.thisCollider.gameObject;

            GameObject ball = collision.collider.gameObject;

            if (self == body && ball.CompareTag("Ball"))
            {
                Rigidbody rbBall = ball.GetComponent<Rigidbody>();
                ballsOnTheGround.Remove(new Tuple<GameObject, Rigidbody>(ball, rbBall));
                Destroy(rbBall);
                ball.transform.parent = transform;
                attachedBalls.Add(ball);
            }
        }
    }

    private void ShootBallsIfAny()
    {
        if (attachedBalls.Count > 0)
        {
            ReleaseAttachedBallsWithForce(shootSpeed);
        }
        inUseTimeRemaining = inUseTimer;
        SetMagnetState(MagnetState.Cooldown);
    }

    private void HandleCooldown()
    {
        cooldownTimeRemaining -= Time.deltaTime;
        if (cooldownTimeRemaining < 0f)
        {
            cooldownTimeRemaining = cooldownTimer;
            SetMagnetState(MagnetState.Idle);
        }
    }

    private void UseMagnetUntilCooldown()
    {
        if (magnetState != MagnetState.InUse)
        {
            SetMagnetState(MagnetState.InUse);
        }
        else
        {
            inUseTimeRemaining -= Time.deltaTime;
            if (inUseTimeRemaining < 0f)
            {
                if (attachedBalls.Count > 0)
                {
                    ReleaseAttachedBallsWithForce(releaseSpeed);
                }
                inUseTimeRemaining = inUseTimer;
                SetMagnetState(MagnetState.Cooldown);
            }
        }
    }

    private void ReleaseAttachedBallsWithForce(float force)
    {
        foreach (GameObject ball in attachedBalls)
        {
            ball.transform.parent = null;
            Rigidbody rbBall = ball.AddComponent<Rigidbody>();
            ballsOnTheGround.Add(new Tuple<GameObject, Rigidbody>(ball, rbBall));
            rbBall.mass = 0.1f;
            rbBall.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            ball.GetComponent<BallBehaviour>().rbRegen = true;

            Vector3 shootDir = (ball.transform.position - body.transform.position).normalized;
            rbBall.AddForce(shootDir * force);
        }
        attachedBalls.Clear();
    }

    private void SetMagnetState(MagnetState state)
    {
        UnityEngine.Color color = new();

        switch (state)
        {
            case MagnetState.Idle:
                color = UnityEngine.Color.green;
                break;
            case MagnetState.InUse:
                color = UnityEngine.Color.yellow;
                break;
            case MagnetState.Cooldown:
                color = UnityEngine.Color.red;
                break;
            default:
                break;
        }

        magnetState = state;
        magnetMaterial.SetColor("_EmissionColor", color * colorIntensity);
    }

    private void InitializeVehicle()
    {
        aiControlChanged = true;
        attachedBalls = new List<GameObject>();
        ballsOnTheGround = gameManager.GetComponent<BallSpawner>().spawnedBallList;
        ballsIncoming = new List<Tuple<GameObject, Rigidbody>>();

        rb = GetComponent<Rigidbody>();
        rb.constraints |= (slideAxis == VehicleSlideAxis.axisX) ? RigidbodyConstraints.FreezePositionZ : RigidbodyConstraints.FreezePositionX;

        body = transform.Find("Body").gameObject;
        magnet = transform.Find("Magnet").gameObject;

        cooldownTimeRemaining = cooldownTimer;
        inUseTimeRemaining = inUseTimer;
        aiMagnetTimeRemaining = aiMagnetTimer;

        magnetMaterial = magnet.GetComponent<Renderer>().material;
        SetMagnetState(MagnetState.Idle);

        RaycastHit hit;
        if (Physics.Raycast(body.transform.position, transform.right, out hit))
        {
            Vector3 pillarPoint = hit.collider.transform.position;
            rightBorder = slideAxis == VehicleSlideAxis.axisZ ? new Vector2(pillarPoint.z, pillarPoint.x) : new Vector2(pillarPoint.x, pillarPoint.z);
        }
        if (Physics.Raycast(body.transform.position, -transform.right, out hit))
        {
            Vector3 pillarPoint = hit.collider.transform.position;
            leftBorder = slideAxis == VehicleSlideAxis.axisZ ? new Vector2(pillarPoint.z, pillarPoint.x) : new Vector2(pillarPoint.x, pillarPoint.z);
        }
    }

    private void HandleEvents()
    {
        if (!isAI)
        {
            input.SlideEvent += HandleSlide;
            input.MagnetEvent += HandleMagnet;
            input.MagnetCancelledEvent += HandleCancelledMagnet;
        }
        else
        {
            input.SlideEvent -= HandleSlide;
            input.MagnetEvent -= HandleMagnet;
            input.MagnetCancelledEvent -= HandleCancelledMagnet;
        }
    }

    private void HandleSlide(float dir)
    {
        moveDirection = dir;
    }

    private void HandleMagnet()
    {
        isMagnet = true;
    }

    private void HandleCancelledMagnet()
    {
        isMagnet = false;
    }

}
