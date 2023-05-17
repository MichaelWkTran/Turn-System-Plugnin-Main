using UnityEngine;
using UnityEngine.UI;
using System;

public class UnitUI : MonoBehaviour
{
    public UnitStats m_unitStats;
    public Slider m_healthSlider;

    public void Init(UnitStats _unitStats)
    {
        if (_unitStats == null || m_unitStats == null) return;
        m_unitStats = _unitStats;
    }

    void UpdateSlider(Slider _slider, float _value, float _maxValue)
    {
        _slider.maxValue = _maxValue;
        _slider.value = _value;
    }

    public void UpdateUI()
    {
        if (m_unitStats == null) { Destroy(gameObject); return; }

        UpdateSlider(m_healthSlider, m_unitStats.Health, m_unitStats.MaxHealth);
    }

    static public UnitUI FindUIWithUnitStats(UnitStats _unitStats)
    {
        UnitUI[] unitUIInWorld = FindObjectsOfType<UnitUI>(true);
        return Array.Find(unitUIInWorld, i => i.m_unitStats == _unitStats);
    }
}
