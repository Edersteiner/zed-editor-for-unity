using System.Text;
using NiceIO;
using Unity.CodeEditor;
using UnityEngine;

namespace UnityZed
{
    public class ZedProcess
    {
        private static readonly ILogger sLogger = ZedLogger.Create();
        private NPath m_ExecPath;
        private readonly NPath m_ProjectPath;

        public ZedProcess(string execPath)
        {
            m_ExecPath = string.IsNullOrEmpty(execPath) ? null : new NPath(execPath);
            m_ProjectPath = new NPath(Application.dataPath).Parent;
        }

        public bool OpenProject(string filePath = "", int line = -1, int column = -1)
        {
            sLogger.Log("OpenProject");

            // always add project path
            var args = new StringBuilder($"\"{m_ProjectPath}\" ");

            // if file path is provided, add it too
            if (!string.IsNullOrEmpty(filePath))
            {
                args.Append($"\"{filePath}\"");

                if (line >= 0)
                {
                    args.Append(":");
                    args.Append(line);

                    if (column >= 0)
                    {
                        args.Append(":");
                        args.Append(column);
                    }
                }
            }

            // Ensure we have a valid executable path. If not, try to discover one.
            if (m_ExecPath == null || !m_ExecPath.FileExists())
            {
                sLogger.LogFormat(LogType.Warning, null, "Configured Zed executable not found: {0}", m_ExecPath == null ? "(null)" : m_ExecPath.ToString());

                var discovery = new ZedDiscovery();
                var installs = discovery.GetInstallations();
                if (installs != null && installs.Length > 0)
                {
                    m_ExecPath = new NPath(installs[0].Path);
                    sLogger.LogFormat(LogType.Log, null, "Discovered Zed executable: {0}", m_ExecPath.ToString());
                }
                else
                {
                    sLogger.LogFormat(LogType.Error, null, "Could not find a Zed installation to open the project.");
                    return false;
                }
            }

            var exec = m_ExecPath.ToString();
            // Unconditional debug log so users can see what we attempt to run in the Editor Console.
            UnityEngine.Debug.Log($"ZedProcess.OpenProject: exec='{exec}' args='{args}'");
            var result = CodeEditor.OSOpenFile(exec, args.ToString());
            sLogger.LogFormat(LogType.Log, null, "OSOpenFile('{0}', '{1}') => {2}", exec, args.ToString(), result);
            return result;
        }
    }
}
