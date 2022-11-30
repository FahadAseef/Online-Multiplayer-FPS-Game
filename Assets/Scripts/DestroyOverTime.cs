using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    [SerializeField] float LifeTime = 1.5f;


    private void Start()
    {
        Destroy(gameObject, LifeTime);
    }
}
