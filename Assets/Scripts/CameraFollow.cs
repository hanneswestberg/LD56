using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private float desiredSize = 5.0f;

    private Camera _camera;
    private Tick _tick;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
        _tick = Tick.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        // try to focus the camera on the player, where Y offset is -3
        var targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, -10);
        transform.position = targetPosition;

        desiredSize = _tick.isSuckingBlood || _tick.isEvolving ? 3.0f : 5.0f;

        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, desiredSize, 0.08f);
    }
}
