using UnityEngine;

namespace Network.Connection
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private GameObject _prefabBoard;
        
        public GameObject PrefabBoard => _prefabBoard;
    }
}