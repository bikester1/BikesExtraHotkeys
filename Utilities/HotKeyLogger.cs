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
    /// <summary>
    /// Class <c>HotKeyLogger</c> a logging class used to allow initialization and queueing of logs prior to final logger and mod initialization.
    /// <br/>
    /// Ff the parent mod has not been initialized it will queue messages until the parent provides an instance of logger at which point it will flush queued messages.
    /// </summary>
    public class HotKeyLogger
	{
        private ILog logger;
        public bool debugMod;
        private static List<(LogLevel, object)> LogQueue = new List<(LogLevel, object)>();
		private bool initialized = false;


        /// <summary>
        /// Constructor <c>HotKeyLogger</c> construct a logger without an instance of ILog.
		/// <br/>
		/// Constuction in this manner will be set to uninitialized and all messages will be queued until InitializeLogger is called and a valid instance of ILog is provided.
		/// <br/>
		/// Once InitializeLogger is called all queued messages will be flushed to the provided logger.
        /// </summary>
        /// <param name="debugMod"></param> If set to true additional call stack info is included with log messages to help with debugging.
        public HotKeyLogger(bool debugMod = false)
		{
			this.debugMod = debugMod;
            initialized = false;
		}

		/// <summary>
		/// Constructor <c>HotKeyLogger</c> constructs a logger instance with an instance of ILog log.
		/// <br/>
		/// Construction in this manner will always be initialized and will never utilize log queue instead meassages are immediately logged when log methods are called.
		/// </summary>
		/// <param name="log"></param> Provided ILog for performing logging.
		/// <param name="debugMod"></param> If set to true additional call stack info is included with log messages to help with debugging.
        public HotKeyLogger(ILog log, bool debugMod = false)
		{
			logger = log;
			this.debugMod = debugMod;
            initialized = true;
		}

        /// <summary>
        /// Method <c>InitializeLogger</c> will assign an ILog instance, set the initialized state to true and flush the queue of messages to the provided logger.
        /// </summary>
        /// <param name="log"></param> Provided ILog for performing logging.
        public void InitializeLogger(ILog log)
		{
			logger = log;
            initialized = true;
			FlushQueue();
		}

		/// <summary>
		/// Method FlushQueue flushes all messages to the assigned logger and should only be called after initialization has complete.
		/// <br/>
		/// This is not guarenteed to be infallable so is a private method and should be used with care.
		/// </summary>
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