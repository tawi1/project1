using UnityEngine;

namespace Prototype.NetworkLobby
{
    public class StateManager : MonoBehaviour
    {
        static int state = 0;

        public static int State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }
    }
}
