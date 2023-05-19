using UnityEngine;

public class HandMoveObject : InGameButton
{
    public Vector3 StartTriggerScale, ChoosenTriggerScale;
    public Transform TriggerTransform;
    public Transform StartPosCursor, EndPosCursor;
    public Transform StartPosObject, EndPosObject;
    public float StartOffset, EndOffset, MinusSinus;

    protected Vector3 startPosCursorVec { get; private set; }
    protected Vector3 endPosCursorVec { get; private set; }
    protected Vector3 startPosObjectVec { get; private set; }
    protected Vector3 endPosObjectVec { get; private set; }

    private BasePlayer _player;
    protected BasePlayer player
    {
        get
        {
            if (!_player)
                _player = FindObjectOfType<BasePlayer>();
            return _player;
        }
    }

    private void Start()
    {
        UpdatePoses();
    }

    public void UpdatePoses()
    {
        startPosCursorVec = StartPosCursor.position;
        endPosCursorVec = EndPosCursor.position;
        startPosObjectVec = StartPosObject.position;
        endPosObjectVec = EndPosObject.position;
    }

    public override void OnChoosen() { }

    public override void OnClick(BasePlayer player) { }

    public override void OnDraging(Vector3 dragPos)
    {
        TriggerTransform.localScale = ChoosenTriggerScale;
        float d1 = Vector3.Distance(startPosCursorVec, dragPos);
        float d2 = Vector3.Distance(endPosCursorVec, dragPos);
        float mult = Mathf.Clamp(d1 / (d1 + d2), 0, 1);
        float a = Mathf.PI / (EndOffset - StartOffset);
        if (mult <= StartOffset) mult = 0;
        else if (mult >= EndOffset) mult = 1;
        else
            mult = .5f * Mathf.Sin(a * (mult - (Mathf.PI / 2) - MinusSinus)) + .5f;
        Vector3 finalPos = startPosObjectVec + (endPosObjectVec - startPosObjectVec) * mult;
        transform.position = finalPos;
        if (mult >= EndOffset)
            FullOpened();
        else if (mult <= StartOffset)
            FullClosed();
    }

    protected virtual void FullOpened() { }

    protected virtual void FullClosed() { }

    public override void OnUnchoosen() { }

    public override void StopDraging()
    {
        TriggerTransform.localScale = StartTriggerScale;
    }
}
