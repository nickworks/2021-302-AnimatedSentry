using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargeting : MonoBehaviour {

    public Transform target;
    public bool wantsToTarget = false;
    public bool wantsToAttack = false;
    public float visionDistance = 10;
    public float visionAngle = 45;

    private List<TargetableThing> potentialTargets = new List<TargetableThing>();

    float cooldownScan = 0;
    float cooldownPick = 0;
    float cooldownShoot = 0;

    public float roundsPerSecond = 10;

    // references to the player's arm "bones":
    public Transform armL;
    public Transform armR;

    private Vector3 startPosArmL;
    private Vector3 startPosArmR;

    /// <summary>
    /// A reference to the particle system prefab to spawn
    /// when the gun shoots.
    /// </summary>
    public ParticleSystem prefabMuzzleFlash;
    public Transform handR;
    public Transform handL;

    CameraOrbit camOrbit;


    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;

        startPosArmL = armL.localPosition;
        startPosArmR = armR.localPosition;

        camOrbit = Camera.main.GetComponentInParent<CameraOrbit>();
    }

    void Update() {
        wantsToTarget = Input.GetButton("Fire2");
        wantsToAttack = Input.GetButton("Fire1");

        if (!wantsToTarget) target = null;

        cooldownScan -= Time.deltaTime; // counting down...
        if (cooldownScan <= 0 || (target == null && wantsToTarget) ) ScanForTargets(); // do this when countdown finished

        cooldownPick -= Time.deltaTime; // counting down...
        if (cooldownPick <= 0) PickATarget(); // do this when countdown finished

        if (cooldownShoot > 0) cooldownShoot -= Time.deltaTime;

        // if we have target and we can't see it, set target to null
        if (target && CanSeeThing(target) == false) target = null;

        SlideArmsHome();

        DoAttack();

    }
    private void SlideArmsHome() {
        armL.localPosition = AnimMath.Slide(armL.localPosition, startPosArmL, .01f);
        armR.localPosition = AnimMath.Slide(armR.localPosition, startPosArmR, .01f);
    }

    private void DoAttack() {

        if (cooldownShoot > 0) return; // too soon!
        if (!wantsToTarget) return; // player not targeting
        if (!wantsToAttack) return; // player not shooting
        if (target == null) return; // no target
        if (!CanSeeThing(target)) return; // target can't be seen


        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth) {
            targetHealth.TakeDamage(20);
        }

        cooldownShoot = 1 / roundsPerSecond;

        // attack!

        camOrbit.Shake(.5f);

        if(handL) Instantiate(prefabMuzzleFlash, handL.position, handL.rotation);
        if(handR) Instantiate(prefabMuzzleFlash, handR.position, handR.rotation);

        // trigger arm animation

        // rotates the arms up:
        armL.localEulerAngles += new Vector3(-20, 0, 0);
        armR.localEulerAngles += new Vector3(-20, 0, 0);

        // moves the arms backwards:
        armL.position += -armL.forward * .1f;
        armR.position += -armR.forward * .1f;

    }


    private bool CanSeeThing(Transform thing) {

        if (!thing) return false; // uh... error

        Vector3 vToThing = thing.position - transform.position;

        // check distance:
        if (vToThing.sqrMagnitude > visionDistance * visionDistance) return false; // too far away to see...

        // check direction:
        if (Vector3.Angle(transform.forward, vToThing) > visionAngle) return false; // out of vision "cone"

        // TODO: check occlusion

        return true;
    }


    private void ScanForTargets() {

        // do the next scan in 1 seconds:
        cooldownScan = 1;

        // empty the list:
        potentialTargets.Clear();
        
        // refill the list:

        TargetableThing[] things = GameObject.FindObjectsOfType<TargetableThing>();
        foreach(TargetableThing thing in things) {

            // if we can see it
            // add target to potentialTargets

            if (CanSeeThing(thing.transform)) { 
                potentialTargets.Add(thing);
            }
            
        }

    }

    void PickATarget() {

        cooldownPick = .25f;

        //if (target) return; // we already have a target...
        target = null;

        float closestDistanceSoFar = 0;

        // find closest targetable-thing and sets it as our target:
        foreach(TargetableThing pt in potentialTargets) {
            
            float dd = (pt.transform.position - transform.position).sqrMagnitude;

            if(dd < closestDistanceSoFar || target == null) {
                target = pt.transform;
                closestDistanceSoFar = dd;
            }

        }

    }
}
