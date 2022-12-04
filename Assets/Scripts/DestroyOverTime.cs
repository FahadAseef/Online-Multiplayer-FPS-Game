using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    public float LifeTime = 1.5f;


    private void Start()
    {
        Destroy(gameObject, LifeTime);
    }
}
