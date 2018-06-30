using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using IllusionPlugin;

namespace NextRankPlugin
{
    public class NextRankPlugin : MonoBehaviour, IPlugin
    {

        public string Name => "Next Rank";
        public string Version => "1.0.0";

        public const int MenuIndex = 1;

        private SongSelectionMasterViewController _songSelectionView;
        private SongDetailViewController _songDetailViewController;
        private TMP_Text _highScoreText;
        private TMP_Text _nextRankText;
        private bool poll = false;

        public void OnApplicationStart()
        {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
        }

        public void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
        {
            if (scene.buildIndex == MenuIndex)
            {
                SetupUI();

                poll = true;
            }
            else
            {
                poll = false;
            }
        }

        public void SetupUI()
        {
            _songDetailViewController = Resources.FindObjectsOfTypeAll<SongDetailViewController>().FirstOrDefault();
            _songSelectionView = Resources.FindObjectsOfTypeAll<SongSelectionMasterViewController>().FirstOrDefault();
            _highScoreText = ReflectionUtil.GetPrivateField<TMP_Text>(_songDetailViewController, "_highScoreText");
            
            _highScoreText.rectTransform.anchoredPosition = new Vector2(0, -1);

            TMP_Text maxComboText = ReflectionUtil.GetPrivateField<TMP_Text>(_songDetailViewController, "_maxComboText");
            _nextRankText = Instantiate(maxComboText, maxComboText.transform.parent, false);
            _nextRankText.rectTransform.anchoredPosition = new Vector2(0, -5);

            TMP_Text statsHeader = Resources.FindObjectsOfTypeAll<TMP_Text>().FirstOrDefault(x => x.text == "Your Stats");
            statsHeader.rectTransform.anchoredPosition = new Vector2(0, 3);

            TMP_Text highscoreHeader = Resources.FindObjectsOfTypeAll<TMP_Text>().FirstOrDefault(x => x.text == "Highscore");
            TMP_Text nextRankHeader = Instantiate(highscoreHeader, highscoreHeader.transform.parent, false);
            nextRankHeader.text = "Next Rank";
            highscoreHeader.rectTransform.anchoredPosition = new Vector2(0, -1);
        }

        public void SetNextRankText()
        {
            if (!poll) return;

            // At launch, the UI exists, but no level is selected.
            string levelId = ReflectionUtil.GetPrivateField<string>(_songSelectionView, "_levelId");
            if (levelId == null) return;

            // If the player hasn't finished a level, blank out the Next Rank text to fit in with the rest of the UI.
            if (_highScoreText.text == "-")
            {
                _nextRankText.text = _highScoreText.text;
                return;
            }

            LevelStaticData levelData = _songSelectionView.GetLevelStaticDataForSelectedSong();
            LevelStaticData.Difficulty difficulty = ReflectionUtil.GetPrivateField<LevelStaticData.Difficulty>(_songDetailViewController, "_difficulty");
            LevelStaticData.DifficultyLevel difficultyLevel = levelData.GetDifficultyLevel(difficulty);
            GameplayMode gameplayMode = ReflectionUtil.GetPrivateField<GameplayMode>(_songSelectionView, "_gameplayMode");
            PlayerLevelStatsData playerLevelStatsData = PersistentSingleton<GameDataModel>.instance.gameDynamicData.GetCurrentPlayerDynamicData().GetPlayerLevelStatsData(levelData.levelId, difficulty, gameplayMode);

            _nextRankText.text = GetPointsToNextRank(difficultyLevel.songLevelData.songData.notesCount, playerLevelStatsData.highScore);
        }

        public string GetPointsToNextRank(int notes, int highScore)
        {
            int maxPointsPerNote = 110;
            int notesAt1x = 2;
            int notesAt2x = 4;
            int notesAt4x = 8;
            int notesUnder8x = notesAt1x + notesAt2x + notesAt4x;
            int pointsUnder8x = notesAt1x * maxPointsPerNote + notesAt2x * 2 * maxPointsPerNote + notesAt4x * 4 * maxPointsPerNote;
            int pointScoreAt8x = maxPointsPerNote * 8;
            int maxPoints = (notes - notesUnder8x) * pointScoreAt8x + pointsUnder8x;

            double percent = (double)highScore / maxPoints;

            double pointsToNextRank;

            if (percent <= 0.2f)
            {
                pointsToNextRank = (maxPoints * 0.2f) - highScore;
            }
            else if (percent <= 0.35f)
            {
                pointsToNextRank = (maxPoints * 0.35f) - highScore;
            }
            else if (percent <= 0.5f)
            {
                pointsToNextRank = (maxPoints * 0.5f) - highScore;
            }
            else if (percent <= 0.65f)
            {
                pointsToNextRank = (maxPoints * 0.65f) - highScore;
            }
            else if (percent <= 0.8f)
            {
                pointsToNextRank = (maxPoints * 0.8f) - highScore;
            }
            else if (percent <= 0.9f)
            {
                pointsToNextRank = (maxPoints * 0.9f) - highScore;
            }
            else
            {
                pointsToNextRank = maxPoints - highScore;
            }

            return pointsToNextRank.ToString("0");
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
            SetNextRankText();
        }
    }
}