using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUnstuck : MonoBehaviour
{
    [SerializeField] private GameObject _player;

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            _player.SetActive(false);
            StartCoroutine(ResetPlayer());
        }
    }

    private IEnumerator ResetPlayer()
    {
        yield return new WaitForEndOfFrame();

        _player.SetActive(true);
    }
}
