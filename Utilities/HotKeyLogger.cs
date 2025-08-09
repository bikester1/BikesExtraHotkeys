using BikesExtraHotkey.Utilities;
using Colossal.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;
using static System.Net.Mime.MediaTypeNames;

namespace BikesExtraHotKey.Debugger
{
	public class HotKeyLogger
	{
        private ILog logger;
        public bool debugMod;
        private static List<(LogLevel, object)> LogQueue = new List<(LogLevel, object)>();
		private bool initialized = false;



		public HotKeyLogger(bool debugMod = false)
		{
			this.debugMod = debugMod;
            initialized = false;
		}

        public HotKeyLogger(ILog log, bool debugMod = false)
		{
			logger = log;
			this.debugMod = debugMod;
            initialized = true;
		}

		public void InitializeLogger(ILog log)
		{
			logger = log;
            initialized = true;
			FlushQueue();
		}

		private void FlushQueue()
		{
			foreach ((LogLevel level, object logMessage) in LogQueue)
			{
				switch (level) {
					case LogLevel.Info:
						logger.Info (logMessage);
						break;
                    case LogLevel.Warning:
                        logger.Warn(logMessage);
                        break;
                    case LogLevel.Error:
						logger.Error (logMessage);
						break;
					case LogLevel.Critical:
						logger.Critical (logMessage);
						break;
					case LogLevel.Fatal:
						logger.Fatal (logMessage);
						break;
					case LogLevel.Debug: 
						logger.Debug (logMessage);
						break;
					default:
						break;
				}
			}

			LogQueue.Clear();
		}

		public void Info(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.Log($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}

			if (initialized) {
                logger.Info(LogMessage);
            } else
			{
				LogQueue.Add((LogLevel.Info, LogMessage));
			}
			
		}

		public void InfoWithLine(object LogMessage, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
		{
			Info($"{Path.GetFileName(file)}_{member}({line}): {LogMessage}");
		}

		public void Warn(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.LogWarning($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}

            if (initialized)
            {
                logger.Warn(LogMessage);
            } else
			{
                LogQueue.Add((LogLevel.Warning, LogMessage));
            }
		}

        public void WarnWithLine(object LogMessage, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            Warn($"{Path.GetFileName(file)}_{member}({line}): {LogMessage}");
        }

        public void Error(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.LogError($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}

			if (initialized)
			{
				logger.Error(LogMessage);
			} else
			{
                LogQueue.Add((LogLevel.Error, LogMessage));
            }
		}

        public void ErrorWithLine(object LogMessage, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            Error($"{Path.GetFileName(file)}_{member}({line}): {LogMessage}");
        }

        public void Critical(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.LogError($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}

			if (initialized)
			{
				logger.Critical(LogMessage);
			} else
			{
                LogQueue.Add((LogLevel.Critical, LogMessage));
            }
		}

        public void CriticalWithLine(object LogMessage, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            Critical($"{Path.GetFileName(file)}_{member}({line}): {LogMessage}");
        }

        public void Fatal(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.LogError($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}

			if (initialized)
			{
				logger.Fatal(LogMessage);
			} else
			{
                LogQueue.Add((LogLevel.Fatal, LogMessage));
            }
		}

        public void FatalWithLine(object LogMessage, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            Fatal($"{Path.GetFileName(file)}_{member}({line}): {LogMessage}");
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical,
        Fatal
    }
}