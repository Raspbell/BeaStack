using UniRx;
using InGame.Model.Data;
using UnityEngine;

namespace InGame.Model.Logic
{
    public class SkillManager
    {
        private readonly GameModel _gameModel;
        private readonly GameData _gameData;

        public SkillManager(GameModel gameModel, GameData gameData)
        {
            _gameModel = gameModel;
            _gameData = gameData;
        }

        // スキル発動時の処理
        public void ActivateSkill()
        {
            if (_gameModel.SkillPoint.Value >= _gameData.MaxSkillPoint)
            {
                _gameModel.SkillPoint.Value = 0;
                _gameModel.IsSkillActivationReady.Value = true;
            }
        }

        // ツムを選択してスキル効果を発動した後の処理
        public void CompleteSkillActivation()
        {
            _gameModel.IsSkillActivationReady.Value = false;
        }
    }
}