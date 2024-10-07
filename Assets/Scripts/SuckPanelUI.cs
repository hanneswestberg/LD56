using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuckPanelUI : MonoBehaviour
{
    private Tick _player;

    [SerializeField] private Button _suckButton;
    [SerializeField] private Button _evolveButton;
    [SerializeField] private Image _suckImage;

    private Color _startColor;

    // Start is called before the first frame update
    void Start()
    {
        _player = Tick.Instance;
        _startColor = _suckImage.color;
    }

    // Update is called once per frame
    void Update()
    {
        _suckButton.interactable = _player.canSuckBlood;
        _suckButton.image.color = _player.canSuckBlood ? Color.white : Color.gray;

        _evolveButton.interactable = _player.canEvolve;
        _evolveButton.image.color = _player.canEvolve ? Color.white : Color.gray;

        _suckImage.color = _player.isSuckingBlood
            ? Color.Lerp(_suckImage.color, new Color(_startColor.r, _startColor.g, _startColor.b, 1f), 0.01f)
            : _startColor;
    }

    public void Evolve()
    {
        _player.Evolve();
    }

    public void SuckBlood()
    {
        _player.SuckBlood();
    }

    public void JumpButton()
    {
        _player.Jump();
    }
}
