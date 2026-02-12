using UniRx;
using Model.Data;
using UnityEngine;

namespace Model.Logic
{
    public class SkillManager
    {
        private readonly GameModel _gameModel;
        private readonly GameData _gameData;

        // スキル発動待機状態かどうか
        public BoolReactiveProperty IsSkillActivationReady = new BoolReactiveProperty(false);

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
                IsSkillActivationReady.Value = true;
            }
        }

        // ツムを選択してスキル効果を発動した後の処理
        public void CompleteSkillActivation()
        {
            IsSkillActivationReady.Value = false;
        }
    }
}