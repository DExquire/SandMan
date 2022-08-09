using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SandmanController : MonoBehaviour
{
    public Collider2D collider;
    enum AnimsState
    {
        StartCharge,
        EndCharge,
        Dead
    }
    //public bool hasWeapon;
    //[Helper.ConditionalHide("hasWeapon")]
    public GameObject Weapon;
    public Animator animator;
    private float Health;
    public int MaxHealth;
    public static Animator an;
    void Start()
    {
        //GameController.players++;
        collider = GetComponent<Collider2D>();
        Health = MaxHealth;
        StartCoroutine(AnimatorStates());
        an = animator;
    }

    private void OnCollisionEnter2D(Collision2D collision)   
    {
        //Damager damager = obstacle.collider.TryGetComponent(out Damager damager);
        if(collision.collider.TryGetComponent(out Damager damager))
        {
            //if(obstacle.gameObject.tag == "wood") //как пример сравнивать по тегу
            {
                Health -= damager.DamageSize;
                if(Health != 0 && Health < MaxHealth)
                {
                    //Weapon.SetActive(false); maybe not disactive weapon
                    //animator.Play("Death"); //set other animation clip
                    //gameController.OnDamageReceived();
                }
                else if(Health <= 0)
                {
                    GameController.players--;
                    collider.enabled = false;

                    if (Weapon != null)
                    {
                        Weapon.SetActive(false);
                    }
                    animator.Play("Death");
                    
                }
            }
        }
    }

    void SetState(AnimsState state)
    {
        string txt = "";

        if (state== AnimsState.EndCharge)
        {
            txt = "EndCharge";
        }
        else if (state == AnimsState.StartCharge)
        {
            txt = "StartCharge";

        }
        else if(state == AnimsState.Dead)
        {
            txt = "Dead";
        }
        animator.SetTrigger(txt);
    }
    IEnumerator AnimatorStates()
    {
        while (true)
        {
            yield return null;
            
            if(Health > 0)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    SetState(AnimsState.StartCharge);
                }
                else if (Input.GetKeyUp(KeyCode.Mouse0)) 
                {
                    SetState(AnimsState.EndCharge);

                }
            }
            else if (Health <= 0) 
            {
                SetState(AnimsState.Dead);
            }
        }
    }
}
