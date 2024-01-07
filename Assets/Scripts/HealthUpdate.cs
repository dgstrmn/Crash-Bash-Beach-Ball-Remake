using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUpdate : MonoBehaviour
{
    //To be attached to the goal object of the vehicle
    private GameObject vehicle;
    private VehicleHealth vehicleHealth;
    private BallSpawner spawner;

    private void Start()
    {
        spawner = GameObject.Find("/GameManager").GetComponent<BallSpawner>();
        vehicle = GameObject.Find("/" + transform.name.Substring(0, transform.name.Length - 4));
        vehicleHealth = vehicle.GetComponent<VehicleHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Tuple<GameObject, Rigidbody> tupleToRemove = new Tuple<GameObject, Rigidbody>(gameObject, gameObject.GetComponent<Rigidbody>());
            spawner.ballsToSpawn++;
            spawner.spawnedBallList.Remove(tupleToRemove);
            vehicleHealth.UpdateHealth();
            Destroy(other);
        }
    }
}
