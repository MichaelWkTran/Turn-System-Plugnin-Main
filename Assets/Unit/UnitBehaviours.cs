using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehaviours : MonoBehaviour
{
    SpriteRenderer m_spriteRenderer;
    Material m_defaultMaterial;
    [SerializeField] Material m_flashMaterial;

    public void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_defaultMaterial = m_spriteRenderer.sharedMaterial;
    }

    public void PointerEnter()
    {
        if (FindObjectOfType<TurnSystemSample>().CurrentBattleState != TurnSystemSample.BattleState.SelectTarget) return;
        m_spriteRenderer.material = m_flashMaterial;
    }

    public void PointerExit()
    {
        m_spriteRenderer.material = m_defaultMaterial;
    }
}
