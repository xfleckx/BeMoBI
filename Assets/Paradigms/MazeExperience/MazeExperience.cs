using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public enum TrialState { OnWayToArtefact, WayBackToStart }

public class MazeExperience : MonoBehaviour
{
    public VRSubjectController subject;
    public StartPoint startPoint;
    public VirtualRealityManager vrmanager;
    public LSLMarkerStream marker;
    public List<beMobileMaze> mazes = new List<beMobileMaze>();
    public Text InstructionPanel;
    public ArtefactController artefact;
    public Animation entrance; 

    private Stack<beMobileMaze> outStandingMazes = new Stack<beMobileMaze>();
    public TrialState trialState = TrialState.OnWayToArtefact;

    private beMobileMaze currentMaze;
    
    void Start()
    {
        subject.SetMultipleInputMethods(subject.RiftOnlyInput, subject.XBoxActions);

        startPoint.EnterStartPoint += subjectEntersStartPoint; 

        subject.ItemValueRequested += subject_ItemValueRequested;
        subject.SubmitPressed += subject_SubmitPressed;
    }

    void artefact_Approached(Collider obj)
    {
        if (trialState == TrialState.OnWayToArtefact)
        {
            artefact.ActivateLight();
            trialState = TrialState.WayBackToStart;
        }
         
    }

    void subject_SubmitPressed()
    {
        StartParadigm();
    }

    void subject_ItemValueRequested(float result)
    {
        //if(result > 0 || result < 0)
        //    Debug.Log("Value: " + result);
    }

    void Update()
    {
        if(Input.GetButton("Recenter")){
            Debug.Log("Recenter");
        }
    }

    public void StartParadigm()
    {
        mazes.Reverse();
        mazes.ForEach((m) => outStandingMazes.Push(m));

        InitTrial();
    }

    public void InitTrial()
    {
        if (outStandingMazes.Count == 0) { 
            FinishParadigm();
            return;
        }

        currentMaze = outStandingMazes.Pop();
        currentMaze.gameObject.SetActive(true);
        artefact = currentMaze.gameObject.GetComponentInChildren<ArtefactController>();
        trialState = TrialState.OnWayToArtefact;

        artefact.Approached += artefact_Approached;
        artefact.DeactivateLight();
        entrance.Play("DoorOpening");
    }

    private void FinishParadigm()
    {
        InstructionPanel.text = "Thanks for participating";
    }

    public void FinishTrial()
    { 
        entrance.Play("DoorClosing");
        StartCoroutine(WaitUntilMazeReplacement());
    }

    void subjectEntersStartPoint(Collider c)
    {
        var subject = c.GetComponent<VRSubjectController>();

        if (subject == null)
            return;

        if (trialState == TrialState.OnWayToArtefact)
        {
            // Does nothing?
        }

        if (trialState == TrialState.WayBackToStart)
        {
            Debug.Log("Entered Start Pointafter artefact activation");
            FinishTrial();
        }
    }

    IEnumerator WaitUntilMazeReplacement()
    {
       yield return new WaitForSeconds(3);
       currentMaze.gameObject.SetActive(false);
       InitTrial();
    }

    public void SetTestHeight()
    {
        subject.HeadPerspective.transform.position += new Vector3(0, 1.78f, 0);
    }
}
