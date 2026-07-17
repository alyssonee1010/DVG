using UnityEngine;

public class AttackBox : MonoBehaviour
{
    [SerializeField] int damageAmount = 10; //How much damage does the attack box do?
    enum HurtsWhat {Enemy, Player}
    [SerializeField] HurtsWhat hurtsWhat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        int attackSide;

        if(transform.position.x < collision.transform.position.x)
        {
            attackSide = -1;
        }
        else
        {
            attackSide = 1;
        }


        //If attack box is set to hurt the player, hurt the player. Otherwise, hurt the breakable
        if (hurtsWhat == HurtsWhat.Enemy)
        {
            if (collision.GetComponent<Breakable>())
            {
                collision.GetComponent<Breakable>().TakeDamage(damageAmount);
            }
        }
        else if (hurtsWhat == HurtsWhat.Player)
        {
            if (collision.gameObject == PlayerController.Instance.gameObject)
            {
                PlayerController.Instance.TakeDamage(damageAmount, attackSide);
            }
        }

       
    }
}
