using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A class to add to any object in your scene to mark it as managed by a proximity manager.
    /// </summary>
    public class ProximityManaged : MonoBehaviour
    {
        [Header("Thresholds")]
        /// the distance from the proximity center (the player) under which the object should be enabled
        [Tooltip("the distance from the proximity center (the player) under which the object should be enabled")]
		public float EnableDistance = 35f;
        /// the distance from the proximity center (the player) after which the object should be disabled
        [Tooltip("the distance from the proximity center (the player) after which the object should be disabled")]
        public float DisableDistance = 45f;

        /// whether or not this object was disabled by the ProximityManager
        [MMReadOnly]
        [Tooltip("whether or not this object was disabled by the ProximityManager")]
        public bool DisabledByManager;
    }
}
