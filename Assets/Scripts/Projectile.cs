using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {

        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player) { // overlapping a player object!
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth) {
                playerHealth.TakeDamage(10); // do damage to player
            }
            Destroy(gameObject); // remove projectile from game
        }

    }
}
