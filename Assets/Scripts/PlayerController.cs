using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
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
    }

    private void Update()
    {
        GetMouseInput();
        RotateHorizontal();
        RotateVertical();

        GetMovementInput();
        Move();

        ToggleCursor();

        Shoot();

    }

    private void LateUpdate()
    {
        SetCamera();
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

    private void SetCamera()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            ray.origin = cam.transform.position;
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject impact = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(impact, impactLifetime);
            }
        }
    }
}
