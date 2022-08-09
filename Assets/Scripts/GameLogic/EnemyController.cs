using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject Weapon;
    public Animator animator;
    [SerializeField] private float Health;
    public int MaxHealth;

    public bool isDead = false;

    Collider2D _col;

    AnimsState animsState;

    enum AnimsState
    {
        Falling,
        Dead
    }

    private void OnEnable() 
    {
        _col = GetComponent<Collider2D>();
        Health = MaxHealth;       
        StartCoroutine(AnimatorStates());
    }
    
    void Start()
    {
        //GameController.enemies++;
        
    }

    private void OnTriggerEnter2D(Collider2D obstacle)
    {
        //for liquid
    }

    public GameObject _bloodVFX;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Damager damager))
        {
            Health -= damager.DamageSize;

            if (Health != 0 && Health < MaxHealth)
            {
                ///
            }
            else if (Health <= 0)
            {
                GameController.enemies--;
                _bloodVFX.transform.position = collision.contacts[0].point;
                _bloodVFX.SetActive(true);
                _col.enabled = false;

                //Collider2D otherCollider = transform.GetChild(0).GetComponent<BoxCollider2D>();
                //if(otherCollider == null)
                {
                 //   otherCollider = transform.parent.GetComponent<BoxCollider2D>();
                }
                //_col.attachedRigidbody.simulated = false;
                //otherCollider.enabled = false;
                if (Weapon != null)
                {
                    Weapon.SetActive(false);
                }

                _col.transform.GetChild(0).gameObject.layer = 13;
                //animator.Play("Death");
                //print("You win!");
            }
        }
    }

    void SetState(AnimsState state)
    {
        string txt = "";

        if (state== AnimsState.Falling)
        {
            txt = "Falling";
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
                if (_col.attachedRigidbody.velocity.y <= (-3))
                {
                    animsState = AnimsState.Falling;
                    SetState(animsState);
                }
            }
            
            else if (Health <= 0 && animsState != AnimsState.Dead) 
            {
                //if(AnimsState.Dead)
                animsState = AnimsState.Dead;
                SetState(animsState);
                //isDead = true;
            }
        }
    }
}
