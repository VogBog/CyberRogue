using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Rifle : Glock
{
    public Vector3 LeftHandWhenReloadWithoutMagazinePos;
    public Vector3 LeftHandWhenReloadWithoutMagazineRot;

    private Quaternion LocalMagazineRotationInLeftHand;
    private Vector3 LocalMagazinePositionInLeftHand;

    protected override IEnumerator ReloadAnimForPCIE(Transform leftHand, Transform rightHand, Transform AmmoPos, PCPlayerController pc)
    {
        if (!isAmmoInMe)
            yield return new WaitForSeconds(.5f);

        if (isPlayerHoldMe)
        {
            pc.SetReloadAnim(true);

            Vector3 rightHandPos = rightHand.localPosition + new Vector3(-.1f, .1f, 0);
            bool OKLetsGo = false;

            if (isAmmoInMe)
            {
                rightHand.DOLocalRotate(new Vector3(-50, -30, 0), .5f);
                rightHand.DOLocalMove(rightHandPos, .5f);
                leftHand.DOLocalRotate(LeftHandWhenReloadLocalRot, .3f);
                leftHand.DOLocalMove(LeftHandWhenReloadLocalPos, .3f);
                yield return new WaitForSeconds(.3f);

                Magazine.transform.SetParent(leftHand);

                LocalMagazinePositionInLeftHand = Magazine.transform.localPosition;
                LocalMagazineRotationInLeftHand = Magazine.transform.localRotation;

                leftHand.DOLocalMove(LeftHandWhenReloadWithoutMagazinePos, .2f);
                leftHand.DOLocalRotate(LeftHandWhenReloadWithoutMagazineRot, .2f);

                yield return new WaitForSeconds(.2f);
                PlayerTookMyAmmo();
                OKLetsGo = true;
            }

            if (isPlayerHoldMe)
            {
                if (!OKLetsGo)
                {
                    rightHand.DOLocalMove(rightHandPos, .1f);
                    rightHand.DOLocalRotate(new Vector3(-50, -30, 0), .1f);
                }

                Magazine.transform.SetParent(AmmoPos);
                Magazine.transform.position = AmmoPos.position;
                Magazine.transform.rotation = new Quaternion();

                leftHand.DOLocalMove(AmmoPos.localPosition + Vector3.down / 2, .3f);

                yield return new WaitForSeconds(.3f);

                if (isPlayerHoldMe)
                {
                    StartCoroutine(ReloadAmmoAnimPCState1(leftHand, rightHand));
                    yield return new WaitForSeconds(.6f);

                    if (isPlayerHoldMe)
                    {
                        PlayerGiveMeAmmo();
                        isAmmoInMe = false;
                        leftHand.DOLocalMove(new Vector3(-1, -1, 0), .2f);
                        pc.UpdateRightHandAnim(true);
                        pc.UpdateLeftHandAnim(HoldWithLeftHand);
                        pc.SetReloadAnim(false);
                        rightHand.DOLocalRotate(Vector3.zero, .2f);

                        yield return new WaitForSeconds(.1f);

                        isAmmoInMe = true;
                    }
                }
            }
        }
    }

    protected override IEnumerator ReloadAmmoAnimPCState1(Transform leftHand, Transform rightHand)
    {
        Magazine.transform.SetParent(leftHand);
        Magazine.transform.localPosition = LocalMagazinePositionInLeftHand;
        Magazine.transform.localRotation = LocalMagazineRotationInLeftHand;
        leftHand.localPosition += Vector3.down / 2;
        leftHand.localRotation = Quaternion.Euler(LeftHandWhenReloadWithoutMagazineRot);
        leftHand.DOLocalMove(LeftHandWhenReloadWithoutMagazinePos, .6f);

        yield return new WaitForSeconds(.5f);

        leftHand.DOLocalRotate(LeftHandWhenReloadLocalRot, .1f);
        leftHand.DOLocalMove(LeftHandWhenReloadLocalPos, .1f);
    }
}
