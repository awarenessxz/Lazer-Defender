using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float padding = 1f;
    [SerializeField] float health = 200f;

    [Header("Projectile")]
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileFirePeriod = 0.1f;

    float xMin;
    float xMax;
    float yMin;
    float yMax;
    Coroutine fireRoutine;

    // Start is called before the first frame update
    void Start()
    {
        SetupMoveBoundaries();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        Fire();
    }


    /*
     * Camera View (imagine a rectangle):
     *                  01      11
     *                  00      10
     */
    private void SetupMoveBoundaries()
    {
        Camera gameCamera = Camera.main;
        xMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + padding; // ignore y and z, hence 0. x = 0 based on camera view
        xMax = gameCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - padding; // ignore y and z, hence 0. x = 1 based on camera view
        yMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + padding; // ignore x and z, hence 0. y = 0 based on camera view
        yMax = gameCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - padding; // ignore x and z, hence 0. y = 1 based on camera view
    }

    /*
     * Time.deltaTime makes movement frame rate indepdenent
     * Because Suppose the following
     * Frame per second:  10 vs 100
     * Distance per second: 1x10=10 vs 1x100=100
     * Duration of frame: 0.1s vs 0.01s
     * 
     * Hence, the faster computer will move faster...To make it constant, use Time.deltaTime
     */
    private void MovePlayer()
    {
        // Time.deltaTime makes it framework independent.
        var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;
        var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
        var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);
        transform.position = new Vector2(newXPos, newYPos);
    }

    private void Fire()
    {
        // check in Edit -> Project Settings -> Input Manager
        if (Input.GetButtonDown("Fire1"))
        {
            fireRoutine = StartCoroutine(FireContinously());
        }
        if (Input.GetButtonUp("Fire1"))
        {
            StopCoroutine(fireRoutine);
        }
    }

    // Coroutine
    IEnumerator FireContinously()
    {
        while (true)
        {
            GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity) as GameObject;
            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed);
            yield return new WaitForSeconds(projectileFirePeriod);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (!damageDealer) { return; } // protect against null
        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.GetDamage();
        damageDealer.Hit();
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
