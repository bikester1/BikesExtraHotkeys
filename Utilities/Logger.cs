﻿using Colossal.Logging;
using System.Diagnostics;
using System.Reflection;

namespace BikesExtraHotKey.Debugger
{
	public class Logger
	{
		public ILog logger;
		public bool debugMod;

		public Logger(ILog log, bool debugMod = false)
		{
			logger = log;
			this.debugMod = debugMod;
		}

		public void Info(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.Log($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}
			logger.Info(LogMessage);
		}

		public void Warn(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.LogWarning($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}
			logger.Warn(LogMessage);
		}

		public void Error(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.LogError($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}
			logger.Error(LogMessage);
		}

		public void Critical(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.LogError($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}
			logger.Critical(LogMessage);
		}
		public void Fatal(object LogMessage)
		{
			if (debugMod)
			{
				MethodBase caller = new StackFrame(1, false).GetMethod();
				UnityEngine.Debug.LogError($"[{caller.DeclaringType} : {caller.Name}] {LogMessage}");
			}
			logger.Fatal(LogMessage);
		}
	}
}