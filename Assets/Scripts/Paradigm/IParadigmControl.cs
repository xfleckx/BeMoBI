using UnityEngine;
using System.Collections;
namespace Assets.BeMoBI.Scripts
{
    public interface IParadigmControl
    {
        void StartExperimentFromBeginning(); 

        void StartExperimentWithCurrentPendingCondition();

        void InitializeCondition(string conditionName);

        void ForceABreakInstantly();

    }
}

