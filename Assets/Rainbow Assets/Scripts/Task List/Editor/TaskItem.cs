using UnityEditor;
using UnityEngine.UIElements;

namespace RainbowAssets.TaskList.Editor
{
    /// <summary>
    /// Represents a visual UI element for displaying an individual task item in the task list editor.
    /// </summary>
    public class TaskItem : VisualElement
    {
        /// <summary>
        /// Toggle UI element used to mark the task as completed or not.
        /// </summary>
        Toggle taskToggle;

        /// <summary>
        /// Label UI element used to display the task text.
        /// </summary>
        Label taskLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskItem"/> class using the provided task text.
        /// </summary>
        /// <param name="taskText">The text of the task to display.</param>
        public TaskItem(string taskText)
        {
            VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TaskListEditor.GetPath() + "TaskItem.uxml");
            this.Add(original.Instantiate());

            taskToggle = this.Q<Toggle>();
            taskLabel = this.Q<Label>();

            taskLabel.text = taskText;
        }

        /// <summary>
        /// Gets the toggle component of the task item.
        /// </summary>
        /// <returns>The toggle UI element.</returns>
        public Toggle GetTaskToggle()
        {
            return taskToggle;
        }

        /// <summary>
        /// Gets the label component of the task item.
        /// </summary>
        /// <returns>The label UI element.</returns>
        public Label GetTaskLabel()
        {
            return taskLabel;
        }
    }
}