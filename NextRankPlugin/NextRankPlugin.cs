using System;
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
        public string Version => "1.3.0";

        public const string MenuName = "Menu";

        private StandardLevelDetailViewController _songDetailViewController;
        private TMP_Text _highScoreText;
        private TMP_Text _nextRankText;
        private bool poll = false;

        public void OnApplicationStart()
        {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
        }

        public void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
        {
            if (scene.name == MenuName)
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
            _songDetailViewController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
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
            LevelSO.DifficultyBeatmap difficultyBeatMap = ReflectionUtil.GetPrivateField<LevelSO.DifficultyBeatmap>(_songDetailViewController, "_difficultyBeatmap");
            if (difficultyBeatMap == null) return;

            // If the player hasn't finished a level, blank out the Next Rank text to fit in with the rest of the UI.
            if (_highScoreText.text == "-")
            {
                _nextRankText.text = _highScoreText.text;
                return;
            }

            BeatmapDifficulty difficulty = difficultyBeatMap.difficulty;
            int notesCount = difficultyBeatMap.beatmapData.notesCount;
            PlayerDataModelSO.LocalPlayer player = ReflectionUtil.GetPrivateField<PlayerDataModelSO.LocalPlayer>(_songDetailViewController, "_player");
            PlayerLevelStatsData playerLevelStatsData = player.GetPlayerLevelStatsData(difficultyBeatMap.level.levelID, difficulty);
            _nextRankText.text = GetPointsToNextRank(notesCount, playerLevelStatsData.highScore);
        }

        public string GetPointsToNextRank(int notes, int highScore)
        {
            int maxPoints = ScoreController.MaxScoreForNumberOfNotes(notes);
            double percent = (double) highScore / maxPoints;
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