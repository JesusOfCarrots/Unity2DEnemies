using Cinemachine;
using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PatorlJump : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Grounded")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask whatIsGround;
    [Range(0f, 1f)]
    [SerializeField] private float groundRadius = .2f;    //.4f
    bool isGrounded = false;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolPointWaitTime = 10f;
    [SerializeField] private Transform targetPlayerTransform;

    [Header("Jump")]
    [SerializeField] private float targetJumpDistance = 1.5f;
    [SerializeField] private float playerDetectionRange = 5f;
    [SerializeField] private float jumpInitializeTime = 0.4f;
    [SerializeField] private float jumpBackIniTime = .6f;
    private bool inRange;
    private bool hasExecuted = false;

    private int currentPatrolPointIndex = 0;
    private bool isWaiting;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        inRange = Vector2.Distance(transform.position, targetPlayerTransform.position) <= playerDetectionRange && !hasExecuted;

        if (!inRange)
        {
            Patrol();
        }
        else
        {
            StartCoroutine(JumpToTarget(targetPlayerTransform));
            hasExecuted = true;
        }
    }

    IEnumerator JumpToTarget(Transform target)
    {
        // Wait Initialization time
        yield return new WaitForSeconds(jumpInitializeTime);

        // Include sprite size
        float thisX = GetComponent<SpriteRenderer>().bounds.size.x;
        thisX = (thisX + targetJumpDistance + 0.2f) / 2;

        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.position;

        // Calculate x distance including player sprite width
        float distanceX = (targetPos.x - currentPos.x - thisX) / 2f;

        // Calculate time to reach apex
        float timeToApex = (targetPos.x - currentPos.x) / (2 * distanceX);
        float totalTime = timeToApex * 2;

        // Calculate jumpVelocity
        Vector2 jumpVelocity = new Vector2(distanceX / timeToApex, 10);
        rb.AddForce(jumpVelocity, ForceMode2D.Impulse); // alternatively use rb.velocity += jumpeVelocity;

        // Call jump back function and set hasExecuted = false;
        yield return new WaitForSeconds(totalTime + jumpBackIniTime); // Waits until jump has finished and the jumpBackTime
        Debug.Log("Function finished jumping.");

        if(target != patrolPoints[currentPatrolPointIndex])
        {
            StartCoroutine(JumpToTarget(patrolPoints[currentPatrolPointIndex]));
        }
        else
        {
            hasExecuted = false;
            Patrol();
        }
    }

    void Patrol()
    {
        if (hasExecuted)
        {
            return;
        }

        Vector2 targetPosition = patrolPoints[currentPatrolPointIndex].position;
        targetPosition.y = transform.position.y;

        if (isGrounded)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Flip sprite based on movement direction
            if (transform.position.x < targetPosition.x)
            {
                // Moving right
                transform.localScale = new Vector2(1, transform.localScale.y);
            }
            else
            {
                // Moving left
                transform.localScale = new Vector2(-1, transform.localScale.y);
            }
        }

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f && !isWaiting)
        {
            currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
            isWaiting = true;
            Invoke("Temp", patrolPointWaitTime);
        }
    }

    void Temp() { isWaiting = false; }

    private void FixedUpdate()
    {
        //isGrounded check
        bool touchingGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundRadius, whatIsGround);

        if (touchingGround)
        { isGrounded = true; }
        else
        { isGrounded = false; }
    }

    private void OnDrawGizmos()
    {
        //Ground-check Sphere
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheckPoint.position, groundRadius);
    }
}
