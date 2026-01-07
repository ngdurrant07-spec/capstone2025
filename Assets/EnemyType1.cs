using UnityEngine;
using UnityEngine.Rendering;

public class EnemyType1 : MonoBehaviour
{
    public Transform Player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Is Grounded?
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);

        //Player Direction
        float direction = Mathf.Sign(Player.position.x - transform.position.x);

        //Player above direction
        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f, 1 << Player.gameObject.layer);

        if (isGrounded)
        {
            //Chase Player
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

            //Jump if there's a gap ahead && no ground in front
            //Else if there's a player abbove and platform above

            //If Ground
            RaycastHit2D groundInfront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);
            //If Gap
            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer);
            //If Platform Above
            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f, groundLayer);

            if(!groundInfront && !gapAhead)
            {
                shouldJump = true;
            }
            else if(isPlayerAbove && platformAbove)
            {
                shouldJump = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if(isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 direction = (Player.position - transform.position).normalized;

            Vector2 jumpDirection = direction * jumpForce;

            rb.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
        }
    }
}
