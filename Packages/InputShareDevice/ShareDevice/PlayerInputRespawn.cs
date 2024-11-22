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

        /// <summary> selection the player made </summary>
        private GameObject respawnPrefab;

        /// <summary> players instantiated instance of <seealso cref="respawnPrefab"/>. </summary>
        private GameObject respawnedAs;

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

        /// <summary> Set prefab to be used in <seealso cref="FrameAfterSubmit"/></summary>
        public void RespawnAs(GameObject respawnCharacter)
        {
            respawnPrefab = respawnCharacter;
        }

        /// <summary> called after a player has modified their individual selection lock for a UiSelectedOnEnable.IsGroupSelect. Such as by PlayeruiMultiSelect's UnityEvent OnSelectionChanged </summary>
        /// <param name="isLocked"></param>
        public void OnSelectionChanged(bool isLocked)
        {
            if (isLocked && respawnPrefab != null)
            {
                respawnedAs = Instantiate(respawnPrefab, this.transform); // lock new selection
            }
            else if (respawnedAs != null)
            {
                Destroy(respawnedAs); // remove previous selection.
            }
        }
    }
}