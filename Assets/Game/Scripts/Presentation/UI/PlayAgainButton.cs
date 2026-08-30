using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Presentation
{
    /// <summary>
    /// Restarts the run from the end screen. Reloading the active scene is the whole reset: every
    /// controller, the notebook, the leaked rooms and the story flags are rebuilt from the assets,
    /// so nothing can survive a run by accident.
    ///
    /// The intro is the one thing not replayed — see <see cref="RunSession.SkipIntro"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PlayAgainButton : MonoBehaviour
    {
        [Tooltip("Off replays the intro too. On drops the player straight into day 1, with the house already in its post-intro state.")]
        [SerializeField] private bool _skipIntro = true;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(Restart);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(Restart);
        }

        public void Restart()
        {
            RunSession.SkipIntro = _skipIntro;

            // The button lives on the end panel, which is a child of the scene being unloaded —
            // there is nothing to clean up, the load takes the whole hierarchy with it.
            Scene active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.buildIndex);
        }
    }
}
