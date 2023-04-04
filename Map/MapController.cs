using System.Collections.Generic;
using UnityEngine;

namespace SpaceBangBang
{
    public class MapController : MonoBehaviour
    {
        [field: SerializeField]
        public List<Transform> Spawnpos { get; private set; }
    }
}