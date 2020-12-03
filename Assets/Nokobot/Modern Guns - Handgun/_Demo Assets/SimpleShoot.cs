using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("Nokobot/Modern Guns/Simple Shoot")]
public class SimpleShoot : MonoBehaviour
{
    [Header("Prefab Refrences")] 
    
    public int maxammo = 10;
    private int currentammo;
    public TMPro.TextMeshPro text;
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;
    public GameObject line;

    [Header("AudioSettings")]
    public AudioSource source;
    public AudioClip fire;
    public AudioClip reload;
    public AudioClip noammo;
    
    [Header("Location Refrences")]
    [SerializeField] private Animator gunAnimator;
    [SerializeField] public Transform barrelLocation;
    [SerializeField] public Transform casingExitLocation;

    [Header("Settings")]
    [Tooltip("Specify time to destory the casing object")] [SerializeField] private float destroyTimer = 2f;
    [Tooltip("Bullet Speed")] [SerializeField] public float shotPower = 1000f;
    [Tooltip("Casing Ejection Speed")] [SerializeField] private float ejectPower = 500f;


    void Start()
    {
        if (barrelLocation == null)
            barrelLocation = transform;
        Reload();

    }

    void Reload()
    {
        currentammo = maxammo;
        source.PlayOneShot(reload);
    }
    
    void Update()
    {
        //If you want a different input, change it here
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            if (currentammo > 0)
                //Calls animation on the gun that has the relevant animation events that will fire
                GetComponent<Animator>().SetTrigger("Fire");
            else
                source.PlayOneShot(noammo);
        }

        if (Vector3.Angle(transform.up, Vector3.up) > 100 && currentammo < maxammo)
            Reload();

            text.text = currentammo.ToString();
        
    }


    //This function creates the bullet behavior
    public void Shoot()
    {
        currentammo--;
        source.PlayOneShot(fire);
     
        GameObject tempFlash;
        if (bulletPrefab)
            Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation).GetComponent<Rigidbody>().AddForce(barrelLocation.forward * shotPower, ForceMode.VelocityChange);
         tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);
        
        RaycastHit hitInfo;
        bool hasHit = Physics.Raycast(barrelLocation.position, barrelLocation.forward, out hitInfo, 100);
        
        if(hasHit)
            hitInfo.collider.SendMessageUpwards("Dead", hitInfo.point,SendMessageOptions.DontRequireReceiver);

        if(line)
        {
            GameObject liner = Instantiate(line);
            liner.GetComponent<LineRenderer>().SetPositions(new Vector3[] {barrelLocation.position, barrelLocation.position + barrelLocation.forward * 100});
            Destroy(liner, 0.5f);
        }

        // { return; }

        // Create a bullet and add force on it in direction of the barrel


    }

    //This function creates a casing at the ejection slot
    void CasingRelease()
    {
        //Cancels function if ejection slot hasn't been set or there's no casing
        if (!casingExitLocation || !casingPrefab)
        { return; }

        //Create the casing
        GameObject tempCasing;
        tempCasing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation) as GameObject;
        //Add force on casing to push it out
        tempCasing.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower), (casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f), 1f);
        //Add torque to make casing spin in random direction
        tempCasing.GetComponent<Rigidbody>().AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);

        //Destroy casing after X seconds
        Destroy(tempCasing, destroyTimer);
    }

}
