using System.Collections.Generic;
using UnityEngine;

namespace RainbowAssets.TaskList
{
    /// <summary>
    /// Represents a list of string-based tasks stored as a ScriptableObject asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Rainbow Assets/New Task List")]
    public class TaskList : ScriptableObject
    {
        /// <summary>
        /// The internal list of saved task strings.
        /// </summary>
        [SerializeField, HideInInspector] List<string> tasks = new();

        /// <summary>
        /// Gets the current list of tasks.
        /// </summary>
        /// <returns>The list of task strings.</returns>
        public List<string> GetTasks()
        {
            return tasks;
        }

        /// <summary>
        /// Adds a single task to the task list.
        /// </summary>
        /// <param name="savedTask">The task string to add.</param>
        public void AddTask(string savedTask)
        {
            tasks.Add(savedTask);
        }

        /// <summary>
        /// Replaces the current list of tasks with a new list.
        /// </summary>
        /// <param name="savedTasks">The new list of task strings.</param>
        public void AddTasks(List<string> savedTasks)
        {
            tasks.Clear();
            tasks = savedTasks;
        }
    }
}