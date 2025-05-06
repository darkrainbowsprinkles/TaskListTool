using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEditor.Callbacks;

namespace RainbowAssets.TaskList.Editor
{
    /// <summary>
    /// Custom Unity EditorWindow that allows users to create, view, update, and search task lists using a visual interface.
    /// </summary>
    public class TaskListEditor : EditorWindow
    {
        VisualElement container;
        ObjectField savedTasksObjectField;
        Button loadTasksButton;
        TextField taskText;
        Button addTaskButton;
        ScrollView taskListScrollView;
        TaskList currentTaskList;
        Button saveProgressButton;
        ProgressBar taskProgressBar;
        ToolbarSearchField searchBox;
        Label notificationLabel;

        /// <summary>
        /// Path to the folder containing UXML and USS files for the task list editor.
        /// </summary>
        public const string path = "Assets/Asset Packs/Rainbow Assets/Scripts/Task List/Editor/";

        /// <summary>
        /// Opens the Task List Editor window from the Unity menu.
        /// </summary>
        [MenuItem("Tools/Task List Editor")]
        public static void OpenWindow()
        {
            TaskListEditor window = GetWindow<TaskListEditor>();
            window.titleContent = new GUIContent("Task List");
        }

        /// <summary>
        /// Automatically opens the task list editor when a TaskList asset is double-clicked in the Project window.
        /// </summary>
        [OnOpenAsset]
        public static bool OnStateMachineOpened(int instanceID, int line)
        {
            TaskList taskList = EditorUtility.InstanceIDToObject(instanceID) as TaskList;

            if (taskList != null)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Initializes and builds the GUI from UXML and USS files.
        /// </summary>
        public void CreateGUI()
        {
            container = rootVisualElement;

            VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "TaskListEditor.uxml");
            container.Add(original.Instantiate());

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "TaskListEditor.uss");
            container.styleSheets.Add(styleSheet);

            savedTasksObjectField = container.Q<ObjectField>("savedTasksObjectField");
            savedTasksObjectField.objectType = typeof(TaskList);

            loadTasksButton = container.Q<Button>("loadTasksButton");
            loadTasksButton.clicked += LoadTasks;

            taskText = container.Q<TextField>("taskText");
            taskText.RegisterCallback<KeyDownEvent>(AddTask);

            addTaskButton = container.Q<Button>("addTaskButton");
            addTaskButton.clicked += AddTask;

            taskListScrollView = container.Q<ScrollView>("taskListScrollView");

            saveProgressButton = container.Q<Button>("saveProgressButton");
            saveProgressButton.clicked += SaveProgress;

            taskProgressBar = container.Q<ProgressBar>("taskProgressBar");

            searchBox = container.Q<ToolbarSearchField>("searchBox");
            searchBox.RegisterValueChangedCallback(OnSearchTextChanged);

            notificationLabel = container.Q<Label>("notificationLabel");

            UpdateNotifications("Please load a task list to continue.");
        }

        /// <summary>
        /// Creates a visual task item and attaches a progress update callback to its toggle.
        /// </summary>
        /// <param name="taskText">The text for the task item.</param>
        /// <returns>The constructed TaskItem instance.</returns>
        TaskItem CreateTask(string taskText)
        {
            TaskItem taskItem = new TaskItem(taskText);
            taskItem.GetTaskToggle().RegisterValueChangedCallback(UpdateProgress);
            return taskItem;
        }

        /// <summary>
        /// Callback to add a task when the Enter key is pressed in the text field.
        /// </summary>
        /// <param name="evt">Keyboard event data.</param>
        void AddTask(KeyDownEvent evt)
        {
            if (Event.current.Equals(Event.KeyboardEvent("Return")))
            {
                AddTask();
            }
        }

        /// <summary>
        /// Adds a new task to the scroll view and the underlying TaskList asset.
        /// </summary>
        void AddTask()
        {
            if (currentTaskList == null) return;

            if (!string.IsNullOrEmpty(taskText.value))
            {
                taskListScrollView.Add(CreateTask(taskText.value));
                SaveTask(taskText.value);
                taskText.value = "";
                taskText.Focus();
                UpdateProgress();
                UpdateNotifications("Task added successfully.");
            }
        }

        /// <summary>
        /// Saves a new task to the TaskList asset and updates the AssetDatabase.
        /// </summary>
        /// <param name="task">The task string to save.</param>
        void SaveTask(string task)
        {
            if (currentTaskList == null) return;

            currentTaskList.AddTask(task);
            EditorUtility.SetDirty(currentTaskList);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UpdateNotifications("Task added successfully.");
        }

        /// <summary>
        /// Loads tasks from the selected TaskList asset and populates the scroll view.
        /// </summary>
        void LoadTasks()
        {
            currentTaskList = savedTasksObjectField.value as TaskList;

            if (currentTaskList == null)
            {
                UpdateNotifications("Failed to load task list.");
                return;
            }

            taskListScrollView.Clear();
            List<string> tasks = currentTaskList.GetTasks();

            foreach (string task in tasks)
            {
                taskListScrollView.Add(CreateTask(task));
            }

            UpdateProgress();
            UpdateNotifications($"{currentTaskList.name} successfully loaded.");
        }

        /// <summary>
        /// Saves the current task progress by updating the task list with only the incomplete tasks.
        /// </summary>
        void SaveProgress()
        {
            if (currentTaskList == null) return;

            List<string> tasks = new List<string>();

            foreach (TaskItem task in taskListScrollView.Children())
            {
                if (!task.GetTaskToggle().value)
                {
                    tasks.Add(task.GetTaskLabel().text);
                }
            }

            currentTaskList.AddTasks(tasks);
            EditorUtility.SetDirty(currentTaskList);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            LoadTasks();
            UpdateNotifications("Progress saved successfully.");
        }

        /// <summary>
        /// Updates the progress bar based on completed tasks.
        /// </summary>
        void UpdateProgress()
        {
            if (currentTaskList == null) return;

            int count = 0;
            int completed = 0;

            foreach (TaskItem task in taskListScrollView.Children())
            {
                if (task.GetTaskToggle().value)
                {
                    completed++;
                }
                count++;
            }

            if (count > 0)
            {
                float progress = completed / (float)count;
                taskProgressBar.value = progress;
                taskProgressBar.title = $"{Mathf.Round(progress * 1000) / 10f}%";
                UpdateNotifications("Progress updated. Don't forget to save!");
            }
            else
            {
                taskProgressBar.value = 1;
                taskProgressBar.title = "100%";
            }
        }

        /// <summary>
        /// Event callback that triggers progress update when a task toggle changes.
        /// </summary>
        /// <param name="evt">The value change event.</param>
        void UpdateProgress(ChangeEvent<bool> evt)
        {
            UpdateProgress();
        }

        /// <summary>
        /// Highlights tasks that match the search query entered in the search bar.
        /// </summary>
        /// <param name="evt">The string value change event.</param>
        void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            if (currentTaskList == null) return;

            string searchText = evt.newValue.ToUpper();

            foreach (TaskItem task in taskListScrollView.Children())
            {
                string taskText = task.GetTaskLabel().text.ToUpper();

                if (!string.IsNullOrEmpty(searchText) && taskText.Contains(searchText))
                {
                    task.GetTaskLabel().AddToClassList("highlight");
                }
                else
                {
                    task.GetTaskLabel().RemoveFromClassList("highlight");
                }
            }
        }

        /// <summary>
        /// Displays a notification to the user at the bottom of the editor window.
        /// </summary>
        /// <param name="text">The notification message.</param>
        void UpdateNotifications(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                notificationLabel.text = text;
            }
        }
    }
}
