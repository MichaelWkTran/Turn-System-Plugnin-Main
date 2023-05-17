using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatEnemy : MonoBehaviour
{
    //private GameObject UnitUI;
    [SerializeField]
    private Animator Animator;

    void Start()
    {
        //UnitUI = GetComponent<UnitStats>().m_UnitUI;
        Animator = transform.GetChild(1).GetComponent<Animator>();
    }

    void Update()
    {
        //This is to disable the Unit once it is defeated
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Down") && Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            //UnitUI.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    //This is used to replace the Unit with another
    void OnDestroy()
    {
        //Destroy(this.UnitUI);
    }
}
