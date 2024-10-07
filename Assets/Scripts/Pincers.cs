using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Pincers : MonoBehaviour
{
    private Tick _player;
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _player = Tick.Instance;
        _animator = GetComponent<Animator>();
        StartCoroutine(DestroyAfterDelay(11f));
    }

    // Update is called once per frame
    void Update()
    {
        if (_player.transform.parent != null)
            transform.position = new Vector2(_player.transform.position.x, Mathf.Lerp(transform.position.y, _player.transform.position.y, 0.05f));
        else
        {
            Leave();
        }
    }

    private void Leave()
    {
        _animator.SetTrigger("Leave");
        StartCoroutine(DestroyAfterDelay(3f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
