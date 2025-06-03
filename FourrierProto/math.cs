using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace FourrierRhythm.Math;

public class DiscreteFourrierTransfom
{

    private IEnumerable<double> SamplePoints;

    private IEnumerable<Complex> DFTResult;

    public DiscreteFourrierTransfom(IEnumerable<double> samplePoints)
    {
        SamplePoints = samplePoints;

        DFTResult = DFT(samplePoints);
    }

    // TODO: Upgrade to an fft
    private IEnumerable<Complex> DFT(IEnumerable<double> pSamplePoints)
        => pSamplePoints.Select((double value, int index) => DFTSubFormula(value, index, pSamplePoints));

    // O(n^2)
    private Complex DFTSubFormula(double value, int index, IEnumerable<double> samplePoints)
        => samplePoints
                    .Select((double x, int i) => (Complex)x * Complex.FromPolarCoordinates(1d, -index * i / samplePoints.Count()))
                    .Aggregate((Complex a, Complex b) => a + b);

    public IEnumerable<double> GetMagnetudeOfCos()
        => DFTResult.Select(a => Complex.Abs(a));
}
