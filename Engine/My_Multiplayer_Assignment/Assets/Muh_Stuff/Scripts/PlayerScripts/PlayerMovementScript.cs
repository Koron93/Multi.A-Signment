using UnityEngine;
using Unity.Netcode;

public class PlayerMovementScript : MonoBehaviour
{
    [SerializeField] Camera m_Camera;
    [SerializeField] Animator anim;
    [SerializeField] CharacterController m_CharacterController;
    public bool Dead = false;
    private string currentAnimation = "";
    [SerializeField] float Speed = 1f;
    [SerializeField] float SprintSpeed = 3f;
    [SerializeField] float sensitivity = 1f;
    float rotationX = 1f;
    public void Movement()
    {
        float IncrementZ = Input.GetAxis("Horizontal");
        float IncrementX = Input.GetAxis("Vertical");

        Vector3 move = transform.right * IncrementZ + transform.forward * IncrementX;
        if (Input.GetKey("left shift"))
        { m_CharacterController.Move(move * SprintSpeed * Time.deltaTime); }
        else { m_CharacterController.Move(move * Speed * Time.deltaTime); }

    }
    public void Rotation()
    {
        float XInput = Input.GetAxis("Mouse X");

        transform.Rotate(0, XInput * sensitivity, 0);
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        m_Camera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
    public void ChechAnimation()
    {
        if (Dead)
        {
            ChangeAnimation("Death");
        }
        else
        {
            ChangeAnimation("BaseAnim");
        }
    }
    private void ChangeAnimation(string animationName)
    {
        if (currentAnimation != animationName)
        {
            currentAnimation = animationName;
            anim.CrossFade(animationName, 0.2f);
        }
    }
}
