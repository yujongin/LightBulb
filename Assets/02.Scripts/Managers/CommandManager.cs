using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CommandManager : MonoBehaviour
{
    private Dictionary<int, ICommand> stepCommands;
    private int completedStep = 0;
    public int CompletedStep { get { return completedStep; } }
    [SerializeField] private bool isDefaultSave = true;

    void Awake()
    {
        stepCommands = new Dictionary<int, ICommand>();
    }

    public void ExecuteCommand(int stepIndex)
    {
        if (stepCommands.ContainsKey(stepIndex))
        {
            stepCommands[stepIndex].Execute();
            completedStep = stepIndex;
            if (isDefaultSave)
                Managers.System.SaveStep(stepIndex);
        }
    }
    public void RedoCommand(int stepIndex)
    {
        for (int i = 1; i <= stepIndex; i++)
        {
            if (stepCommands.ContainsKey(i))
            {
                stepCommands[i].Redo();
                completedStep = i;
            }
        }
    }

    public void RegisterCommand(int stepIndex, ICommand command)
    {
        stepCommands.Add(stepIndex, command);
    }

    public T GetCommand<T>() where T : ICommand
    {
        foreach (var command in stepCommands)
        {
            if (command.Value is T)
            {
                return (T)command.Value;
            }
        }
        return default(T);
    }
    public T GetCommand<T>(int stepIndex) where T : ICommand
    {
        if (stepCommands.ContainsKey(stepIndex))
        {
            if (stepCommands[stepIndex] is T)
            {
                return (T)stepCommands[stepIndex];
            }
        }
        return default(T);
    }

#if UNITY_EDITOR
    //test code
    int index = 1;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (stepCommands.ContainsKey(index))
            {
                
                stepCommands[index].Redo();
                completedStep = index;
                index++;
            }
        }
    }
#endif
}
