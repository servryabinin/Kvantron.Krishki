using CapDefectDetector.Domain;
using OpenCvSharp;
using System;
using System.Windows.Forms;

namespace CapDefectDetector.Forms.DefectParamSettingsForms.OvalitySettingsForm
{
    public partial class OvalitySettingsForm : Form
    {
        private readonly DefectSettingsFormContext _context;
        private readonly OvalitySettings _settings;
        private readonly Mat _imageForTest;

        public OvalitySettingsForm(DefectSettingsFormContext context, OvalitySettings settings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _imageForTest = context.ImageForTest?.Clone() ?? throw new ArgumentNullException(nameof(context.ImageForTest));

            InitializeComponent();
        }
    }
}