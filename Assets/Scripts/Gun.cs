using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool IsAutomatic;
    public float TimeBetweenShots=1f;
    public float HeatPerShot=1f;
    public GameObject MuzzleFlash;
    public int ShotDamage;
    public float adsZoom;
    public AudioSource shotSound;
}
