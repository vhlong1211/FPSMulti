using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        UIManager.ins.weaponTempSlide.maxValue = maxHeat;

        Transform pos = SpawnManager.ins.GetRandomPoint();
        transform.position = pos.position;
        transform.rotation = pos.rotation;
    }

    // Update is called once per frame
    void Update()
    {
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
            SwitchGun();
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;
            if (selectedGun < 0)
            {
                selectedGun = allGuns.Length - 1;
            }
            SwitchGun();
        }

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
            GameObject eff = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f),Quaternion.LookRotation(hit.normal,Vector3.up));
            Destroy(eff, 5f);
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

    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }
}
