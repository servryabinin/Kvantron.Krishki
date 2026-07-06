using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDefectDetector.DTO.PrSettings
{
    public class BreakingSettings
    {
        /// <summary>
        /// Диметр колеса энкодера, мм
        /// </summary>
        public int DiameterEncoderWheel { get; set; } = 48;

        /// <summary>
        /// Разрядность энкодера
        /// </summary>
        public int EncoderBitrate { get; set; } = 600;

        /// <summary>
        /// Расстояние от датчика до камеры, мм
        /// </summary>
        public int DistanceFromSensorToCamera { get; set; } = 50;

        /// <summary>
        /// Расстояние от датчика до отбраковщика, мм
        /// </summary>
        public int DistanceFromSensorToBreaker { get; set; } = 330;
    }
}
