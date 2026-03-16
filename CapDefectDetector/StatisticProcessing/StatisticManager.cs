using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CapDefectDetector.StatisticProcessing
{
    public class StatisticsManager
    {
        private readonly DataGridView _grid;

        public StatisticsManager(DataGridView grid)
        {
            _grid = grid;
        }

        /// <summary>
        /// Добавляет запись в таблицу статистики.
        /// </summary>
        /// <param name="stat">Данные о крышке</param>
        /// <param name="okCapsSaveEnabled">Разрешено ли сохранять хорошие крышки</param>
        /// <param name="ngCapsSaveEnabled">Разрешено ли сохранять дефектные крышки</param>
        public void Add(CapStatistics stat, bool okCapsSaveEnabled, bool ngCapsSaveEnabled)
        {
            if (_grid.InvokeRequired)
            {
                _grid.BeginInvoke(() => Add(stat, okCapsSaveEnabled, ngCapsSaveEnabled));
                return;
            }

            // --- Обработка папки и имени файла ---
            string folderToShow;
            string fileNameToShow;

            bool saveDisabled = (stat.IsNg && !ngCapsSaveEnabled) || (!stat.IsNg && !okCapsSaveEnabled);

            if (saveDisabled)
            {
                folderToShow = "Сохранение отключено";
                fileNameToShow = "Сохранение отключено";
            }
            else
            {
                folderToShow = Path.Combine(
                    new DirectoryInfo(stat.SaveFolder).Name,
                    stat.IsNg ? "NG" : "OK"
                );
                fileNameToShow = stat.ImageName;
            }

            // --- Вставляем строку сверху ---
            _grid.Rows.Insert(0,
                stat.Number,
                stat.IsNg ? "NG" : "OK",
                stat.Defects,
                folderToShow,
                fileNameToShow
            );

            DataGridViewRow row = _grid.Rows[0];

            // --- Цвет фона ---
            row.DefaultCellStyle.BackColor = stat.IsNg
                ? Color.FromArgb(255, 220, 220)  // бледно красный
                : Color.FromArgb(220, 255, 220); // бледно зелёный
        }
    }
}