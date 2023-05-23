using UnityEngine;
using UnityEngine.UI;
using System;

public class UnitUI : MonoBehaviour
{
    public Unit m_Unit;
    public Slider m_healthSlider;

    public void Init(Unit _Unit)
    {
        if (_Unit == null || m_Unit == null) return;
        m_Unit = _Unit;
    }

    void UpdateSlider(Slider _slider, float _value, float _maxValue)
    {
        _slider.maxValue = _maxValue;
        _slider.value = _value;
    }

    public void UpdateUI()
    {
        if (m_Unit == null) { Destroy(gameObject); return; }

        UpdateSlider(m_healthSlider, m_Unit.Health, m_Unit.MaxHealth);
    }

    static public UnitUI FindUIWithUnit(Unit _Unit)
    {
        UnitUI[] unitUIInWorld = FindObjectsOfType<UnitUI>(true);
        return Array.Find(unitUIInWorld, i => i.m_Unit == _Unit);
    }
}
