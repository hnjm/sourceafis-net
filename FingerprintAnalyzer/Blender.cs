using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SourceAFIS.Visualization;

namespace FingerprintAnalyzer
{
    class Blender
    {
        public LogCollector Logs;

        public struct ExtractionOptions
        {
            public bool OriginalImage;
            public bool Equalized;
            public bool Contrast;
        }

        public ExtractionOptions Probe;

        public Bitmap OutputImage;

        public void Blend()
        {
            ColorF[,] output;
            if (Probe.Equalized)
            {
                float[,] inverted = GrayscaleInverter.GetInverted(Logs.Probe.Equalized);
                GlobalContrast.Normalize(inverted);
                output = PixelFormat.ToColorF(inverted);
            }
            else if (Probe.OriginalImage)
                output = PixelFormat.ToColorF(Logs.Probe.InputImage);
            else
            {
                output = new ColorF[Logs.Probe.InputImage.GetLength(0), Logs.Probe.InputImage.GetLength(1)];
                for (int y = 0; y < output.GetLength(0); ++y)
                    for (int x = 0; x < output.GetLength(1); ++x)
                        output[y, x] = new ColorF(1, 1, 1, 1);
            }

            if (Probe.Contrast)
            {
                float[,] contrast = BlockFiller.FillCornerAreas(PixelFormat.ToFloat(Logs.Probe.BlockContrast), Logs.Probe.Blocks);
                ColorF high = new ColorF(0, 1, 0, 0.25f);
                ColorF low = new ColorF(1, 0, 0, 0.25f);
                AlphaLayering.Layer(output, ScalarColoring.Interpolate(contrast, low, high));
            }

            OutputImage = ImageIO.CreateBitmap(PixelFormat.ToColorB(output));
        }
    }
}