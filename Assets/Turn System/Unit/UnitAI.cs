﻿using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitAI : MonoBehaviour
{
    Unit m_unit;

    void Awake()
    {
        m_unit = GetComponent<Unit>();
    }

    void OnEnable()
    {
        m_unit.m_onUpdate.AddListener(OnUnitUpdate);
    }

    void OnDisable()
    {
        m_unit.m_onUpdate.RemoveListener(OnUnitUpdate);
    }

    public virtual void SelectMovesTargetAI()
    {
        //Select random player unit to target
        {
            var playerUnits = FindObjectOfType<TurnSystem>().m_players;
            m_unit.m_targetUnit = playerUnits[Random.Range(0, playerUnits.Count)];
        }

        //Select random move to use
        m_unit.SetMoveSelected(Random.Range(0, m_unit.m_unitMoves.Count));
    }

    protected virtual void OnUnitUpdate()
    {
        //Kill unit when they have no health
        if (m_unit.Health <= 0)
        {
            Destroy(m_unit.gameObject);
        }
    }
}
