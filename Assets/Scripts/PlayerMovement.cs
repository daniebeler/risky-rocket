using System.Collections;
using UnityEngine;

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

    [SerializeField]
    private ParticleSystem particles;

    private General general;

    private SoundController soundController;

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
                if (Input.touchCount > 0)
                {
                    cf.force = new Vector2(Input.acceleration.x * ForceHorizontalMobile, ForceUp);
                }
                else
                {
                    cf.force = new Vector2(Input.acceleration.x * ForceHorizontalMobile, ForceDown);
                }

                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    particles.Play();
                    soundController.fadeInSound();
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    particles.Stop();
                    soundController.fadeOutSound();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    cf.force = new Vector2(ForceHorizontal, cf.force.y);
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    cf.force = new Vector2(-ForceHorizontal, cf.force.y);
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    cf.force = new Vector2(cf.force.x, ForceUp);
                    particles.Play();
                    soundController.fadeInSound();
                }

                if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
                {
                    cf.force = new Vector2(0, cf.force.y);
                }

                if (Input.GetKeyUp(KeyCode.Space))
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
        particles.Stop();
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void setPlayingStatus(bool status)
    {
        this.playing = status;
    }

}
