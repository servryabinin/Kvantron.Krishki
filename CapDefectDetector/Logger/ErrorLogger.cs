using System;
using System.IO;
using System.Text;

namespace CapDefectDetector.Logger
{
    /// <summary>
    /// Уровни журналирования.
    /// </summary>
    public enum LogLevel
    {
        Debug   = 0,
        Info    = 1,
        Warning = 2,
        Error   = 3,
        Fatal   = 4
    }

    /// <summary>
    /// Файловый логгер с поддержкой уровней, ротацией по дате и потокобезопасной записью.
    ///
    /// ИСПРАВЛЕНО по сравнению с предыдущей версией:
    ///
    ///   1. Добавлены уровни (Debug / Info / Warning / Error / Fatal).
    ///      Прежняя версия имела только один метод Log(Exception).
    ///      Теперь можно логировать любые сообщения, не только исключения.
    ///
    ///   2. Путь к файлу вычисляется при каждой записи (lazy property).
    ///      Прежняя версия вычисляла путь один раз при инициализации класса —
    ///      если приложение работало сутки, лог писался в старый файл (старая дата).
    ///
    ///   3. Добавлена ротация: файлы старше <see cref="RetentionDays"/> дней удаляются.
    ///      Прежняя версия могла бесконечно копить логи.
    ///
    ///   4. Запись защищена lock — безопасна при вызове из нескольких потоков.
    ///      В прежней версии параллельные вызовы File.AppendAllText могли
    ///      вызвать IOException при одновременном доступе.
    ///
    ///   5. Добавлен фильтр MinLevel: в production можно отключить Debug-сообщения.
    /// </summary>
    public static class ErrorLogger
    {
        // ─── Конфигурация ────────────────────────────────────────────────────

        /// <summary>Папка для хранения лог-файлов.</summary>
        public static string LogFolder { get; set; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        /// <summary>
        /// Минимальный уровень для записи.
        /// Сообщения ниже этого уровня игнорируются.
        /// По умолчанию Info (Debug не пишется в production).
        /// </summary>
        public static LogLevel MinLevel { get; set; } = LogLevel.Info;

        /// <summary>Сколько дней хранить лог-файлы. По умолчанию 30.</summary>
        public static int RetentionDays { get; set; } = 30;

        // ─── Внутреннее состояние ────────────────────────────────────────────

        private static readonly object _fileLock = new();

        // Путь вычисляется при каждом обращении — файл меняется в полночь автоматически
        private static string CurrentLogFile =>
            Path.Combine(LogFolder, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

        // ─── Публичный API ───────────────────────────────────────────────────

        /// <summary>Записывает исключение с уровнем Error.</summary>
        public static void Log(Exception ex, string context = "")
            => LogError(ex, context);

        /// <summary>Записывает сообщение уровня Debug.</summary>
        public static void LogDebug(string message)
            => Write(LogLevel.Debug, message, null);

        /// <summary>Записывает информационное сообщение.</summary>
        public static void LogInfo(string message)
            => Write(LogLevel.Info, message, null);

        /// <summary>Записывает предупреждение.</summary>
        public static void LogWarning(string message, Exception? ex = null)
            => Write(LogLevel.Warning, message, ex);

        /// <summary>Записывает ошибку с необязательным исключением.</summary>
        public static void LogError(string message, Exception? ex = null)
            => Write(LogLevel.Error, message, ex);

        /// <summary>Записывает исключение с контекстом (обратная совместимость).</summary>
        public static void LogError(Exception ex, string context = "")
            => Write(LogLevel.Error, string.IsNullOrWhiteSpace(context) ? ex.Message : context, ex);

        /// <summary>Записывает критическую ошибку (Fatal).</summary>
        public static void LogFatal(string message, Exception? ex = null)
            => Write(LogLevel.Fatal, message, ex);

        // ─── Внутренняя логика ───────────────────────────────────────────────

        private static void Write(LogLevel level, string message, Exception? ex)
        {
            if (level < MinLevel) return;

            try
            {
                EnsureLogFolder();

                var sb = new StringBuilder();
                sb.AppendLine("──────────────────────────────────────");
                sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level.ToString().ToUpperInvariant(),-7}]");
                sb.AppendLine($"  {message}");

                if (ex is not null)
                {
                    sb.AppendLine($"  Тип : {ex.GetType().FullName}");
                    sb.AppendLine($"  Текст: {ex.Message}");

                    if (ex.InnerException is not null)
                        sb.AppendLine($"  Inner: {ex.InnerException.Message}");

                    sb.AppendLine("  Stack:");
                    foreach (var line in (ex.StackTrace ?? "").Split('\n'))
                        sb.AppendLine($"    {line.TrimEnd()}");
                }

                // lock защищает от одновременной записи из разных потоков
                lock (_fileLock)
                {
                    File.AppendAllText(CurrentLogFile, sb.ToString(), Encoding.UTF8);
                }

                // Ротация только для Error и Fatal, чтобы не замедлять Debug/Info
                if (level >= LogLevel.Error)
                    RotateOldFiles();
            }
            catch
            {
                // Если лог записать невозможно — молча продолжаем.
                // Логгер не должен ронять приложение.
            }
        }

        private static void EnsureLogFolder()
        {
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);
        }

        /// <summary>
        /// Удаляет лог-файлы старше <see cref="RetentionDays"/> дней.
        /// ИСПРАВЛЕНО: в прежней версии ротации не было — логи копились бесконечно.
        /// </summary>
        private static void RotateOldFiles()
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-RetentionDays);
                foreach (var file in Directory.GetFiles(LogFolder, "log_*.txt"))
                {
                    if (File.GetLastWriteTime(file) < cutoff)
                        File.Delete(file);
                }
            }
            catch
            {
                // Ротация не критична — игнорируем ошибки
            }
        }
    }
}
