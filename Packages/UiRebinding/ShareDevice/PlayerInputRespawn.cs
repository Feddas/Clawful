using UnityEngine;
using UnityEngine.InputSystem;

namespace ShareDevice
{
    /// <summary> Stores where a PlayerInput should respawn. </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputRespawn : MonoBehaviour
    {
        /// <summary> Position where this player will be re/spawned. Move this transform when a checkpoint is reached. </summary>
        public TransformAtIndex Respawn { get; private set; }

        /// <summary> Required <see cref="PlayerInput"/> component invokes this function via UnityEvent when DropPlayer action is triggered. </summary>
        public void OnDropPlayer(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Destroy(gameObject);
                // TODO: remove PlayerInputManager dependancy on NotifyPlayerLeft. Instead create a C# event, OnDropped, for TestDropPlayer to subscribe to.
            }
        }

        public virtual void SetPosition(TransformAtIndex spawn)
        {
            Respawn = spawn;
            transform.position = Respawn.Transform.position;
        }
    }
}
