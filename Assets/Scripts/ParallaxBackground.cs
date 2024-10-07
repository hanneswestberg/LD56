using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    Transform cam; // Camera reference (of its transform)
    Vector3 previousCamPos;

    public float minDistanceX = 0f;
    public float maxDistanceX = 0f;
    public float distanceY = 0f;

    public float smoothingX = 1f; // Smoothing factor of parrallax effect
    public float smoothingY = 1f;

    private float distanceX;

    void Awake () {
        cam = Camera.main.transform;

        distanceX = Random.Range(minDistanceX, maxDistanceX);
    }

    // Update is called once per frame
    void Update()
    {
        if (distanceX != 0f) {
            var parallaxX = (previousCamPos.x - cam.position.x) * distanceX;
            var backgroundTargetPosX = new Vector3(transform.position.x + parallaxX, transform.position.y, transform.position.z);

            // Lerp to fade between positions
            transform.position = Vector3.Lerp(transform.position, backgroundTargetPosX, smoothingX * Time.deltaTime);
        }

        if (distanceY != 0f) {
            var parallaxY = (previousCamPos.y - cam.position.y) * distanceY;
            var backgroundTargetPosY = new Vector3(transform.position.x, transform.position.y + parallaxY, transform.position.z);

            transform.position = Vector3.Lerp(transform.position, backgroundTargetPosY, smoothingY * Time.deltaTime);
        }

        previousCamPos = cam.position;
    }
}
