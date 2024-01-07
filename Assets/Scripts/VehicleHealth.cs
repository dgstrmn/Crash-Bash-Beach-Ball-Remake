using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleHealth : MonoBehaviour
{
    
    [SerializeField] private int health;
    [SerializeField] public int maxHealth = 20;
    private GameObject healthProgress;
    private Vector3 defaultScale;


    // Start is called before the first frame update
    void Start()
    {
        healthProgress = transform.Find("/" + transform.name + "/HealthBar" + "/Health").gameObject;
        health = maxHealth;
        defaultScale = healthProgress.transform.localScale;
        healthProgress.transform.localScale = new Vector3(GetHealthPercentage(), defaultScale.y, defaultScale.z);
        healthProgress.transform.parent.LookAt(Camera.main.transform);
    }


    private float GetHealthPercentage()
    {
        return (float) health / maxHealth;
    }

    public void UpdateHealth()
    {
        health--;
        if(health > 0)
        {
            healthProgress.transform.localScale = new Vector3(GetHealthPercentage(), defaultScale.y, defaultScale.z);
        }
        else
        {
            Destroy(gameObject.GetComponent<Rigidbody>());
            GameObject goal = GameObject.Find("/" + transform.name + "Goal");
            goal.transform.position += transform.forward * 0.7f;
            goal.GetComponent<Collider>().isTrigger = false;
            Destroy(gameObject);
        }
        
    }
    
}
