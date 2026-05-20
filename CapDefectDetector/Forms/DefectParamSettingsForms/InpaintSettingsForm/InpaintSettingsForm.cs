using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapDefectDetector.Domain;
using OpenCvSharp;

namespace CapDefectDetector.Forms.DefectParamSettingsForms.InpaintSettingsForm
{
    public partial class InpaintSettingsForm : Form
    {
        private readonly DefectSettingsFormContext _context;
        private readonly InpaintSettings _settings;
        private readonly Mat _imageForTest;

        public InpaintSettingsForm(DefectSettingsFormContext context, InpaintSettings settings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _imageForTest = context.ImageForTest?.Clone() ?? throw new ArgumentNullException(nameof(context.ImageForTest));

            InitializeComponent();
        }

    }
}
