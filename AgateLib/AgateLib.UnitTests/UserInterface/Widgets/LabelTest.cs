﻿using AgateLib.Display;
using AgateLib.Mathematics.Geometry;
using AgateLib.Tests.Fakes;
using AgateLib.UserInterface;
using AgateLib.UserInterface.Content;
using FluentAssertions;
using Microsoft.Xna.Framework;
using Moq;
using System.Linq;
using Xunit;

namespace AgateLib.Tests.UserInterface.Widgets
{
    public class LabelTest
    {
        private readonly FakeFontCore fontCore;
        private readonly Font font;
        private readonly FontProvider fontProvider;
        private readonly ContentLayoutEngine contentLayout;
        private readonly Mock<IUserInterfaceRenderContext> context;

        public LabelTest()
        {
            fontCore = new FakeFontCore("default");
            font = new Font(fontCore);

            fontProvider = new FontProvider();
            fontProvider.Add("default", font);

            contentLayout = new ContentLayoutEngine(fontProvider);

            context = CommonMocks.RenderContext(contentLayout);
        }

        [Fact]
        public void LabelBasicTextNoWrap()
        {
            string text = "These aren't the droids you're looking for.";
            int expectedWidth = 215;
            int expectedHeight = 10;

            var label = new Label(new LabelProps { Text = text });
            var labelElement = (LabelElement)label.FinalizeRendering(null);

            labelElement.Display.ParentFont = new Font(font);
            labelElement.Style.Update();

            Size idealSize = labelElement.CalcIdealContentSize(context.Object, new Size(1000, 1000));
            labelElement.Draw(context.Object, new Rectangle(40, 60, 1000, 1000));

            labelElement.Props.Text.Should().Be(text);

            idealSize.Width.Should().Be(expectedWidth);
            idealSize.Height.Should().Be(expectedHeight);

            fontCore.DrawCalls.Count.Should().Be(1);
            var drawCall = fontCore.DrawCalls[0];

            drawCall.Text.Should().Be(text);
            drawCall.Parameters.Should().Match(p => p == null || p.Count() == 0);
            drawCall.Color.Should().Be(Color.White);
            drawCall.Dest.Should().Be(new Vector2(40, 60));
        }

        [Fact]
        public void LabelBasicTextWithCarriageReturn()
        {
            string text = "This has a carriage\nreturn.";
            int expectedWidth = 95;
            int expectedHeight = 20;

            var label = new Label(new LabelProps { Text = text });
            var labelElement = (LabelElement)label.FinalizeRendering(null);
            labelElement.Display.ParentFont = font;
            labelElement.Style.Update();

            Size idealSize = labelElement.CalcIdealContentSize(context.Object, new Size(1000, 1000));
            labelElement.Draw(context.Object, new Rectangle(40, 60, 1000, 1000));

            labelElement.Props.Text.Should().Be(text);

            idealSize.Width.Should().Be(expectedWidth);
            idealSize.Height.Should().Be(expectedHeight);

            fontCore.DrawCalls.Count.Should().Be(2);
            var firstDraw = fontCore.DrawCalls[0];
            var secondDraw = fontCore.DrawCalls[1];

            firstDraw.Text.Should().Be("This has a carriage");
            firstDraw.Parameters.Should().Match(p => p == null || p.Count() == 0);
            firstDraw.Color.Should().Be(Color.White);
            firstDraw.Dest.Should().Be(new Vector2(40, 60));

            secondDraw.Text.Should().Be("return.");
            secondDraw.Parameters.Should().Match(p => p == null || p.Count() == 0);
            secondDraw.Color.Should().Be(Color.White);
            secondDraw.Dest.Should().Be(new Vector2(40, 70));
        }
    }
}
