using System;
using UnityEngine;

public class BallBehaviour : MonoBehaviour
{
    BallSpawner spawner;
    public float currentSpeed;

    Rigidbody rb;
    public bool rbRegen = true;


    private void Awake()
    {
        spawner = GameObject.Find("/GameManager").GetComponent<BallSpawner>();
    }


    private void FixedUpdate()
    {
        if (rbRegen)
        {
            rb = GetComponent<Rigidbody>();
            rbRegen = false;
        }
        if(rb != null)
        {
            currentSpeed = rb.velocity.magnitude;
            if (currentSpeed != 0)
            {
                if (rb.velocity.y > 0)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                }
                currentSpeed = rb.velocity.magnitude;
                if (currentSpeed < 4f)
                {
                    rb.velocity *= (4f / rb.velocity.magnitude);
                } 
            }
        }
        

    }
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Despawner"))
        {
            Tuple<GameObject, Rigidbody> tupleToRemove = new Tuple<GameObject, Rigidbody>(gameObject, gameObject.GetComponent<Rigidbody>());
            spawner.ballsToSpawn++;
            spawner.spawnedBallList.Remove(tupleToRemove);
            Destroy(gameObject);
        }
    }
    
}
