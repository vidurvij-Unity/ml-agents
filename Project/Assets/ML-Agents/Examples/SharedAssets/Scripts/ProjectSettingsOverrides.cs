using UnityEngine;
using Unity.MLAgents;
using UnityEditor;
using System.IO;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// A helper class for the ML-Agents example scenes to override various
    /// global settings, and restore them afterwards.
    /// This can modify some Physics and time-stepping properties, so you
    /// shouldn't copy it into your project unless you know what you're doing.
    /// </summary>
    public class ProjectSettingsOverrides : MonoBehaviour
    {
        public GameObject agent;
        // Original values
        Vector3 m_OriginalGravity;
        float m_OriginalFixedDeltaTime;
        float m_OriginalMaximumDeltaTime;
        int m_OriginalSolverIterations;
        int m_OriginalSolverVelocityIterations;
        bool m_OriginalReuseCollisionCallbacks;

        [Header("Current Simulation Variables")]
        public int episode = 0;
        public int currentDataPoint = 1;
        public int maxEpisodes = 100;
        public int nDataPoints = 20;
        [Tooltip("Fixed rate increment between successive episodes")]
        public float fixedDeltaIncrements = .1f;

        private string filePath = "Assets/DataRecorder";
        private string fileName = "ragDollWalker.json";

        [Tooltip("Increase or decrease the scene gravity. Use ~3x to make things less floaty")]
        public float gravityMultiplier = 1.0f;

        [Header("Advanced physics settings")]
        [Tooltip("The interval in seconds at which physics and other fixed frame rate updates (like MonoBehaviour's FixedUpdate) are performed.")]
        public float fixedDeltaTime = .001f;
        [Tooltip("The maximum time a frame can take. Physics and other fixed frame rate updates (like MonoBehaviour's FixedUpdate) will be performed only for this duration of time per frame.")]
        public float maximumDeltaTime = 1.0f / 3.0f;
        [Tooltip("Determines how accurately Rigidbody joints and collision contacts are resolved. (default 6). Must be positive.")]
        public int solverIterations = 6;
        [Tooltip("Affects how accurately the Rigidbody joints and collision contacts are resolved. (default 1). Must be positive.")]
        public int solverVelocityIterations = 1;
        [Tooltip("Determines whether the garbage collector should reuse only a single instance of a Collision type for all collision callbacks. Reduces Garbage.")]
        public bool reuseCollisionCallbacks = true;

        public void Awake()
        {
            agent.GetComponent<DataRecorder>().currentTimestep = Time.fixedDeltaTime;
            // Save the original values
            m_OriginalGravity = Physics.gravity;
            m_OriginalFixedDeltaTime = Time.fixedDeltaTime;
            m_OriginalMaximumDeltaTime = Time.maximumDeltaTime;
            m_OriginalSolverIterations = Physics.defaultSolverIterations;
            m_OriginalSolverVelocityIterations = Physics.defaultSolverVelocityIterations;
            m_OriginalReuseCollisionCallbacks = Physics.reuseCollisionCallbacks;

            // Override
            Physics.gravity *= gravityMultiplier;
            Time.fixedDeltaTime = fixedDeltaTime;
            Time.maximumDeltaTime = maximumDeltaTime;
            Physics.defaultSolverIterations = solverIterations;
            Physics.defaultSolverVelocityIterations = solverVelocityIterations;
            Physics.reuseCollisionCallbacks = reuseCollisionCallbacks;

            // Make sure the Academy singleton is initialized first, since it will create the SideChannels.
            Academy.Instance.EnvironmentParameters.RegisterCallback("gravity", f => { Physics.gravity = new Vector3(0, -f, 0); });
        }


        public void AdjustSettings()
        {
            episode++;
            if (episode > maxEpisodes)
            {
                episode = 1;
                WriteToJson();
                agent.GetComponent<DataRecorder>().Reset();
                currentDataPoint++;
                if (currentDataPoint > nDataPoints)
                {
                    EditorApplication.isPlaying = false;
                }
                Time.fixedDeltaTime += fixedDeltaIncrements;
                fixedDeltaTime = Time.fixedDeltaTime;
                agent.GetComponent<DataRecorder>().currentTimestep = fixedDeltaTime;
                agent.GetComponent<DataRecorder>().epoch_no++;
            }
        }

        private void WriteToJson()
        {
            string location = Path.Combine(filePath, fileName);
            if (!System.IO.File.Exists(location))
            {
                System.IO.File.Create(location).Close();
            }

            var record = JsonUtility.ToJson(agent.GetComponent<DataRecorder>());
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(location, true))
            {
                file.WriteLine(record);
            }
        }

        public void OnDestroy()
        {
            Physics.gravity = m_OriginalGravity;
            Time.fixedDeltaTime = m_OriginalFixedDeltaTime;
            Time.maximumDeltaTime = m_OriginalMaximumDeltaTime;
            Physics.defaultSolverIterations = m_OriginalSolverIterations;
            Physics.defaultSolverVelocityIterations = m_OriginalSolverVelocityIterations;
            Physics.reuseCollisionCallbacks = m_OriginalReuseCollisionCallbacks;
        }
    }
}
