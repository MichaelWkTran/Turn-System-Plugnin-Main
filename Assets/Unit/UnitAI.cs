using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitAI : MonoBehaviour
{
    Unit m_unit;

    void Start()
    {
        m_unit = GetComponent<Unit>();
        m_unit.OnUpdate.AddListener(() => OnUnitUpdate(m_unit));
    }

    public virtual void SelectMovesTargetAI()
    {
        //Select random player unit to target
        {
            var playerUnits = FindObjectOfType<TurnSystem>().m_players;
            m_unit.m_targetUnit = playerUnits[Random.Range(0, playerUnits.Count - 1)];
        }

        //Select random move to use
        m_unit.SetMoveSelected(Random.Range(0, m_unit.m_unitMoves.Count - 1));
    }

    public virtual void OnUnitUpdate(Unit _unit)
    {
        //Kill unit when they have no health
        if (_unit.Health <= 0)
        {
            Destroy(_unit.gameObject);
        }
    }
}
