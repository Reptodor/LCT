using NUnit.Framework;
using UnityEngine;

public class SafeAreaFitterTests
{
    [Test]
    public void ToAnchorRect_MapsFullScreenToUnitRect()
    {
        var safe = new Rect(0, 0, 1080, 1920);
        var anchors = SafeAreaFitter.ToAnchorRect(safe, new Vector2(1080, 1920));

        Assert.AreEqual(0f, anchors.xMin, 0.0001f);
        Assert.AreEqual(0f, anchors.yMin, 0.0001f);
        Assert.AreEqual(1f, anchors.xMax, 0.0001f);
        Assert.AreEqual(1f, anchors.yMax, 0.0001f);
    }

    [Test]
    public void ToAnchorRect_InsetsNotchAndHomeIndicator()
    {
        var safe = new Rect(0, 80, 1080, 1760);
        var anchors = SafeAreaFitter.ToAnchorRect(safe, new Vector2(1080, 1920));

        Assert.AreEqual(0f, anchors.xMin, 0.0001f);
        Assert.AreEqual(80f / 1920f, anchors.yMin, 0.0001f);
        Assert.AreEqual(1f, anchors.xMax, 0.0001f);
        Assert.AreEqual(1840f / 1920f, anchors.yMax, 0.0001f);
    }

    [Test]
    public void ToAnchorRect_ReturnsFullWhenScreenInvalid()
    {
        var anchors = SafeAreaFitter.ToAnchorRect(new Rect(0, 0, 100, 100), Vector2.zero);
        Assert.AreEqual(0f, anchors.xMin);
        Assert.AreEqual(1f, anchors.xMax);
    }

    [Test]
    public void ToAnchorRect_InsetsAndroidPunchHole()
    {
        var safe = new Rect(48, 0, 1032, 1920);
        var anchors = SafeAreaFitter.ToAnchorRect(safe, new Vector2(1080, 1920));

        Assert.AreEqual(48f / 1080f, anchors.xMin, 0.0001f);
        Assert.AreEqual(0f, anchors.yMin, 0.0001f);
        Assert.AreEqual(1f, anchors.xMax, 0.0001f);
        Assert.AreEqual(1f, anchors.yMax, 0.0001f);
    }
}
