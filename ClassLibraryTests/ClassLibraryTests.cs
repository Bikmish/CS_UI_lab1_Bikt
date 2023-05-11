using ClassLibrary;
using FluentAssertions;

namespace ClassLibraryTests
{
    public class ClassLibraryTests
    {
        [Fact]
        public void RawDataGridTest()
        {
            var rd = new RawData(new double[] { 0.0, 1.0 }, 3, true, RawData.f1);
            
            rd.NodeCoords.Length.Should().Be(3);
            rd.NodeCoords[0].Should().Be(0.0);
            rd.NodeCoords[1].Should().Be(0.5);
            rd.NodeCoords[2].Should().Be(1.0);
        }

        [Fact]
        public void RawDataValuesTest()
        {
            var rd = new RawData(new double[] { 0.0, 1.0 }, 3, true, RawData.f1);
            //f1 is i + 5
            rd.NodeValues.Length.Should().Be(3);
            rd.NodeValues[0].Should().Be(5.0); //0+5
            rd.NodeValues[1].Should().Be(6.0); //1+5
            rd.NodeValues[2].Should().Be(7.0); //2+5
        }

        [Fact]
        public void RawDataSaveLoadTest()
        {
            var rd = new RawData(new double[] { 0.0, 1.0 }, 3, true, RawData.f1);
            var path = "..\\savingtest";
            var saving = rd.Save(path);

            var rd_copy = new RawData();
            var loading = RawData.Load(path, rd_copy);

            saving.Should().BeTrue();
            loading.Should().BeTrue();

            rd_copy.Should().NotBeNull();
            rd_copy.NodeCoords.Length.Should().Be(rd.NodeCoords.Length);
            rd_copy.NodeValues.Length.Should().Be(rd.NodeValues.Length);

            var zipValues = rd.NodeValues.Zip(rd_copy.NodeValues);
            var zipCoords = rd.NodeCoords.Zip(rd_copy.NodeCoords);
            zipValues.Any(x => x.First - x.Second == 0).Should().BeTrue(); //checking if every value equals to copy value
            zipCoords.Any(x => x.First - x.Second == 0).Should().BeTrue(); //checking coords by the same way
        }

        [Fact]
        public void SplineDataTest()
        {
            var rd = new RawData(new double[] { 0.0, 1.0 }, 3, true, RawData.f1);
            var sd = new SplineData(rd, new double[] { 1.0, 1.0 }, 3);
            sd.Interpolate().Should().BeTrue();

            //checking calculated spline coords
            sd.Items.Count.Should().Be(3);
            sd.Items[0].Coord.Should().Be(rd.NodeCoords[0]);
            sd.Items[1].Coord.Should().Be(rd.NodeCoords[1]);
            sd.Items[2].Coord.Should().Be(rd.NodeCoords[2]);

            //checking calculated spline values
            sd.Items[0].Values[0].Should().Be(rd.NodeValues[0]);
            sd.Items[1].Values[0].Should().Be(rd.NodeValues[1]);
            sd.Items[2].Values[0].Should().Be(rd.NodeValues[2]);

            //checking calculated derivatives
            sd.Items[0].Values[1].Should().Be(1.0);
            sd.Items[2].Values[1].Should().Be(1.0);
        }

        [Fact]
        public void SplineDataCrashTest()
        {
            var rd = new RawData(new double[] { 0.0, 1.0 }, 3, true, RawData.f1);
            var sd = new SplineData(null, new double[] { 1.0, 1.0 }, 3);
            Action act = () => sd.Interpolate();
            act.Should().Throw<NullReferenceException>();
        }
    }
}