using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Camera cam;
    private CharacterController pawn;
    public float walkSpeed = 5;

    public Transform leg1;
    public Transform leg2;

    public float gravityMultiplier = 10;
    public float jumpImpulse = 5;

    private Vector3 inputDirection = new Vector3();

    private float timeLeftGrounded = 0;

    public bool isGrounded {
        get { // return true if pawn is on ground OR "coyote-time" isn't zero
            return pawn.isGrounded || timeLeftGrounded > 0;
        }
    }

    /// <summary>
    /// How fast the player is currently moving vertically (y-axis), in meters/second.
    /// </summary>
    private float verticalVelocity = 0;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        pawn = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update() {

        // countdown:
        if (timeLeftGrounded > 0) timeLeftGrounded -= Time.deltaTime;

        MovePlayer();
        if (isGrounded) WiggleLegs(); // idle + walk
        else AirLegs(); // jump (or falling)
    }

    private void WiggleLegs() {

        float degrees = 45;
        float speed = 10;


        Vector3 inputDirLocal = transform.InverseTransformDirection(inputDirection);
        Vector3 axis = Vector3.Cross(inputDirLocal, Vector3.up);

        // check the alignment of inputDirLocal against forward vector
        float alignment = Vector3.Dot(inputDirLocal, Vector3.forward);

        // Vector3.forward == new Vector3(0,0,1)
        
        //if (alignment < 0) alignment *= -1; // flips negative numbers

        alignment = Mathf.Abs(alignment); // flips negative numbers

        degrees *= AnimMath.Lerp(0.25f, 1, alignment); // decrease `degrees` when strafing
        
        float wave = Mathf.Sin(Time.time * speed) * degrees;

        leg1.localRotation = AnimMath.Slide(leg1.localRotation, Quaternion.AngleAxis(wave, axis), .001f);
        leg2.localRotation = AnimMath.Slide(leg2.localRotation, Quaternion.AngleAxis(-wave, axis), .001f);

    }

    private void AirLegs() {
        leg1.localRotation = AnimMath.Slide(leg1.localRotation, Quaternion.Euler(30, 0, 0), .001f);
        leg2.localRotation = AnimMath.Slide(leg2.localRotation, Quaternion.Euler(-30, 0, 0), .001f);
    }

    private void MovePlayer() {
        float h = Input.GetAxis("Horizontal"); // strafing?
        float v = Input.GetAxis("Vertical"); // forward / backward

        bool isJumpHeld = Input.GetButton("Jump");
        bool onJumpPress = Input.GetButtonDown("Jump");

        bool isTryingToMove = (h != 0 || v != 0);
        if (isTryingToMove) {
            // turn to face the correct direction...
            float camYaw = cam.transform.eulerAngles.y;
            transform.rotation = AnimMath.Slide(transform.rotation, Quaternion.Euler(0, camYaw, 0), .02f);
        }

        inputDirection = transform.forward * v + transform.right * h;
        if (inputDirection.sqrMagnitude > 1) inputDirection.Normalize();

        // apply gravity:
        verticalVelocity += gravityMultiplier * Time.deltaTime;

        // adds lateral movement to vertical movement:
        Vector3 moveDelta = inputDirection * walkSpeed + verticalVelocity * Vector3.down;

        // move pawn:
        CollisionFlags flags = pawn.Move(moveDelta * Time.deltaTime);
        if (pawn.isGrounded) {
            verticalVelocity = 0; // on ground, zero-out vertical-velocity
            timeLeftGrounded = .2f;
        }

        if(isGrounded) {
            
            if (isJumpHeld) {
                verticalVelocity = -jumpImpulse;
                timeLeftGrounded = 0; // not on ground (for animation's sake)
            }
        }
    }
}
