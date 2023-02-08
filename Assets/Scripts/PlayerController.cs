using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float mouseSensitivity = 1f;
    [SerializeField] Transform viewPoint;
    [SerializeField] float lookRotationLimit;
    [SerializeField] bool invertLook;

    private float verticalRotStore;
    private Vector2 mouseInput;
    private Vector3 moveDirection;
    private Vector3 movement;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        GetMouseInput();
        RotateHorizontal();
        RotateVertical();

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
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime;
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
}
