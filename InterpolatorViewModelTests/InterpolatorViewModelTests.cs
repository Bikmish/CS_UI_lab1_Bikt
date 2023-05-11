using InterpolatorViewModel;
using ClassLibrary;
using Moq;
using FluentAssertions;

namespace InterpolatorViewModelTests
{
    public class InterpolatorViewModelTests
    {
        [Fact]
        public void ExecutingFailedScenario()
        {
            var er0 = new Mock<IUIServices>();
            var er1 = new Mock<IGraphicProvider>();
            var mvm = new MainViewModel(er0.Object, er1.Object);
            mvm.NmSplineNodes = 5;
            mvm.LDer = mvm.RDer = 1;
            mvm.ExecuteFromFileCommand.Execute(null);

            er0.Verify(r => r.ReportError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void SavingFailedScenario()
        {
            var er0 = new Mock<IUIServices>();
            var er1 = new Mock<IGraphicProvider>();
            var mvm = new MainViewModel(er0.Object, er1.Object);
            mvm.rd = new RawData(new double[] { 0.0, 1.0 }, 3, true, RawData.f1);
            mvm.SaveCommand.Execute(null);

            er0.Verify(r => r.ReportError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void BasicScenario()
        {
            var er0 = new Mock<IUIServices>();
            var er1 = new Mock<IGraphicProvider>();
            var mvm = new MainViewModel(er0.Object, er1.Object);
            mvm.Ends = new double[] {0.0, 1.0};
            mvm.NmNodes = 3;
            mvm.NmSplineNodes = 3;
            mvm.IsUniform = true;
            mvm.SelectedFunc = FRawEnum.Linear.ToString();
            mvm.LDer = mvm.RDer = 1;

            mvm.ExecuteFromControlsCommand.Execute(null);

            //checking for absence of error reports
            er0.Verify(r => r.ReportError(It.IsAny<string>()), Times.Never);

            //checking for correct computing raw data and spline data
            mvm.rd.Should().NotBeNull();
            mvm.sd.Should().NotBeNull();

            //checking constructed grid
            mvm.rd.NodeValues.Length.Should().Be(3);
            mvm.rd.NodeValues[0].Should().Be(5.0);
            mvm.rd.NodeValues[1].Should().Be(6.0);
            mvm.rd.NodeValues[2].Should().Be(7.0);

            //checking calculated spline coords
            mvm.sd.Items.Count.Should().Be(3);
            mvm.sd.Items[0].Coord.Should().Be(mvm.rd.NodeCoords[0]);
            mvm.sd.Items[1].Coord.Should().Be(mvm.rd.NodeCoords[1]);
            mvm.sd.Items[2].Coord.Should().Be(mvm.rd.NodeCoords[2]);

            //checking calculated spline values
            mvm.sd.Items[0].Values[0].Should().Be(mvm.rd.NodeValues[0]);
            mvm.sd.Items[1].Values[0].Should().Be(mvm.rd.NodeValues[1]);
            mvm.sd.Items[2].Values[0].Should().Be(mvm.rd.NodeValues[2]);

            //checking calculated derivatives
            mvm.sd.Items[0].Values[1].Should().Be(1.0);
            mvm.sd.Items[2].Values[1].Should().Be(1.0);
        }

        [Fact]
        public void ItemsTestScenario()
        {
            var er0 = new Mock<IUIServices>();
            var er1 = new Mock<IGraphicProvider>();
            var mvm = new MainViewModel(er0.Object, er1.Object);
            mvm.Ends = new double[] { 0.0, 1.0 };
            mvm.NmNodes = 3;
            mvm.NmSplineNodes = 7;
            mvm.IsUniform = true;
            mvm.SelectedFunc = FRawEnum.Linear.ToString();
            mvm.LDer = mvm.RDer = 1;

            mvm.ExecuteFromControlsCommand.Execute(null);

            //checking for absence of error reports
            er0.Verify(r => r.ReportError(It.IsAny<string>()), Times.Never);

            //checking for correct computing raw data and spline data
            mvm.rd.Should().NotBeNull();
            mvm.sd.Should().NotBeNull();

            //checking length of calculated lists of items
            mvm.RawItems.Should().NotBeNullOrEmpty();
            mvm.SplineItems.Should().NotBeNullOrEmpty();
            mvm.RawItems.Count.Should().Be(3);
            mvm.SplineItems.Count.Should().Be(7);
        }

        [Fact]
        public void CorrectInterpolationEndsScenario()
        {
            var er0 = new Mock<IUIServices>();
            var er1 = new Mock<IGraphicProvider>();
            var mvm = new MainViewModel(er0.Object, er1.Object);
            mvm.Ends = new double[] { 1.0, 5.0 };

            mvm["Ends"].Should().Be(String.Empty);
        }


        [Fact]
        public void UncorrectInterpolationEndsScenario()
        {
            var er0 = new Mock<IUIServices>();
            var er1 = new Mock<IGraphicProvider>();
            var mvm = new MainViewModel(er0.Object, er1.Object);
            mvm.Ends = new double[] { 1.0, 0.0 };

            mvm["Ends"].Should().NotBe(String.Empty);
        }
    }
}