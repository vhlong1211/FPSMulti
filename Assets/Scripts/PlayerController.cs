using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;

    public float moveSpeed = 5f, runSpeed = 8f;
    private float activeMoveSpeed;
    private Vector3 moveDir;
    private Vector3 movement;

    public CharacterController cc;
    private Camera cam;
    public Animator animator;

    public float jumpForce = 12, gravityMod = 2.5f;

    public List<Transform> groundCheckPoints;
    private bool isGrounded;
    public LayerMask groundLayer;

    public GameObject bulletImpact;
    private float shotCounter;

    public float maxHeat = 10f, coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool overHeated;

    public Gun[] allGuns;
    private int selectedGun;

    public float muzzleDisplayTime;
    private float muzzleCounter;
    private bool isSetup = false;

    public GameObject playerHitImpact;
    public GameObject model;
    public Transform gunHolder;
    public Transform gunAdjust;

    public int maxHealth = 100;
    private int currentHealth;

    [PunRPC]
    public void Setup()
    {
        isSetup = true;
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        currentHealth = maxHealth;

        if (photonView.IsMine)
        {
            Debug.Log("set up gun mine :" + photonView.Owner.NickName);

            model.SetActive(false);
            UIManager.ins.weaponTempSlide.maxValue = maxHeat;
            UIManager.ins.playerHealthSlide.maxValue = maxHealth;
        }
        else
        {
            Debug.Log("set up gun :" + photonView.Owner.NickName);
            gunHolder.SetParent(gunAdjust);
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
            gunHolder.localScale = Vector3.one;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (!isSetup) return;
        //Movement
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") * mouseSensitivity, Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

        viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);

        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        float yVel = movement.y;
        movement = (transform.forward * moveDir.z + transform.right * moveDir.x).normalized;
        movement.y = yVel;
        if (cc.isGrounded)
        {
            movement.y = 0;
        }

        isGrounded = false;
        foreach (var a in groundCheckPoints)
        {
            bool isGr = Physics.Raycast(a.position, Vector3.down, 0.25f);
            if (isGr)
            {
                isGrounded = true;
                break;
            }
        }
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        }
        else
        {
            activeMoveSpeed = moveSpeed;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        cc.Move(movement * activeMoveSpeed * Time.deltaTime);

        if (allGuns[selectedGun].muzzleFlash.activeSelf)
        {
            muzzleCounter -= Time.deltaTime;
            if (muzzleCounter <= 0)
            {
                allGuns[selectedGun].muzzleFlash.SetActive(false);
            }
        }

        //Shoot
        if (!overHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
            if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0)
                {
                    Shoot();
                }
            }

            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overheatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                overHeated = false;

                UIManager.ins.overheatedTxt.gameObject.SetActive(false);
            }
        }

        if (heatCounter < 0) heatCounter = 0;
        UIManager.ins.weaponTempSlide.value = heatCounter;

        //Switching Gun
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun++;
            if (selectedGun >= allGuns.Length)
            {
                selectedGun = 0;
            }
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;
            if (selectedGun < 0)
            {
                selectedGun = allGuns.Length - 1;
            }
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }

        animator.SetBool("grounded", isGrounded);
        animator.SetFloat("speed", moveDir.magnitude);

        //escape cursor
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(cam.transform.position,ray.direction * 100f,Color.black);
        //ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point,Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].shotDamage);
            }
            else
            {
                GameObject eff = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(eff, 5f);
            }

        }
        shotCounter = allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;
            UIManager.ins.overheatedTxt.gameObject.SetActive(true);

        }

        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    private void SwitchGun()
    {
        foreach (var a in allGuns)
        {
            a.muzzleFlash.SetActive(false);
            a.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);
    }
    [PunRPC]
    public void SetGun(int targetGun)
    {
        if (targetGun < allGuns.Length)
        {
            selectedGun = targetGun;
            SwitchGun();
        }
    }

    [PunRPC]
    public void DealDamage(string damager,int damage)
    {
        TakeDamage(damager,damage);
    }
    public void TakeDamage(string damager, int damage)
    {
        if (photonView.IsMine)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                PlayerSpawner.ins.Die(damager);
            }
            UIManager.ins.playerHealthSlide.value = currentHealth;
        }
    }

    private void LateUpdate()
    {
        if (!isSetup) return;

        if (photonView.IsMine)
        {
            cam.transform.position = viewPoint.position;
            cam.transform.rotation = viewPoint.rotation;
        }
    }
}
