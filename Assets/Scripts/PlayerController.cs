using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Movement Configs")]
    [SerializeField] float moveSpeed;
    [SerializeField] float runSpeed;

    [Header("Look Rotation Configs")]
    [SerializeField] Transform viewPoint;
    [SerializeField] float rotationSpeed;
    [SerializeField] float lookRotationLimit;
    [SerializeField] bool invertLook;
    [SerializeField] float mouseSensitivity = 1f;

    [Header("Jump Configs")]
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float gravityModification = 2.5f;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] LayerMask groundLayers;

    [Header("Shooting Configs")]
    [SerializeField] GameObject bulletImpact;
    [SerializeField] float impactLifetime = 10f;
    [SerializeField] float maxHeat = 10f;
    [SerializeField] float coolRate = 4f;
    [SerializeField] float overheatCoolRate = 5f;
    [SerializeField] float muzzleDisplayTime;
    private float muzzleCounter;

    [Header("Available Guns")]
    [SerializeField] Gun[] allGuns;
    private int selectedGun;

    [Header("VFX")]
    [SerializeField] GameObject playerHitImpact;


    private float heatCounter;
    private bool overHeated;

    private float shotCounter;
    private float verticalRotStore;
    private Vector2 mouseInput;

    private Vector3 moveDirection;
    private Vector3 movement;
    private float activeMoveSpeed;
    private bool isGrounded;

    private CharacterController characterController;
    private Camera cam;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;

        UIController.instance.weaponTempSlider.maxValue = maxHeat;
        SwitchGun();
    }

    private void SpawnPlayer()
    {
        Transform newSpawnPoint = SpawnManager.instance.GetSpawnPoint();
        transform.position = newSpawnPoint.position;
        transform.rotation = newSpawnPoint.rotation;
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        HandleRotation();
        HandleMovement();
        ToggleCursor();

        if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
            if (muzzleCounter <= 0)
            {
                allGuns[selectedGun].muzzleFlash.SetActive(false);

            }
        }


        if (!overHeated)
        {
            Shoot();
            CalculateCurrentHeat();
        }
        else
        {
            HandleOverheating();
        }

        ResetIfHeatIsNegative();
        SelectGun();

    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            SetCamera();
        }
    }

    #region Movement

    private void HandleRotation()
    {
        GetMouseInput();
        RotateHorizontal();
        RotateVertical();
    }

    private void HandleMovement()
    {
        GetMovementInput();
        Move();
    }


    private void GetMouseInput()
    {
        float xRot = Input.GetAxisRaw("Mouse X");
        float yRot = Input.GetAxisRaw("Mouse Y");
        
        mouseInput =  new Vector2(xRot, yRot) * mouseSensitivity;
    }

    private void GetMovementInput()
    {
        float xMove = Input.GetAxisRaw("Horizontal");
        float zMove = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(xMove, 0f, zMove);
    }
    private void Move()
    {
        activeMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        float yVelocity = movement.y;
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * activeMoveSpeed;
        movement.y = yVelocity;

        if (characterController.isGrounded)
        {
            movement.y = 0f;
        }

        Jump();

        characterController.Move(movement * Time.deltaTime);

       
    }

    private void Jump()
    {
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayers);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityModification;
    }

    private void RotateHorizontal()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
    }

    private void RotateVertical()
    {
        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -lookRotationLimit, lookRotationLimit);

        if (invertLook)
        {
            viewPoint.rotation = Quaternion.Euler(verticalRotStore, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        else
        {
            viewPoint.rotation = Quaternion.Euler(-verticalRotStore, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }
    private void ToggleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
    #endregion

    #region ShootingAndOverheating
    private void SetCamera()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    private void Shoot()
    {
        if (Input.GetMouseButton(0))
        {
            shotCounter -= Time.deltaTime;
            if (shotCounter <= 0)
            {
                allGuns[selectedGun].muzzleFlash.SetActive(true);
                muzzleCounter = muzzleDisplayTime;

                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                ray.origin = cam.transform.position;
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    HandleShotImpact(hit);
                }

                shotCounter = allGuns[selectedGun].timeBetweenShots;
                heatCounter += allGuns[selectedGun].heatPerShot;

                if (heatCounter >= maxHeat)
                {
                    SetToMaxHeat();
                }
            }
        }
    }

    [PunRPC]
    public void DealDamage(string damager)
    {
        TakeDamage(damager);
    }

    public void TakeDamage(string damager)
    {
        if (photonView.IsMine)
        {
            PlayerSpawner.Instance.Die(damager);
        }

    }

    private void HandleShotImpact(RaycastHit hit)
    {
        if (hit.collider.gameObject.CompareTag("Player"))
        {
            PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);
            hit.collider.gameObject.GetPhotonView().RPC
                (
                "DealDamage",
                RpcTarget.All,
                PhotonNetwork.NickName
                );
        }
        else
        {
            GameObject impact = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
            Destroy(impact, impactLifetime);
        }
    }
    #region HeatLogic
    private void CalculateCurrentHeat()
    {
        heatCounter -= coolRate * Time.deltaTime;
    }

    private void SetToMaxHeat()
    {
        heatCounter = maxHeat;
        overHeated = true;
        UIController.instance.overheatedMessage.gameObject.SetActive(true);
    }

    private void HandleOverheating()
    {
        heatCounter -= overheatCoolRate * Time.deltaTime;
        if (heatCounter <= 0)
        {
            overHeated = false;
            UIController.instance.overheatedMessage.gameObject.SetActive(false);
        }
    }

    private void ResetIfHeatIsNegative()
    {
        if (heatCounter < 0)
        {
            heatCounter = 0f;
        }

        AlignTempSlider();
    }

    private void AlignTempSlider()
    {
        UIController.instance.weaponTempSlider.value = heatCounter;

    }
    #endregion
    #endregion

    #region GunSelection
    private void SelectGun()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            ChooseNextGun();
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            ChoosePreviousGun();
        }

        SwitchGun();
    }

   private void ChooseNextGun()
    {
        selectedGun++;

        if (selectedGun >= allGuns.Length)
        {
            selectedGun = 0;
        }
    }

    private void ChoosePreviousGun()
    {
        selectedGun--;
        if (selectedGun < 0)
        {
            selectedGun = allGuns.Length - 1;
        }
    }

    private void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }

        allGuns[selectedGun].gameObject.SetActive(true);
    }
    #endregion
}
