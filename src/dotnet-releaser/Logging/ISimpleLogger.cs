﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using Spectre.Console.Rendering;

namespace DotNetReleaser.Logging;

public interface ISimpleLogger
{
    bool HasErrors { get; }

    void LogSimple(LogLevel level, Exception? exception, string? message, bool markup, params object?[] args);
}

public static class SimpleLoggerExtensions
{
    public static void Info(this ISimpleLogger log, string message) =>
        log.LogSimple(LogLevel.Information, null, message, false);
    
    public static void Warn(this ISimpleLogger log, string message) =>
        log.LogSimple(LogLevel.Warning, null, message, false);

    public static void Error(this ISimpleLogger log, string message) =>
        log.LogSimple(LogLevel.Error, null, message, false);

    public static void InfoMarkup(this ISimpleLogger log, string message) =>
        log.LogSimple(LogLevel.Information, null, message, true);

    public static void InfoMarkup(this ISimpleLogger log, string message, params IRenderable[] renderable) =>
        log.LogSimple(LogLevel.Information, null, message, true, renderable);
    
    public static void WarnMarkup(this ISimpleLogger log, string message) =>
        log.LogSimple(LogLevel.Warning, null, message, true);

    public static void ErrorMarkup(this ISimpleLogger log, string message) =>
        log.LogSimple(LogLevel.Error, null, message, true);
}


public static class SimpleLogger
{
    public static ISimpleLogger CreateConsoleLogger(ILoggerFactory factory, string? appName = null)
    {
        return new SimpleLoggerRedirect(factory.CreateLogger(appName ?? Assembly.GetEntryAssembly()?.FullName ?? Process.GetCurrentProcess().ProcessName));
    }
    
    private class SimpleLoggerRedirect : ISimpleLogger
    {
        private readonly ILogger _log;
        private int _logId;

        public SimpleLoggerRedirect(ILogger log)
        {
            _log = log;
        }

        public bool HasErrors { get; private set; }

        public void LogSimple(LogLevel level, Exception? exception, string? message, bool markup, params object?[] args)
        {
            if (level == LogLevel.Error) HasErrors = true;
            var id = Interlocked.Increment(ref _logId);
            if (markup)
            {
                _log.LogMarkup(level, new EventId(id), exception, message, args);
            }
            else
            {
                _log.Log(level, new EventId(id), exception, message, args);
            }
        }
    }
}


