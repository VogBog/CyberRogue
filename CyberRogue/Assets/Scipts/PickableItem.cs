using UnityEngine;

public abstract class PickableItem : InGameButton
{
    public GameObject PickMeParticle;
    public GameObject PickGO;
    public string ItemName;
    public Rigidbody Body;
    public bool IsPickFromCollision;

    public SpecialRoom HeadSpecialRoom { get; set; }

    protected bool isPickedAlready = false;
    protected BasePlayer player;

    private bool isDroped = true;
    private float pickTime = 0;
    private Transform MainCamera;
    private float pickSpeed = 10;

    public override void OnDraging(Vector3 dragPos) { }

    public override void StopDraging() { }

    public override void OnChoosen()
    {
        if (isDroped && PickGO != null)
            PickGO.SetActive(true);
    }

    public override void OnClick(BasePlayer player)
    {
        if(isDroped && canPick(player))
        {
            isDroped = false;
            pickTime = .5f;
            if(PickMeParticle != null)
                PickMeParticle.SetActive(false);
        }
    }

    public override void OnUnchoosen()
    {
        if(PickGO != null)
            PickGO.SetActive(false);
    }

    public void PlayerDropMe()
    {
        isPickedAlready = false;
        isDroped = true;
        SetBodyParameters(false);
        Body.AddForce(player.transform.forward * 3);
        if(PickMeParticle != null)
            PickMeParticle.SetActive(true);
        DropItem();
    }

    protected virtual void DropItem() { }

    private void Start()
    {
        GameManager game = FindObjectOfType<GameManager>();
        MainCamera = game.MainCamera.transform;
        player = game.Player;
        AfterStart();
    }

    protected virtual void AfterStart() { }

    protected abstract bool canPick(BasePlayer player);

    private void Update()
    {
        if (pickTime > 0 && !isDroped)
        {
            pickTime -= Time.deltaTime;
            Vector3 moveVec = MainCamera.transform.position + Vector3.down * .5f - transform.position;
            transform.position += moveVec * pickSpeed * Time.deltaTime;
        }
        else if (!isPickedAlready && !isDroped)
        {
            SetBodyParameters(true);
            isPickedAlready = true;
            PickItem(player);
            if (HeadSpecialRoom != null)
                HeadSpecialRoom.PlayerPickItem(this);
        }
        AfterUpdate();
    }

    protected virtual void AfterUpdate() { }

    private void SetBodyParameters(bool isPicked)
    {
        Body.useGravity = !isPicked;
        Body.constraints = isPicked ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
    }

    protected abstract void PickItem(BasePlayer player);

    private void OnTriggerEnter(Collider other)
    {
        if (IsPickFromCollision && other.gameObject.CompareTag("player"))
        {
            OnClick(other.gameObject.GetComponent<BasePlayer>());
        }
    }
}
