using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Dialogue, Overview, Playing, Paused, None
    }
    
    public static GameManager Instance { get; private set; }
    public GameState State { get => _state; set => ChangeGameState(value); }
    public event Action<GameState> GameStateChange;
    public event Action<int> TimerDecreased;
    
    public Level CurrentLevel { get; private set; }

    [SerializeField] private Level[] _levels;
    [SerializeField] private ParticleSystem _electricPaticles, _heartParticles;
    
    private GameState _state;
    private Dictionary<ItemInfo, int> _itemUses;
    private Dictionary<Level, bool> _displayDialogueOnLevels;

    private Coroutine _levelCountdown;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _state = GameState.None;
        SceneManager.sceneLoaded += OnSceneStart;

        _displayDialogueOnLevels = new();
        foreach(var level in _levels) _displayDialogueOnLevels.Add(level, true);
        
    }
    
    private void OnSceneStart(Scene s, LoadSceneMode m)
    {
        EventSystem.current.SetSelectedGameObject(null);
        
        foreach (var level in _levels)
        {
            if (level.Scene != SceneManager.GetActiveScene().name) continue;
        
            CurrentLevel = level;
            break;
        }

        _itemUses = new();
        foreach (var li in CurrentLevel.AvailableItems)
        {
            _itemUses.Add(li.Item, li.Uses);
        }

        State = CurrentLevel.DialogueIndex >= 0 && _displayDialogueOnLevels[CurrentLevel] ? GameState.Dialogue : GameState.Overview;
        _displayDialogueOnLevels[CurrentLevel] = false;
        
        if(_levelCountdown != null) StopCoroutine(_levelCountdown);
        
        HUD.Instance.Fade(true, .7f);
    }
    

    private void ChangeGameState(GameState newState)
    {
        if (newState == _state) return;
        _state = newState;
        switch (newState)
        {
            case GameState.Dialogue:
                DialogueUI.Instance.StartDialogue(CurrentLevel.DialogueIndex, OnDialogueFinished);
                
                // ANTON Musica de hablar
                MusicManager.Instance.PlayMusic("talking",true);
                break;
            case GameState.Overview:
                if(CurrentLevel.DialogueIndex < 0 && _displayDialogueOnLevels[CurrentLevel]) MusicManager.Instance.PlayMusic("escape",true);
                break;
            
            case GameState.Playing:
                _levelCountdown = StartCoroutine(LevelCountDown());
                break;
        }
        
        GameStateChange?.Invoke(_state);
        
    }

    private IEnumerator LevelCountDown()
    {
        int remainingTime = CurrentLevel.LevelTime;
        TimerDecreased?.Invoke(remainingTime);
        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(1);
            remainingTime--;
            TimerDecreased?.Invoke(remainingTime);
        }
        
        Debug.Log("gameover rata eletrocuta");
        
        // ANTON sonido de morir
        //MusicManager.Instance.PlayMusic("muere",false);
        MusicManager.Instance.PlaySound("electrocutarCorto");

        var rat = FindObjectOfType<RatController>();
        rat.PlayOneTimeAnimationXY("Shock",rat.CurrentDirection);
        ElectricParticles(rat.transform.position);
        rat.StartCoroutine(rat.Die());
    }

    private void OnDialogueFinished()
    {
        State = GameState.Overview;

        // ANTON Musica de jugar
        MusicManager.Instance.PlayMusic("escape",true);
    }


    public bool CanUse(ItemInfo item) => _itemUses[item] > 0;

    public void Use(ItemInfo item) => _itemUses[item]--;

    public int GetUses(ItemInfo item) => _itemUses[item];

    public void StopTimer()
    {
        if (_levelCountdown != null) StopCoroutine(_levelCountdown);
    }

    public void ElectricParticles(Vector2 pos) => Instantiate(_electricPaticles, pos, _electricPaticles.transform.rotation);
    public void HeartParticles(Vector2 pos) => Instantiate(_heartParticles, pos, _heartParticles.transform.rotation);

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneStart;
    }
}


