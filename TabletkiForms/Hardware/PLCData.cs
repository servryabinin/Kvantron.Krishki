using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KrishkiForms.Hardware
{
    internal class PLCData
    {
        /// <summary>
        /// Регистр состояния качества, в который отправлются данные
        /// после обработки
        /// </summary>
        public const int QualityRegisterModbus = 16465;

        /// <summary>
        /// Возможные состояния статуса качества для программы в ПЛК.
        /// Принятое в ПЛК значение определяет необходимость включения сбрасывателя
        /// </summary>
        public enum QualityStatus : int
        {
            DefaultValue = 0,
            Good = 1,
            Bad = 2
        }
    }
}
