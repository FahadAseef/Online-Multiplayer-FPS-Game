using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    //player rotation
    [SerializeField] Transform ViewPoint;
    [SerializeField] float MouseSensitivity=1f;
    private float VerticalRotationStore;
    private Vector2 MouseInput;

    //player movement
    [SerializeField] float MoveSpeed=5f;
    private Vector3 MoveDirection;
    private Vector3 Movement;
    [SerializeField] CharacterController CharController;
    [SerializeField] float RunSpeed=8f;
    private float ActiveMoveSpeed;
    private Camera Cam;
    [SerializeField] float JumpForce=7.5f;
    [SerializeField] float GravityMod=2.5f;
    [SerializeField] Transform GroundCheckPoint;
    private bool IsGrounded;
    [SerializeField] LayerMask GroundLayer;

    //player shooting
    private RaycastHit Hit;
    [SerializeField] GameObject BulletImpact;
    private GameObject BulleetImpactObject;
    private float ShotCounter;

    //shooting overheat
    [SerializeField] float MaxHeat=10f;
    [SerializeField] float CoolRate = 4f;
    [SerializeField] float OverHeatCoolRate = 5f;
    private float HeatCounter;
    private bool OverHeated;

    //switching weapon
    public Gun[] AllGuns;
    private int SelectedGun;

    //muzzle flash
    [SerializeField] float MuzzleDisplayTime;
    private float MuzzleCounter;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cam = Camera.main;
        UIcontroller.instance.WeaponTempSlider.maxValue = MaxHeat;
        SwitchGun();
        //Transform NewTransform = SpawnManager.instance.GetSpawnPoints();
        //transform.position = NewTransform.position;
        //transform.rotation = NewTransform.rotation;
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
            Debug.Log(Hit.collider.gameObject.name);
            BulleetImpactObject = Instantiate(BulletImpact, Hit.point + (Hit.normal*.002f), Quaternion.LookRotation(Hit.normal, Vector3.up));
            Destroy(BulleetImpactObject,10f);
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
            SwitchGun();
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            SelectedGun--;
            if (SelectedGun < 0)
            {
                SelectedGun = AllGuns.Length - 1;
            }
            SwitchGun();
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
                SwitchGun();
            }
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            Cam.transform.position = ViewPoint.position;
            Cam.transform.rotation = ViewPoint.rotation;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Cursor.lockState== CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState= CursorLockMode.Locked;
            }
        }
    }

}
