using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
This class can be used as base class for interactable objects
Your class should inherit from this class (instead of Mono/NetworkBehaviour, 
change this class to NetworkBehaviour if required) 
and implement StartInteraction()

To highlight interactable objects this class assumes there is an material 
named "Interactable" in your Resources folder which has Emission enabled.
OverrideInteractableMat can be set if you want to use a spcific material
for an object, but it still needs emisson for highlighting.
*/
public abstract class Interactable : MonoBehaviour {

    private Renderer rend;
    protected GameObject player;
    
	private bool inRange;
    public float range = 3.0f;

    private Color active = Color.blue;
    private Color inactive = Color.red;

    public Material overrideInteractableMat = null;
    
    // Static setup for interactable objects
    void Awake() {
        transform.tag = "Interactable";

        rend = GetComponent<Renderer>();
        if (overrideInteractableMat == null) {
            rend.material = Resources.Load<Material>("Interactable");
        } else {
            rend.material = overrideInteractableMat;
        }

        player = GameObject.FindGameObjectWithTag("Player");

        CheckRange(); // Set initial color

        if (GetComponent<Collider>() == null) {
            gameObject.AddComponent<BoxCollider>();
        }
    }

    // used FixedUpdate to keep Update() available for inheriting classes
    void FixedUpdate () {
        CheckRange();
	}

    // Check if player is in range of interactable and change color accordingly
    private void CheckRange() {
		inRange = Vector3.Distance(player.transform.position, transform.position) < range;
		
        if (inRange) {
            rend.material.SetColor("_EmissionColor", active);
        } else {
            rend.material.SetColor("_EmissionColor", inactive);
        }
    }

    // Checks if player is close enough;
    public bool InRange() {
        return inRange;
    }

    public void Interaction() {
        if (InRange()) {
            StartInteraction();
        }
    }

    // Implement this method to handle interaction
    public abstract void StartInteraction();
}
