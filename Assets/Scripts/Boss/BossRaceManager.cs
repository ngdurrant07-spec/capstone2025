using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BossRaceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GoalArea goalArea;
    [SerializeField] private BossRivalRacer rivalBoss;
    [SerializeField] private Transform playerStartPoint;
    [SerializeField] private Transform rivalStartPoint;
    [SerializeField] private PlayerScript player;

    [Header("Lose Flow")]
    [SerializeField] private bool reloadSceneOnLose = true;
    [SerializeField] private float loseDelay = 1.25f;

    [Header("Events")]
    [SerializeField] private UnityEvent onRaceStarted;
    [SerializeField] private UnityEvent onPlayerWon;
    [SerializeField] private UnityEvent onPlayerLost;

    private Coroutine loseRoutine;
    private bool raceFinished;
    private bool raceStarted;

    private void Awake()
    {
        ResolveReferences();
        RegisterGoalAreaListener();
    }

    private void Start()
    {
        StartRace();
    }

    private void OnDestroy()
    {
        UnregisterGoalAreaListener();
    }

    public void StartRace()
    {
        ResolveReferences();
        RegisterGoalAreaListener();

        raceFinished = false;
        raceStarted = true;

        if (loseRoutine != null)
        {
            StopCoroutine(loseRoutine);
            loseRoutine = null;
        }

        ResetRacersToStart();
        rivalBoss?.BeginRace();
        onRaceStarted?.Invoke();
    }

    public void HandleFinish(Collider2D finisher)
    {
        if (!raceStarted || raceFinished || finisher == null)
            return;

        BossRivalRacer rival = finisher.GetComponentInParent<BossRivalRacer>();
        if (rival != null)
        {
            HandlePlayerLoss();
            return;
        }

        PlayerScript finishedPlayer = finisher.GetComponentInParent<PlayerScript>();
        if (finishedPlayer == null)
            return;

        HandlePlayerWin(finisher);
    }

    public void HandlePlayerLoss()
    {
        if (raceFinished)
            return;

        raceFinished = true;
        rivalBoss?.StopRace();
        onPlayerLost?.Invoke();

        if (loseRoutine != null)
            StopCoroutine(loseRoutine);

        loseRoutine = StartCoroutine(HandleLossRoutine());
    }

    public void ResetRacersToStart()
    {
        if (player != null && playerStartPoint != null)
            player.RespawnAt(playerStartPoint.position);

        rivalBoss?.ResetToStart();
    }

    private void HandlePlayerWin(Collider2D finisher)
    {
        raceFinished = true;
        rivalBoss?.EnterDefeatedPose();
        TimeTrialAreaLock.UnlockTimeTrials();
        onPlayerWon?.Invoke();

        if (goalArea != null)
            goalArea.TryCompleteGoal(finisher);
    }

    private void HandleGoalAreaReached()
    {
        if (!raceStarted || raceFinished)
            return;

        raceFinished = true;
        rivalBoss?.EnterDefeatedPose();
        TimeTrialAreaLock.UnlockTimeTrials();
        onPlayerWon?.Invoke();
    }

    private IEnumerator HandleLossRoutine()
    {
        if (loseDelay > 0f)
            yield return new WaitForSeconds(loseDelay);

        if (reloadSceneOnLose)
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            yield break;
        }

        StartRace();
    }

    private void ResolveReferences()
    {
        if (goalArea == null)
            goalArea = FindFirstObjectByType<GoalArea>();

        if (player == null)
            player = FindFirstObjectByType<PlayerScript>();

        if (rivalBoss == null)
            rivalBoss = FindFirstObjectByType<BossRivalRacer>();

        if (rivalBoss != null)
            rivalBoss.SetRaceManager(this, rivalStartPoint);
    }

    private void RegisterGoalAreaListener()
    {
        if (goalArea != null)
        {
            goalArea.onGoalReached.RemoveListener(HandleGoalAreaReached);
            goalArea.onGoalReached.AddListener(HandleGoalAreaReached);
        }
    }

    private void UnregisterGoalAreaListener()
    {
        if (goalArea != null)
            goalArea.onGoalReached.RemoveListener(HandleGoalAreaReached);
    }
}
