public class CharacteristicsRoomSwitch : HandMoveObject
{
    public CharacteristicsRoom HeadRoom;

    private bool isSwitched = false;

    protected override void FullOpened()
    {
        if(!isSwitched)
        {
            isSwitched = true;
            HeadRoom.SwitchOpened();
        }
    }

    protected override void FullClosed()
    {
        if(isSwitched)
        {
            isSwitched = false;
        }
    }
}
