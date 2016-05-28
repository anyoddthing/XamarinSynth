using System;
using System.Threading;

namespace SynthTest
{
    public class Logger
    {
        public enum Level
        {
            DEBUG=1, INFO, ERROR 
        }

        public const Level DEBUG = Level.DEBUG;
        public const Level INFO = Level.INFO;
        public const Level ERROR = Level.ERROR;

        public static Logger Debug<T>()
        {
            return new Logger(typeof(T), DEBUG);
        }

        public static Logger Debug(Type type)
        {
            return new Logger(type, DEBUG);
        }

        public static Logger Info<T>()
        {
            return new Logger(typeof(T), INFO);
        }

        public static Logger Info(Type type)
        {
            return new Logger(type, INFO);
        }

        public static Logger Error<T>()
        {
            return new Logger(typeof(T), ERROR);
        }

        public static Logger Error(Type type)
        {
            return new Logger(type, ERROR);
        }

        Level _level;
        Type _category;

        public Logger(Type category) : this(category, Level.INFO) {}

        public Logger(Type category, Level level)
        {
            _category = category;
            _level = level;
        }

        public void Debug(String message, params object[] objects)
        {
            Log(Level.DEBUG, message, objects);
        }

        public void Info(String message, params object[] objects)
        {
            Log(Level.INFO, message, objects);
        }

        public void Error(String message, params object[] objects)
        {
            Log(Level.ERROR, message, objects);
        }

        public void Log(Level level, String message, params object[] objects)
        {
            if (level >= _level)
                Console.WriteLine("[" + Thread.CurrentThread.Name + "] " + _category.ToString() + " " + message, objects);
        }
    }
}

