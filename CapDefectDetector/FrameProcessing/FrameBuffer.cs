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
        private readonly BlockingCollection<Mat> _buffer = new();
        private bool _isDisposed;

        public Mat Get(CancellationToken token = default)
        {
            return _buffer.Take(token);
        }

        public void Put(Mat mat, CancellationToken token = default)
        {
            if (_isDisposed || _buffer.IsAddingCompleted)
            {
                mat.Dispose();
                return;
            }

            _buffer.Add(mat, token);
        }

        public void Complete()
        {
            _buffer.CompleteAdding();
        }

        public void Clear()
        {
            while (_buffer.TryTake(out Mat mat))
            {
                mat.Dispose();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _buffer.CompleteAdding();

                while (_buffer.TryTake(out Mat mat))
                {
                    mat.Dispose();
                }

                _buffer.Dispose();
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
