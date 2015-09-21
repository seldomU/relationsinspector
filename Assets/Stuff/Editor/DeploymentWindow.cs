using UnityEditor;
using UnityEngine;

namespace Stuff
{
    class DeploymentWindow : EditorWindow
    {
        void OnGUI()
        {
            ShowTestProjOptions();
            GUILayout.Space(10);
            ShowDeployBackendOptions();
        }

        void ShowTestProjOptions()
        {
            foreach (var projName in DeployToTestProjects.testProjNames)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(projName);

                if (GUILayout.Button("update RI"))
                    DeployToTestProjects.DeployRI(projName);

                if (GUILayout.Button("open"))
                    DeployToTestProjects.Open(projName);

                GUILayout.EndHorizontal();
            }
        }

        void ShowDeployBackendOptions()
        {
            GUILayout.Label("deploy");
            selectedBackend = EditorGUILayout.Popup("backend", selectedBackend, DeployToTestProjects.backendDirNames);
            GUILayout.Label("to");
            selectedProj = EditorGUILayout.Popup("proj", selectedProj, DeployToTestProjects.testProjNames);
            if (GUILayout.Button("engage!"))
                DeployToTestProjects.DeployBackend(
                    DeployToTestProjects.testProjNames[selectedProj],
                    DeployToTestProjects.backendDirNames[selectedBackend]
                    );
        }

        int selectedBackend;
        int selectedProj;

        [MenuItem("Window/Deployment")]
        static void Spawn()
        {
            GetWindow<DeploymentWindow>();
        }
    }
}
