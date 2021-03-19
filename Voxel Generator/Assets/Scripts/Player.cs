using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public bool isGrounded;
    public bool isSprinting;

    public float playerWidth = 0.15f;

    private float horizontal = 0, vertical = 0;
    private float mouseHoriz = 0, mouseVert = 0;
    private Vector3 velocity;
    private float verticalMomentum;
    private bool jumpRequest;

    public bool Front
    {
        get
        {
            if(world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z + playerWidth)
                || world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool Back
    {
        get
        {
            if (world.CheckForVoxel(transform.position.x, transform.position.y, transform.position.z - playerWidth)
                || world.CheckForVoxel(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool Left
    {
        get
        {
            if (world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y, transform.position.z)
                || world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool Right
    {
        get
        {
            if (world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y, transform.position.z)
                || world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private void Awake()
    {
        cam = transform.GetChild(0);
        world = GameObject.Find("World").GetComponent<World>();
    }

    private void FixedUpdate()
    {
        CalculateVelocity();

        if(jumpRequest)
        {
            Jump();
        }

        transform.Rotate(Vector3.up * mouseHoriz);
        cam.Rotate(Vector3.right * -mouseVert);
        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        GetPlayerInputs();
    }

    private void CalculateVelocity()
    {
        if(verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }

        if(isSprinting)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * sprintSpeed;
        }
        else
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * walkSpeed;
        }

        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if((velocity.z > 0 && Front) || velocity.z < 0 && Back)
        {
            velocity.z = 0;
        }

        if ((velocity.x > 0 && Right) || velocity.x < 0 && Left)
        {
            velocity.x = 0;
        }

        if(velocity.y < 0)
        {
            velocity.y = checkDownSpeed(velocity.y);
        }
        else if(velocity.y > 0)
        {
            velocity.y = checkUpSpeed(velocity.y);
        }
    }

    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHoriz = Input.GetAxis("Mouse X");
        mouseVert = Input.GetAxis("Mouse Y");

        if(Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }

        if(Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
    }

    private float checkDownSpeed(float downSpeed)
    {
        if(world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) 
            || world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)
            || world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)
            || world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    private float checkUpSpeed(float upSpeed)
    {
        if (world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)
            || world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)
            || world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)
            || world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }
}
