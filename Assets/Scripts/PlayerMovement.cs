using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class PlayerMovement : MonoBehaviour
{

    private ConstantForce2D cf;
    private Rigidbody2D rigid;
    private float ForceDown = -5;
    private float ForceUp = 8;
    private float ForceHorizontal = 8f;
    private float ForceHorizontalMobile = 16f;
    private bool playing = false;
    private Vector2 startPos = new Vector2(0f, 0f);

    private bool thrusting = false;

    [SerializeField]
    private ParticleSystem particles;

    private General general;

    private SoundController soundController;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        if (Accelerometer.current != null)
        {
            InputSystem.EnableDevice(Accelerometer.current);
        }
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        if (Accelerometer.current != null)
        {
            InputSystem.DisableDevice(Accelerometer.current);
        }
    }

    void Start()
    {
        cf = GetComponent<ConstantForce2D>();
        rigid = GetComponent<Rigidbody2D>();
        cf.force = new Vector2(0f, ForceDown);

        general = GameObject.FindGameObjectWithTag("ScriptHolder").GetComponent<General>();
        soundController = GameObject.FindGameObjectWithTag("ScriptHolder").GetComponent<SoundController>();
    }

    void Update()
    {
        if (playing)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                float tilt = Accelerometer.current != null
                    ? Accelerometer.current.acceleration.ReadValue().x
                    : 0f;

                if (Touch.activeTouches.Count > 0)
                {
                    TouchPhase phase = Touch.activeTouches[0].phase;

                    if (phase == TouchPhase.Began)
                    {
                        thrusting = true;
                        particles.Play();
                        soundController.fadeInSound();
                    }
                    else if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
                    {
                        thrusting = false;
                        particles.Stop();
                        soundController.fadeOutSound();
                    }
                }
                else if (thrusting)
                {
                    thrusting = false;
                    particles.Stop();
                    soundController.fadeOutSound();
                }

                cf.force = new Vector2(tilt * ForceHorizontalMobile, thrusting ? ForceUp : ForceDown);
            }
            else
            {
                Keyboard kb = Keyboard.current;
                if (kb == null) return;

                if (kb.rightArrowKey.wasPressedThisFrame)
                {
                    cf.force = new Vector2(ForceHorizontal, cf.force.y);
                }

                if (kb.leftArrowKey.wasPressedThisFrame)
                {
                    cf.force = new Vector2(-ForceHorizontal, cf.force.y);
                }

                if (kb.spaceKey.wasPressedThisFrame)
                {
                    cf.force = new Vector2(cf.force.x, ForceUp);
                    particles.Play();
                    soundController.fadeInSound();
                }

                if (kb.rightArrowKey.wasReleasedThisFrame || kb.leftArrowKey.wasReleasedThisFrame)
                {
                    cf.force = new Vector2(0, cf.force.y);
                }

                if (kb.spaceKey.wasReleasedThisFrame)
                {
                    cf.force = new Vector2(cf.force.x, ForceDown);
                    particles.Stop();
                    soundController.fadeOutSound();
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!(collision.gameObject.tag == "Start" || collision.gameObject.tag == "Finish" || collision.gameObject.tag == "survive"))
        {
            setPlayerPos();
            StartCoroutine(unlockMovement());
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (playing)
        {
            if (collision.gameObject.tag == "Finish" && rigid.linearVelocity == new Vector2(0, 0))
            {
                playing = false;
                rigid.constraints = RigidbodyConstraints2D.FreezeAll;
                if (PlayerPrefs.GetInt("unlockedlevels", 1) <= PlayerPrefs.GetInt("currentlevel", 1))
                {
                    PlayerPrefs.SetInt("unlockedlevels", PlayerPrefs.GetInt("currentlevel", 1) + 1);
                }
                general.FadeOutGame();
            }
        }
    }

    IEnumerator unlockMovement()
    {
        yield return new WaitForSeconds(0.3f);
        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void setPlayerPos(Vector2 platformPos)
    {
        platformPos.y += GetComponent<PolygonCollider2D>().bounds.size.y / 1.5f;
        startPos = platformPos;
        setPlayerPos();
    }

    public void setPlayerPos()
    {
        transform.position = startPos;
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void lockMovement()
    {
        playing = false;
        thrusting = false;
        cf.force = new Vector2(0f, ForceDown);
        particles.Stop();
        soundController.fadeOutSound();
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void setPlayingStatus(bool status)
    {
        this.playing = status;
    }

}