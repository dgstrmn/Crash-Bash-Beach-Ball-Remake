using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnPoints;
    [SerializeField] private GameObject ballPrefab;

    [SerializeField] private float speed = 1f;
    [SerializeField] private float maxOffset = 0.1f;
    [SerializeField] private float delay = 0.5f;
    [SerializeField] private int maxBallCount = 4;
    private float timeRemaining;
    public int ballsToSpawn;
    [SerializeField] public List<Tuple<GameObject, Rigidbody>> spawnedBallList;

    bool readyToSpawn;

    // Start is called before the first frame update
    private void Awake()
    {
        spawnedBallList = new List<Tuple<GameObject, Rigidbody>>();
    }

    void Start()
    {
        ballsToSpawn = maxBallCount;
        timeRemaining = delay;
        readyToSpawn = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(readyToSpawn)
        {
            int selection = UnityEngine.Random.Range(0, spawnPoints.Length);
            GameObject spawnedBall = Instantiate(ballPrefab, spawnPoints[selection].transform.position, Quaternion.identity);
            Rigidbody rbBall = spawnedBall.GetComponent<Rigidbody>();
            spawnedBallList.Add(new Tuple<GameObject, Rigidbody>(spawnedBall, rbBall));
            ballsToSpawn--;
            float offset = UnityEngine.Random.Range(-maxOffset, maxOffset);
            Vector3 shootDir = (spawnPoints[selection].transform.forward + (spawnPoints[selection].transform.right * offset)).normalized;
            rbBall.AddForce(shootDir * speed);
            readyToSpawn = false;
        }
        else
        {
            if(ballsToSpawn != 0)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining < 0f)
                {
                    timeRemaining = delay;
                    readyToSpawn = true;
                }
            }
            else
            {
                readyToSpawn = false;
            }
        }
    }
}
