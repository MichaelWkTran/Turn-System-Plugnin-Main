using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UnitUI : MonoBehaviour
{
    public Unit m_unit;
    public TMP_Text m_unitName;
    public Slider m_healthSlider;
    public TMP_Text m_healthText;

    void Start()
    {
        m_unitName.text = m_unit.m_unitName;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (m_unit == null) { Destroy(gameObject); return; }

        UpdateSlider(m_healthSlider, m_unit.Health, m_unit.MaxHealth);
        m_healthText.text = m_unit.Health.ToString() + "/" + m_unit.MaxHealth.ToString();
    }

    void UpdateSlider(Slider _slider, float _value, float _maxValue)
    {
        _slider.maxValue = _maxValue;
        _slider.value = _value;
    }
}
