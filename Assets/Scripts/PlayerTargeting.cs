using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargeting : MonoBehaviour {

    public Transform target;
    public bool wantsToTarget = false;
    public float visionDistance = 10;
    public float visionAngle = 45;

    private List<TargetableThing> potentialTargets = new List<TargetableThing>();

    float cooldownScan = 0;
    float cooldownPick = 0;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        wantsToTarget = Input.GetButton("Fire2");

        if (!wantsToTarget) target = null;

        cooldownScan -= Time.deltaTime; // counting down...
        if (cooldownScan <= 0 || (target == null && wantsToTarget) ) ScanForTargets(); // do this when countdown finished

        cooldownPick -= Time.deltaTime; // counting down...
        if (cooldownPick <= 0) PickATarget(); // do this when countdown finished

        // if we have target and we can't see it, set target to null
        if (target && CanSeeThing(target) == false) target = null;
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
