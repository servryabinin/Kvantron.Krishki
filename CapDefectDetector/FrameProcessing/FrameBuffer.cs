using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.FrameProcessing
{
    /// <summary>
    /// Кадровый буфер с FIFO-дисциплиной доступа к кадрам.
    /// Функция Get() блокирует вызывающий поток при отсутствии кадров в очереди
    /// </summary>
    internal class FrameBuffer : IDisposable
    {
        private readonly BlockingCollection<Mat> _buffer = [];
        private bool _isDisposed;

        public Mat Get(CancellationToken token = default)
        {
            return _buffer.Take(token);
        }

        public void Put(Mat mat, CancellationToken token = default)
        {
            _buffer.Add(mat, token);
        }

        /// <summary>
        /// Очистка всех кадров, не уничтожая сам буфер
        /// </summary>
        public void Clear()
        {
            while (_buffer.TryTake(out Mat mat))
            {
                mat.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // освобождение управляемых ресурсов и буфера
                    while (_buffer.Count > 0)
                    {
                        _buffer.Take().Dispose();
                    }
                    _buffer.Dispose();
                }

                /* если появятся неуправляемые ресурсы, освобождать их в этой
                 секции и переопределить финализатор с вызовом Dispose(false)*/

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
