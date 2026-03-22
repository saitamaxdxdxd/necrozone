using UnityEngine;
using Necrozone.Managers;

namespace Necrozone.Gameplay
{
    // TEMPORAL — borrar antes de release
    public class TestButtons : MonoBehaviour
    {
        public void TriggerGameOver()     => GameManager.Instance.GameOver();
        public void TriggerLevelComplete() => GameManager.Instance.LevelComplete();
    }
}
