using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.BeMoBI.Scripts.Paradigm
{
    public interface IConditionController
    {
        void SetSpecificConditionPending(string conditionName, bool attempReRun = false);

        void InjectPauseTrialAfterCurrentTrial();

        void ReturnFromPauseTrial();
    }
}
