using CapDefectDetector.Logger;
using OpenCvSharp;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CapDefectDetector.Utils
{
    /// <summary>
    /// Проверяет, является ли текущий кадр дубликатом предыдущего
    /// путём сравнения нескольких горизонтальных строк изображения.
    /// </summary>
    public sealed class FrameDuplicateChecker : IDisposable
    {
        private byte[][]? _lastRows;
        private bool _hasLastRows;
        private Mat? _lastFrame;

        /// <summary>
        /// Проверяет кадр на дубликат.
        /// </summary>
        /// <param name="frame">Исходный кадр</param>
        /// <param name="cycleIndex">Номер цикла (для сохранения)</param>
        /// <returns>true если кадр является дубликатом</returns>
        public bool IsDuplicate(Mat frame, int cycleIndex)
        {
            try
            {
                using var gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

                int width = gray.Cols;
                int height = gray.Rows;

                int[] rowsY =
                {
                    height / 2 - 10,
                    height / 2,
                    height / 2 + 10
                };

                var currentRows = new byte[rowsY.Length][];

                for (int i = 0; i < rowsY.Length; i++)
                {
                    int y = rowsY[i];

                    if (y < 0 || y >= height)
                        return false;

                    currentRows[i] = new byte[width];

                    Marshal.Copy(
                        gray.Ptr(y),
                        currentRows[i],
                        0,
                        width);
                }

                if (_hasLastRows)
                {
                    bool duplicate = true;

                    for (int i = 0; i < currentRows.Length; i++)
                    {
                        if (!currentRows[i].SequenceEqual(_lastRows![i]))
                        {
                            duplicate = false;
                            break;
                        }
                    }

                    if (duplicate)
                    {
                        if (_lastFrame != null)
                        {
                            Mat first = _lastFrame.Clone();
                            Mat second = frame.Clone();

                            Task.Run(() =>
                            {
                                try
                                {
                                    CycleImageSaver.SaveDuplicate(first, cycleIndex);
                                    CycleImageSaver.SaveDuplicate(second, cycleIndex);
                                }
                                finally
                                {
                                    first.Dispose();
                                    second.Dispose();
                                }
                            });
                        }

                        return true;
                    }
                }

                _lastRows = currentRows;
                _hasLastRows = true;

                _lastFrame?.Dispose();
                _lastFrame = frame.Clone();

                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "FrameDuplicateChecker error");
                return false;
            }
        }

        public void Dispose()
        {
            _lastFrame?.Dispose();
        }
    }
}