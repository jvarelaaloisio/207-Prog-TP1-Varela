using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class Navigator : MonoBehaviour
{
    [SerializeField] private CustomButton playButton;
    [SerializeField] private CustomButton creditsButton;
    [SerializeField] private CustomButton exitButton;
    [SerializeField] private Transform pointee;
    [SerializeField] private float pointerTransitionDuration = 0.15f;

    private void OnEnable()
    {
        playButton.RequestPointer = HandlePointerRequest;
        creditsButton.RequestPointer = HandlePointerRequest;
        exitButton.RequestPointer = HandlePointerRequest;
    }

    private void HandlePointerRequest(Transform target)
    {
        LMotion.Create(pointee.position, target.position, pointerTransitionDuration)
               .WithEase(Ease.InOutQuad)
               // .Bind(vector3 => Debug.Log(vector3));
               .BindToPosition(pointee);
    }
}