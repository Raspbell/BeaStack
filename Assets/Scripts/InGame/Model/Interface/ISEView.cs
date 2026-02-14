using UnityEngine;

namespace InGame.Model.Interface
{
    public interface ISEView
    {
        void PlayDeletedSound();
        void PlaySelectedSound();
        void PlayExplosionSound();
        void PlaySkillChargedSound();
        void PlaySkillActivatedSound();
        void PlaySkillUsedSound();
        void PlayGameOverSound();

        void SetVolume(float volume);
    }
}