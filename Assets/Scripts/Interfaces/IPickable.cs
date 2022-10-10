using UnityEngine;

namespace Interfaces
{
    public interface IPickable
    {
        void OnTriggerEnter2D(Collider2D col);
        void PickedUp();
    }
}