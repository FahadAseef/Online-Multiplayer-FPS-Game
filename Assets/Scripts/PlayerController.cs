//using System;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    //player rotation
    public Transform ViewPoint;
    public float MouseSensitivity=1f;
    public float VerticalRotationStore;
    public Vector2 MouseInput;

    //player movement
    public float MoveSpeed=5f;
    public Vector3 MoveDirection;
    public Vector3 Movement;
    public CharacterController CharController;
    public float RunSpeed=8f;
    private float ActiveMoveSpeed;
    private Camera Cam;
    public float JumpForce=7.5f;
    public float GravityMod=2.5f;
    public Transform GroundCheckPoint;
    private bool IsGrounded;
    public LayerMask GroundLayer;

    //player shooting
    private RaycastHit Hit;
    public GameObject BulletImpact;
    private GameObject BulleetImpactObject;
    private float ShotCounter;

    //shooting overheat
    public float MaxHeat=10f;
    public float CoolRate = 4f;
    public float OverHeatCoolRate = 5f;
    private float HeatCounter;
    private bool OverHeated;

    //switching weapon
    public Gun[] AllGuns;
    private int SelectedGun;

    //muzzle flash
    public float MuzzleDisplayTime;
    private float MuzzleCounter;

    //player impact
    public GameObject PlayerHitImpact;

    //player health
    public int MaxHealth = 100;
    private int CurrentHealth;

    //animation
    public Animator Anime;
    public GameObject PlayerModel;
    public Transform ModelGunPoint;
    public Transform GunHolder;

    //skins
    public Material[] allSkins;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cam = Camera.main;
        UIcontroller.instance.WeaponTempSlider.maxValue = MaxHeat;
        //SwitchGun();
        photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
        //Transform NewTransform = SpawnManager.instance.GetSpawnPoints();
        //transform.position = NewTransform.position;
        //transform.rotation = NewTransform.rotation;
        CurrentHealth=MaxHealth;
        if (photonView.IsMine)
        {
            PlayerModel.SetActive(false);
            UIcontroller.instance.HealthSlider.maxValue = MaxHealth;
            UIcontroller.instance.HealthSlider.value = CurrentHealth;
        }
        else
        {
            GunHolder.parent = ModelGunPoint;
            GunHolder.localPosition = Vector3.zero;
            GunHolder.localRotation = Quaternion.identity;
        }

        PlayerModel.GetComponent<Renderer>().material = allSkins[photonView.Owner.ActorNumber%allSkins.Length];
    }


    void Update()
    {
        if (photonView.IsMine)
        {
            PlayerRotation();
            PlayerMovement();
            if (AllGuns[SelectedGun].MuzzleFlash.activeInHierarchy)
            {
                MuzzleCounter -= Time.deltaTime;
                if (MuzzleCounter <= 0)
                {
                    AllGuns[SelectedGun].MuzzleFlash.SetActive(false);
                }
            }
            if (!OverHeated)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
                if (Input.GetMouseButton(0) && AllGuns[SelectedGun].IsAutomatic)
                {
                    ShotCounter -= Time.deltaTime;
                    if (ShotCounter <= 0)
                    {
                        Shoot();
                    }
                }
                HeatCounter -= CoolRate * Time.deltaTime;
            }
            else
            {
                HeatCounter -= OverHeatCoolRate * Time.deltaTime;
                if (HeatCounter < 0)
                {
                    OverHeated = false;
                    UIcontroller.instance.OverheatMessagte.gameObject.SetActive(false);
                }
            }
            if (HeatCounter < 0)
            {
                HeatCounter = 0;
            }
            UIcontroller.instance.WeaponTempSlider.value = HeatCounter;
            WeaponScrolling();
            WeaponSwapWithNumPad();
            Anime.SetBool("grounded", IsGrounded);
            Anime.SetFloat("speed", MoveDirection.magnitude);
        }
    }

    private void PlayerRotation()
    {
        MouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * MouseSensitivity;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + MouseInput.x, transform.rotation.eulerAngles.z);
        VerticalRotationStore += MouseInput.y;
        VerticalRotationStore = Mathf.Clamp(VerticalRotationStore, -60, 60);
        ViewPoint.rotation = Quaternion.Euler(-VerticalRotationStore, ViewPoint.rotation.eulerAngles.y, ViewPoint.rotation.eulerAngles.z);
    }

    private void PlayerMovement()
    {
        MoveDirection = new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ActiveMoveSpeed = RunSpeed;
        }
        else
        {
            ActiveMoveSpeed = MoveSpeed;
        }
        float yVel = Movement.y;
        Movement = ((transform.forward * MoveDirection.z)+(transform.right*MoveDirection.x)).normalized*ActiveMoveSpeed;
        Movement.y = yVel;
        if (CharController.isGrounded)
        {
            Movement.y = 0f;
        }
        IsGrounded = Physics.Raycast(GroundCheckPoint.position, Vector3.down,.25f,GroundLayer);
        if (Input.GetButtonDown("Jump")&&IsGrounded)
        {
            Movement.y = JumpForce;
        }
        Movement.y += Physics.gravity.y * Time.deltaTime*GravityMod;
        CharController.Move(Movement *Time.deltaTime);
    }

    private void Shoot()
    {
        Ray ray = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = Cam.transform.position;
        if (Physics.Raycast(ray, out Hit))
        {
            //Debug.Log(Hit.collider.gameObject.name);
            if (Hit.collider.gameObject.tag == "Player")
            {
                Debug.Log(Hit.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(PlayerHitImpact.name, Hit.point, Quaternion.identity);
                Hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, AllGuns[SelectedGun].ShotDamage,PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                BulleetImpactObject = Instantiate(BulletImpact, Hit.point + (Hit.normal * .002f), Quaternion.LookRotation(Hit.normal, Vector3.up));
                Destroy(BulleetImpactObject, 10f);
            }
        }
        ShotCounter = AllGuns[SelectedGun].TimeBetweenShots;

        HeatCounter += AllGuns[SelectedGun].HeatPerShot;
        if (HeatCounter >= MaxHeat)
        {
            HeatCounter = MaxHeat;
            OverHeated = true;
            UIcontroller.instance.OverheatMessagte.gameObject.SetActive(true); 
        }
        AllGuns[SelectedGun].MuzzleFlash.SetActive(true);
        MuzzleCounter = MuzzleDisplayTime;
    }
    private void WeaponScrolling()
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel")>0)
        {
            SelectedGun++;
            if (SelectedGun >= AllGuns.Length)
            {
                SelectedGun = 0;
            }
            //SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            SelectedGun--;
            if (SelectedGun < 0)
            {
                SelectedGun = AllGuns.Length - 1;
            }
            //SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
        }
    }
    private void SwitchGun()
    {
        foreach(Gun gun in AllGuns)
        {
            gun.gameObject.SetActive(false);
        }
        AllGuns[SelectedGun].gameObject.SetActive(true);
        AllGuns[SelectedGun].MuzzleFlash.SetActive(false);
    }
    public void WeaponSwapWithNumPad()
    {
        for(int i = 0; i < AllGuns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SelectedGun = i;
                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, SelectedGun);
            }
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (MatchManager.instance.state == MatchManager.GameState.Playing)
            {
                Cam.transform.position = ViewPoint.position;
                Cam.transform.rotation = ViewPoint.rotation;
            }
            else
            {
                Cam.transform.position = MatchManager.instance.MapCamPoint.position;
                Cam.transform.rotation = MatchManager.instance.MapCamPoint.rotation;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Cursor.lockState== CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0) && !UIcontroller.instance.OptionsScreen.activeInHierarchy) 
            {
                Cursor.lockState= CursorLockMode.Locked;
            }
        }
    }

    [PunRPC]
    public void DealDamage(string damger,int DamageAmount,int actor)
    {
        TakeDamage(damger,DamageAmount,actor);
    }

    public void TakeDamage(string damager,int damageAmount,int actor)
    {
        if (photonView.IsMine)
        {
            //Debug.Log(photonView.Owner.NickName + "has been hit  by" + damager);
            CurrentHealth -= damageAmount;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                PlayerSpawner.Instance.DieFN(damager);
                MatchManager.instance.UpdateStatSend(actor, 0, 1);
            }
            UIcontroller.instance.HealthSlider.value = CurrentHealth;
        }
    }

    [PunRPC]
    public void SetGun(int guntoswitchto)
    {
        if (guntoswitchto < AllGuns.Length)
        {
            SelectedGun = guntoswitchto;
            SwitchGun();
        }
    }
  
}
