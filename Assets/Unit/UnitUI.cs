using UnityEngine;
using UnityEngine.UI;
using System;

public class UnitUI : MonoBehaviour
{
    public Unit m_Unit;
    public Slider m_healthSlider;
    public Text m_healthText;

    public void Init(Unit _Unit)
    {
        if (_Unit == null || m_Unit == null) return;
        m_Unit = _Unit;
    }

    public void UpdateUI()
    {
        if (m_Unit == null) { Destroy(gameObject); return; }

        UpdateSlider(m_healthSlider, m_Unit.Health, m_Unit.MaxHealth);
        m_healthText.text = m_Unit.Health.ToString() + "/" + m_Unit.MaxHealth.ToString();
    }

    void UpdateSlider(Slider _slider, float _value, float _maxValue)
    {
        _slider.maxValue = _maxValue;
        _slider.value = _value;
    }
}
