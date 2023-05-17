using UnityEngine;

public class FlashRobot : BaseAnimatedEnemy
{
    public float FlashCooldownTime;
    public ParticleSystem HeadParticle;
    public Transform FlashRoot;
    public AudioSource BlindSource;
    public AudioClip FlashStepsSound, ExplodeSound;

    private float curFlashTime;
    private bool isRunning = false;

    private void Start()
    {
        curFlashTime = FlashCooldownTime;
    }

    protected override void Update()
    {
        if(isFighting && !isRunning)
        {
            if(curFlashTime > 0) curFlashTime -= Time.deltaTime;
            base.Update();
        }
        else if(isRunning)
        {
            if(!BlindSource.isPlaying)
            {
                BlindSource.clip = FlashStepsSound;
                BlindSource.Play();
            }    
            if (Vector3.Distance(transform.position, player.transform.position) <= 5 || curFlashTime <= 0)
            {
                BlindSource.Stop();
                BlindSource.clip = ExplodeSound;
                BlindSource.Play();

                HeadParticle.Stop();
                Anim.SetInteger("State", 0);
                isRunning = false;
                RunSpeedCor();
                Ray ray = new Ray(FlashRoot.position, player.CameraPos.position - FlashRoot.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10, TargetingLayerMask) && hit.collider.CompareTag("player") &&
                    Vector3.Angle(transform.forward, player.transform.forward) >= 80)
                    Game.GetBlindEffect(5, 100);
                curFlashTime = FlashCooldownTime;
            }
            else if(curFlashTime > 0)
            {
                curFlashTime -= Time.deltaTime;
                Agent.SetDestination(player.transform.position);

                ModelTransform.LookAt(player.transform.position);
                float y = ModelTransform.rotation.eulerAngles.y;
                ModelTransform.rotation = Quaternion.Euler(0, y, 0);
            }
        }
    }

    protected override void Target()
    {
        if (curFlashTime <= 0 && !isRunning)
        {
            if (Random.Range(0, 10) >= 5)
                curFlashTime = 2;
            else
            {
                isRunning = true;
                HeadParticle.Play();
                Agent.speed = RunSpeed;
                Anim.SetInteger("State", 1);
                curFlashTime = 5;
            }
        }
        else base.Target();
    }
}
