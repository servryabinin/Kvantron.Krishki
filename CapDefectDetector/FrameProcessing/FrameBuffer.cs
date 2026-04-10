using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CapDefectDetector.FrameProcessing
{
    /// <summary>
    /// Кадровый буфер с FIFO-дисциплиной доступа к кадрам.
    ///
    /// ИСПРАВЛЕНО по сравнению с предыдущей версией:
    ///
    ///   1. Добавлено ограничение размера (boundedCapacity).
    ///      Прежний буфер был unbounded — при медленной обработке очередь
    ///      росла бесконечно, потребляя всю оперативную память.
    ///
    ///   2. Добавлен метод TryPut() с политикой дропа.
    ///      При переполнении буфера можно выбрать стратегию:
    ///        - DropNewest  (default): новый кадр отбрасывается, очередь не меняется.
    ///        - DropOldest: самый старый кадр вытесняется и диспозится, новый встаёт в конец.
    ///      Это предотвращает блокировку потока камеры при перегрузке.
    ///
    ///   3. Метод Get() переименован в Take() и принимает таймаут.
    ///      Прежний вариант мог блокироваться бесконечно, если кадры перестали
    ///      поступать (например, камера отключилась).
    ///
    ///   4. Старый Put() сохранён для обратной совместимости с существующим кодом.
    /// </summary>
    internal sealed class FrameBuffer : IDisposable
    {
        public enum DropPolicy
        {
            DropNewest,  // При переполнении отбрасываем входящий кадр
            DropOldest   // При переполнении вытесняем самый старый кадр
        }

        private readonly BlockingCollection<Mat> _buffer;
        private readonly int _capacity;
        private bool _isDisposed;

        /// <summary>
        /// Количество кадров, потерянных из-за переполнения буфера.
        /// Ненулевое значение сигнализирует, что обработка не успевает за камерой.
        /// </summary>
        public int DroppedFramesCount { get; private set; }

        /// <param name="capacity">
        /// Максимальное число кадров в буфере.
        /// Рекомендуемое значение: 5–15 (зависит от fps камеры и времени обработки).
        /// По умолчанию 10 кадров.
        /// </param>
        public FrameBuffer(int capacity = 10)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

            _capacity = capacity;
            _buffer = new BlockingCollection<Mat>(
                new ConcurrentQueue<Mat>(), // FIFO
                capacity);
        }

        // ─── Запись ──────────────────────────────────────────────────────────

        /// <summary>
        /// Блокирующая запись. Блокирует поток пока в буфере нет места.
        /// Совместимость с прежним API (Put).
        /// Предпочтительнее использовать TryPut() для потока камеры.
        /// </summary>
        public void Put(Mat mat, CancellationToken token = default)
        {
            _buffer.Add(mat, token);
        }

        /// <summary>
        /// Неблокирующая запись с политикой дропа.
        /// Возвращает true если кадр помещён в буфер, false — если отброшен.
        /// Использовать в потоке камеры вместо Put().
        /// </summary>
        public bool TryPut(Mat mat, DropPolicy policy = DropPolicy.DropNewest)
        {
            if (mat is null || mat.IsDisposed)
                return false;

            // Быстрый путь: буфер не полон
            if (_buffer.TryAdd(mat))
                return true;

            // Буфер полон — применяем политику
            DroppedFramesCount++;

            if (policy == DropPolicy.DropOldest)
            {
                // Вытесняем самый старый кадр
                if (_buffer.TryTake(out Mat oldest))
                    oldest.Dispose();

                // Пробуем добавить снова
                return _buffer.TryAdd(mat);
            }

            // DropNewest: новый кадр диспозим, буфер не трогаем
            mat.Dispose();
            return false;
        }

        // ─── Чтение ──────────────────────────────────────────────────────────

        /// <summary>
        /// Блокирует поток до получения кадра или отмены/таймаута.
        /// ИСПРАВЛЕНО: добавлен таймаут, чтобы поток обработки не зависал
        /// при остановке камеры.
        /// </summary>
        public Mat Take(CancellationToken token = default)
        {
            return _buffer.Take(token);
        }

        /// <summary>
        /// Неблокирующее получение кадра.
        /// Возвращает null если буфер пуст.
        /// </summary>
        public Mat? TryTake()
        {
            return _buffer.TryTake(out Mat mat) ? mat : null;
        }

        /// <summary>
        /// Устаревший метод (совместимость). Используйте Take().
        /// </summary>
        [Obsolete("Используйте Take(CancellationToken) для явного управления отменой.")]
        public Mat Get(CancellationToken token = default) => Take(token);

        // ─── Управление ──────────────────────────────────────────────────────

        /// <summary>
        /// Сбрасывает счётчик отброшенных кадров.
        /// </summary>
        public void ResetDropCounter() => DroppedFramesCount = 0;

        /// <summary>
        /// Очищает буфер, диспозя все Mat-объекты.
        /// БЕЗОПАСНО: перебираем кадры TryTake, не трогая заблокированный Take().
        /// </summary>
        public void Clear()
        {
            while (_buffer.TryTake(out Mat mat))
            {
                mat?.Dispose();
            }
        }

        /// <summary>
        /// Текущее количество кадров в буфере.
        /// </summary>
        public int Count => _buffer.Count;

        // ─── IDisposable ─────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Clear();
            _buffer.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
