using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatEnemy : MonoBehaviour
{
    public void OnUnitUpdate(UnitBase _unit)
    {
        if (_unit.Health <= 0)
        {
            Destroy(_unit.gameObject);
        }
    }
}
