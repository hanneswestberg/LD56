using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BloodPanelUI : MonoBehaviour
{
    private Tick _player;

    [SerializeField] private TextMeshProUGUI _bloodText;
    [SerializeField] private Image _bloodImage;
    [SerializeField] private GameObject _evolveText;
    [SerializeField] private TextMeshProUGUI _timesEvolvedText;
    [SerializeField] private Image humanFootArrow;

    private GameObject _closestFoot;

    // Start is called before the first frame update
    void Start()
    {
        _player = Tick.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        _bloodText.text = $"Blood: {_player.currentBlood:F0} / {_player.maxBlood:F0}";
        _bloodImage.fillAmount = _player.currentBlood / _player.maxBlood;
        _evolveText.SetActive(_player.currentBlood >= _player.maxBlood * 0.8f);
        _timesEvolvedText.text = _player.timesEvolved.ToString();


    }
}
